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

            List<Cell>[] characterMovementsInQueue;
            IEnumerator[] actionQueue;
            int queueSize;
            int currentIndex; //Points to the index after the last action

            public ActionQueue(int _queueSize) {
                queueSize = _queueSize + 1; //+ 1 because it's that many moves, plus an extra for action
                actionQueue = new IEnumerator[queueSize];
                characterMovementsInQueue = new List<Cell>[queueSize];
                currentIndex = 0;
            }

            public void QueueAction(IEnumerator action, List<Cell> newMovementPath) {
                if (currentIndex >= queueSize)
                    throw new System.Exception("Adding to Action Queue when it's full");
                actionQueue[currentIndex] = action;
                characterMovementsInQueue[currentIndex++] = newMovementPath;
            }

            public IEnumerator DequeueBackAction() {
                if (IsEmpty())
                    throw new System.Exception("Dequeuing Back Action Queue when it's empty");
                actionQueue[--currentIndex] = null;
                characterMovementsInQueue[currentIndex] = null;
                return actionQueue[currentIndex];
            }

            public IEnumerator DequeueFrontAction() {
                if (IsEmpty())
                    throw new System.Exception("Dequeuing Front Action Queue when it's empty");
                IEnumerator action = actionQueue[0];
                for (int i = 1; i < currentIndex; i++) {
                    actionQueue[i - 1] = actionQueue[i];
                    characterMovementsInQueue[i - 1] = characterMovementsInQueue[i];
                }
                actionQueue[--currentIndex] = null;
                characterMovementsInQueue[currentIndex] = null;
                return action;
            }

            public List<List<Cell>> GetMovementPaths() {
                List<List<Cell>> movementPaths = new List<List<Cell>>();
                for (int i = 0; i < currentIndex; i++) {
                    movementPaths.Add(characterMovementsInQueue[i]);
                }
                return movementPaths;
            }

            public Cell GetProjectedLocation() {
                if (IsEmpty()) { return null; }

                List<Cell> lastMovementPath = characterMovementsInQueue[currentIndex - 1];
                return lastMovementPath[lastMovementPath.Count - 1];
            }

            public bool IsEmpty() {
                return currentIndex == 0;
            }
        }

        ActionQueue actionQueue;

        private LayerMask CELL_LAYER_MASK;
        
        private Cell currentLocation = null;
        public Cell GetCellLocation() {
            Cell projectedCell = actionQueue.GetProjectedLocation();
            return projectedCell ? projectedCell : getCellBelowCharacter();
        }
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

        // ---------------------------------------
        // ---------- Cell Methods -------------
        // ---------------------------------------

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

        private List<Cell> currentCellAsList() {
            return new List<Cell> { GetCellLocation() };
        }

        private void setMoveTarget(Cell newMoveToCell) {
            cellTarget = newMoveToCell;
        }

        private IEnumerator moveCharacter(List<Cell> movementPath) {
            characterState = State.MOVING;
            foreach (Cell targetCell in movementPath) {
                while (Vector3.Distance(transform.position, targetCell.transform.position + new Vector3(0, GridSpace.cellSize/2, 0)) > movementDistanceThreshold) {
                    setMoveTarget(targetCell);
                    yield return 0;
                }                
            }
            setMoveTarget(null);
            characterState = State.IDLE;
        }

        private bool cellHasEnemy(Cell cell)
        {
            return cell.getCharacterOnCell() != null && cell.getCharacterOnCell() != this;
        }

        public void resetPossibleMovementLocations()
        {
            currentPossibleMovementLocations = GridSpace.GetPossibleMovementLocations(currentLocation, travelDistance + movementOffsetModifier);
        }

        public bool isWithinMovementRangeOf(Cell cellToMoveTo)
        {
            return currentPossibleMovementLocations.costToGoThroughNode.ContainsKey(cellToMoveTo);
        }

        // ---------------------------------------
        // ---------- Attack Functions -------------
        // ---------------------------------------

        private IEnumerator attackTarget(Character target) {
            characterState = State.ATTACKING;
            //target.GetComponent<Health>().TakeDamage(weapon.weaponDamage);
            //TODO Maybe also sync up the WaitForSeconds
            weaponSystem.Attack_BasicMelee(target);
            yield return new WaitForSeconds(1f);
            characterState = State.FINISHED;
        }
        
        public HashSet<Character> GetTargetsInRange() {
            //TODO make strings into constants
            string oppositeTeamTag = this.gameObject.CompareTag("Enemy") ? "Player" : "Enemy";
            GameObject[] characters = GameObject.FindGameObjectsWithTag(oppositeTeamTag);
            HashSet<Character> charactersInRange = new HashSet<Character>();
            float weaponRange = weaponSystem.GetCurrentWeapon().weaponRange;
            Vector3 thisPosition = GetCellLocation().transform.position;
            
            foreach (GameObject characterObj in characters) {
                Character character = characterObj.GetComponent<Character>();
                //Position is based on the cell the character's in
                Vector3 characterPosition = character.GetCellLocation().transform.position;
                float distanceToCharacter = Vector3.Distance(characterPosition, thisPosition);
                if (distanceToCharacter <= weaponRange) {
                    charactersInRange.Add(character);
                }
            }
            return charactersInRange;
        }

        // ---------------------------------------
        // ---------- Action Queue Functions -------------
        // ---------------------------------------

        public IEnumerator ExecuteActions() {
            print("Executing...");
            while (!actionQueue.IsEmpty()) {
                IEnumerator currentAction = actionQueue.DequeueFrontAction();
                yield return StartCoroutine(currentAction);
            }
            print("...Done");
            if (numTurnsLeft <= 0) {
                characterState = State.FINISHED;
            }
        }
        
        public void QueueMovementAction(List<Cell> cellPathToMove) {
            if (!CanMove())
                throw new System.Exception("Trying to move when out of move turns");
            print("Queuing movement action");
            --numTurnsLeft;
            LinkToNewCellLocation(cellPathToMove[cellPathToMove.Count - 1]);
            actionQueue.QueueAction(moveCharacter(cellPathToMove), cellPathToMove);

        }

        public void QueueAttackTarget(Character target) {
            if (!GetTargetsInRange().Contains(target))
                throw new System.Exception("Trying to attack a target that's not in range");
            if (!CanMove()) 
                throw new System.Exception("Trying to attack when no moves are available");
            print("Queuing attack action");
            --numTurnsLeft;
            actionQueue.QueueAction(attackTarget(target), currentCellAsList());
        }

        public void DequeueLastAction() {
            if (actionQueue.IsEmpty())
                throw new System.Exception("Dequeueing action when size is 0");
            numTurnsLeft++;
            actionQueue.DequeueBackAction();
            // GetCellLocation looks into the actionQueue's last cell position
            LinkToNewCellLocation(GetCellLocation());
        }

        public List<List<Cell>> GetMovementPathsInfo() {
            return actionQueue.GetMovementPaths();
        }

        public bool CanMove() {
            return numTurnsLeft > 0;
        }

        public bool UsedActions() {
            return numTurnsLeft < maxMoves;
        }

        public bool CanAttackTarget(Character target) {
            // TODO separate moves and actions
            return numTurnsLeft > 0 && GetTargetsInRange().Contains(target);
        }
        
    }
}