using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tactics.Grid;

namespace Tactics.Characters {

    public enum State { IDLE, MOVING, ATTACKING, FINISHED }

    [RequireComponent(typeof(Health))]
    [SelectionBase]
    public class Character : MonoBehaviour {

        private LayerMask CELL_LAYER_MASK;
        
        private Cell currentLocation = null;
        public Cell getCellLocation() { return currentLocation; }
        public void linkToNewCellLocation(Cell newCell) {
            if (currentLocation) {
                currentLocation.clearCharacterOnCell();
            }
            currentLocation = newCell;
            newCell.setNewCharacterOnCell(this);
        }
        
        //TODO integrate this
        private ThirdPersonCharacter thirdPersonCharacter;
        
        private State characterState = State.IDLE;
        public State getCharacterState() { return characterState; }
        public bool isIDLE() { return characterState == State.IDLE; }
        public void resetCharacterState() { characterState = State.IDLE; }

        [SerializeField] private float travelDistance = 5f;
        public float getMovementDistance() { return travelDistance + movementOffsetModifier; }
        private CreateGrid.movementLocationsInfo currentPossibleMovementLocations;
        public CreateGrid.movementLocationsInfo getPossibleMovementLocations() { return currentPossibleMovementLocations; }

        [SerializeField] private float movementOffsetModifier = .5f;
        [SerializeField] private float movementDistanceThreshold = .25f;

        [SerializeField] private Weapon weapon;

        private Cell moveTarget;

        // Use this for initialization
        void Start() {
            CELL_LAYER_MASK = 1 << (int) Layer.CELL_LAYER;
            thirdPersonCharacter = GetComponent<ThirdPersonCharacter>();

            StartCoroutine( setStartingCellLocation() );
        }

        void Update() {
            if (moveTarget) {
                Vector3 desiredVelocity = moveTarget.transform.position - transform.position;
                desiredVelocity.y = 0;
                desiredVelocity.Normalize();
                thirdPersonCharacter.Move(desiredVelocity, false, false);
            }
            else {
                thirdPersonCharacter.Move(Vector3.zero, false, false);
            }
        }

        private IEnumerator setStartingCellLocation() {
            while (CreateGrid.gridCreated == false) {
                yield return 0;
            }
            Cell cellBelowCharacter = null;
            while (cellBelowCharacter == null) {
                cellBelowCharacter = getCellBelowCharacter();
                yield return 0;
            }
            linkToNewCellLocation(cellBelowCharacter);
            transform.position = cellBelowCharacter.transform.position;

            resetPossibleMovementLocations();
        }

        private Cell getCellBelowCharacter() {
            Cell cellBelowCharacter = null;
            RaycastHit rayHit;
            Vector3 raycastStartLocation = transform.position + Vector3.up * .5f; //So that it will start raycasting from around the waist area, to ensure it doesn't start raycast while it's in the ground
            if (Physics.Raycast(raycastStartLocation, Vector3.down, out rayHit, 2f, CELL_LAYER_MASK)) {
                cellBelowCharacter = rayHit.collider.GetComponent<Cell>();
            }
            return cellBelowCharacter;
        }

        private void setMoveTarget(Cell newMoveToCell) {
            moveTarget = newMoveToCell;
        }

        private IEnumerator moveCharacter(List<Cell> movementPath, bool movingFinishesTurn = true) {
            characterState = State.MOVING;
            linkToNewCellLocation(movementPath[movementPath.Count - 1]);
            print("Moving");
            foreach (Cell targetCell in movementPath) {
                while (Vector3.Distance(transform.position, targetCell.transform.position + new Vector3(0, CreateGrid.cellSize/2, 0)) > movementDistanceThreshold) {
                    setMoveTarget(targetCell);
                    yield return 0;
                }                
            }
            setMoveTarget(null);
            
            if (movingFinishesTurn)
                characterState = State.FINISHED;
        }
        
        private IEnumerator moveCharacterAndAttackTarget(List<Cell> movementPath, Character target) {
            yield return StartCoroutine(moveCharacter(movementPath, movingFinishesTurn : false));
            print("Attacking: " + target.name);
            characterState = State.ATTACKING;
            target.GetComponent<Health>().TakeDamage(weapon.weaponDamage);
            yield return new WaitForSeconds(1f);
            print("Finished attacking");
            characterState = State.FINISHED;
        }

        public void setMovementPath(List<Cell> cellPathToMove) {
            StartCoroutine(moveCharacter(cellPathToMove));
        }

        public void setAttackTarget(List<Cell> movementPath, Character target) {
            StartCoroutine(moveCharacterAndAttackTarget(movementPath, target));
        }
        
        private bool cellHasEnemy(Cell cell) {
            return cell.getCharacterOnCell() != null && cell.getCharacterOnCell() != this;
        }

        public void resetPossibleMovementLocations() {
            currentPossibleMovementLocations = CreateGrid.getPossibleMovementLocations(currentLocation, travelDistance + movementOffsetModifier);
        }

        public bool isWithinMovementRangeOf(Cell cellToMoveTo) {
            return currentPossibleMovementLocations.costToGoThroughNode.ContainsKey(cellToMoveTo);
        }

    }
}