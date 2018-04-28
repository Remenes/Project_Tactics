using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tactics.Grid;

namespace Tactics.Characters {

    public enum State { IDLE, MOVING, ATTACKING, FINISHED }

    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(WeaponSystem))]
    [SelectionBase]
    public class Character : MonoBehaviour {

        private class ActionQueue {

            List<Cell>[] characterLocationInQueue;
            IEnumerator[] actionQueue;
            int queueSize;
            int currentIndex; //Points to the index after the last action

            public ActionQueue(int _queueSize) {
                //TODO account for movement and actions
                queueSize = _queueSize + 1; //+ 1 because it's that many moves, plus an extra for action
                actionQueue = new IEnumerator[queueSize];
                characterLocationInQueue = new List<Cell>[queueSize];
                currentIndex = 0;
            }

            public void QueueAction(IEnumerator action, List<Cell> newLocation) {
                if (currentIndex >= queueSize)
                    throw new System.Exception("Adding to Action Queue when it's full");
                actionQueue[currentIndex] = action;
                characterLocationInQueue[currentIndex++] = newLocation;
            }

            public IEnumerator DequeueBackAction() {
                if (IsEmpty())
                    throw new System.Exception("Dequeuing Back Action Queue when it's empty");
                actionQueue[--currentIndex] = null;
                characterLocationInQueue[currentIndex] = null;
                return actionQueue[currentIndex];
            }

            public IEnumerator DequeueFrontAction() {
                if (IsEmpty())
                    throw new System.Exception("Dequeuing Front Action Queue when it's empty");
                IEnumerator action = actionQueue[0];
                for (int i = 1; i < currentIndex; i++) {
                    actionQueue[i - 1] = actionQueue[i];
                    characterLocationInQueue[i - 1] = characterLocationInQueue[i];
                }
                actionQueue[--currentIndex] = null;
                characterLocationInQueue[currentIndex] = null;
                return action;
            }

            public List<List<Cell>> GetMovementPaths() {
                List<List<Cell>> movementPaths = new List<List<Cell>>();
                for (int i = 0; i < currentIndex; i++) {
                    movementPaths.Add(characterLocationInQueue[i]);
                }
                return movementPaths;
            }

            public bool IsEmpty() {
                return currentIndex == 0;
            }
        }

        ActionQueue actionQueue;

        private LayerMask CELL_LAYER_MASK;
        
        private Cell currentLocation = null;
        public Cell getCellLocation() { return currentLocation; }
        public Cell LinkToNewCellLocation(Cell newCell) {
            if (currentLocation) {
                currentLocation.clearCharacterOnCell();
            }
            currentLocation = newCell;
            newCell.setNewCharacterOnCell(this);
            resetPossibleMovementLocations();
            return newCell;
        }

        //TODO integrate this
        private ThirdPersonCharacter thirdPersonCharacter;
        [SerializeField] private AnimatorOverrideController animatorOverride;
        public AnimatorOverrideController GetOverrideController() { return animatorOverride; }

        private State characterState = State.IDLE;
        [SerializeField] private int maxMoves = 2;
        private int numTurnsLeft;
        //private int queuedTurnsLeft; // Used for checking how many
        public int GetMaxTurns() { return maxMoves; }
        public int GetNumTurnsLeft() { return numTurnsLeft; }
        public State GetCharacterState() { return characterState; }
        public bool isIDLE() { return characterState == State.IDLE; }
        public bool isFinished() { return characterState == State.FINISHED; }
        public void ResetCharacterState() { characterState = State.IDLE; numTurnsLeft = maxMoves; }

        [Header("Movement Modifiers")]
        [SerializeField] private float movementOffsetModifier = .5f;
        [SerializeField] private float movementDistanceThreshold = .25f;
        [SerializeField] private float travelDistance = 5f;
        public float getMovementDistance() { return travelDistance + movementOffsetModifier; }
        private GridSpace.MovementLocationsInfo currentPossibleMovementLocations;
        public GridSpace.MovementLocationsInfo GetPossibleMovementLocations() { return currentPossibleMovementLocations; }
        
        private Cell cellTarget;
        private WeaponSystem weaponSystem;


        // Use this for initialization
        void Start() {
            initializeComponents();
        }

        private void initializeComponents() {
            CELL_LAYER_MASK = 1 << (int)Layer.CELL_LAYER;
            numTurnsLeft = maxMoves;
            actionQueue = new ActionQueue(maxMoves);
            thirdPersonCharacter = GetComponent<ThirdPersonCharacter>();
            weaponSystem = GetComponent<WeaponSystem>();
            StartCoroutine(setStartingCellLocation());
        }
        
        void Update() {
            moveToCellTarget();
        }

        private void moveToCellTarget() {
            if (cellTarget) {
                Vector3 desiredVelocity = cellTarget.transform.position - transform.position;
                desiredVelocity.y = 0;
                desiredVelocity.Normalize();
                thirdPersonCharacter.Move(desiredVelocity, false, false);
            }
            else {
                thirdPersonCharacter.Move(Vector3.zero, false, false);
            }
        }

        private IEnumerator setStartingCellLocation() {
            while (GridSpace.gridCreated == false) {
                yield return 0;
            }
            Cell cellBelowCharacter = null;
            while (!cellBelowCharacter) {
                cellBelowCharacter = getCellBelowCharacter();
                yield return 0;
            }
            LinkToNewCellLocation(cellBelowCharacter);
            transform.position = cellBelowCharacter.transform.position;
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
            cellTarget = newMoveToCell;
        }

        private IEnumerator moveCharacter(List<Cell> movementPath) {
            characterState = State.MOVING;
            //numTurnsLeft--;
            //LinkToNewCellLocation(movementPath[movementPath.Count - 1]);
            foreach (Cell targetCell in movementPath) {
                while (Vector3.Distance(transform.position, targetCell.transform.position + new Vector3(0, GridSpace.cellSize/2, 0)) > movementDistanceThreshold) {
                    setMoveTarget(targetCell);
                    yield return 0;
                }                
            }

            //resetPossibleMovementLocations();
            setMoveTarget(null);
            characterState = State.IDLE;
        }

        private IEnumerator attackTarget(Character target) {
            characterState = State.ATTACKING;
            //target.GetComponent<Health>().TakeDamage(weapon.weaponDamage);
            //TODO combine animation into one function, or extract it to look nicer
            //TODO Maybe also sync up the WaitForSeconds
            weaponSystem.Attack_BasicMelee();
            yield return new WaitForSeconds(1f);
            characterState = State.FINISHED;
        }

        private bool cellHasEnemy(Cell cell) {
            return cell.getCharacterOnCell() != null && cell.getCharacterOnCell() != this;
        }

        //TODO extract this and think of a way to make this account for range units too, by maybe using the AI classes
        //TODO fix so that when your movement path isn't in reach, you can still attack if your range is long enough (Use the Enemy's AI code and put it in GridSpace)
        //TODO save this as a variable so multiple calls to this function won't be needed
        public HashSet<Character> GetTargetsInRange() {
            //TODO make strings into constants
            string oppositeTeamTag = this.gameObject.CompareTag("Enemy") ? "Player" : "Enemy";
            GameObject[] characters = GameObject.FindGameObjectsWithTag(oppositeTeamTag);
            HashSet<Character> charactersInRange = new HashSet<Character>();
            float weaponRange = weaponSystem.GetCurrentWeapon().weaponRange;
            GridSpace.MovementLocationsInfo weaponRangeAreaInfo = GridSpace.GetPossibleMovementLocations(currentLocation, weaponRange);

            foreach (GameObject characterObj in characters) {
                Character character = characterObj.GetComponent<Character>();
                Cell cellOfCharacter = character.getCellLocation();
                var costForNodes = currentPossibleMovementLocations.costToGoThroughNode;
                print("WeaponRange: " + weaponSystem.GetCurrentWeapon().weaponRange + " | " + costForNodes[cellOfCharacter]);
                if (costForNodes.ContainsKey(cellOfCharacter) && 
                    costForNodes[cellOfCharacter] < weaponSystem.GetCurrentWeapon().weaponRange) {
                    charactersInRange.Add(character);
                }
            }
            return charactersInRange;
            //HashSet<Character> targets = new HashSet<Character>();
            //foreach (Cell cell in currentLocation.getAllSurroundingCells()) {
            //    if (cell.getCharacterOnCell() != null) {
            //        targets.Add(cell.getCharacterOnCell());
            //    }
            //}
            //return targets;
        }
        public bool CanAttackTarget(Character target) { return GetTargetsInRange().Contains(target); }

        public IEnumerator ExecuteActions() {
            print("Executing...");
            while (!actionQueue.IsEmpty()) {
                IEnumerator currentAction = actionQueue.DequeueFrontAction();
                yield return StartCoroutine(currentAction);
            }
            print("...Done");
            if (numTurnsLeft <= 0) {
                if (GetTargetsInRange().Count == 0) { //Only end turn if they have no targets in range after they have no more turns
                    characterState = State.FINISHED;
                }
            }
        }
        
        public void QueueMovementAction(List<Cell> cellPathToMove) {
            if (!CanMove())
                throw new System.Exception("Trying to move when out of move turns");
            print("Queuing movement action");
            --numTurnsLeft;
            Cell newLocation = LinkToNewCellLocation(cellPathToMove[cellPathToMove.Count - 1]);
            actionQueue.QueueAction(moveCharacter(cellPathToMove), cellPathToMove);

        }

        public void QueueAttackTarget(Character target) {
            if (!GetTargetsInRange().Contains(target))
                throw new System.Exception("Trying to attack a target that's not in range");
            print("Queuing attack action");
            numTurnsLeft = 0;
            actionQueue.QueueAction(attackTarget(target), null);
        }

        public void resetPossibleMovementLocations() {
            currentPossibleMovementLocations = GridSpace.GetPossibleMovementLocations(currentLocation, travelDistance + movementOffsetModifier);
        }

        public bool isWithinMovementRangeOf(Cell cellToMoveTo) {
            return currentPossibleMovementLocations.costToGoThroughNode.ContainsKey(cellToMoveTo);
        }

        public bool CanMove() {
            return numTurnsLeft > 0;
        }

        public List<List<Cell>> GetMovementPathsInfo() {
            return actionQueue.GetMovementPaths();
        }
        
    }
}