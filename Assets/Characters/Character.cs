﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tactics.Grid;

namespace Tactics.Characters {

    public enum State { IDLE, MOVING, ATTACKING, FINISHED }

    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(WeaponSystem))]
    [SelectionBase]
    public class Character : MonoBehaviour {

        public const string PLAYER = "Player";
        public const string ENEMY = "Enemy";

        private enum ActionType { MOVE, ACTION }

        private class ActionQueue {

            List<Cell>[] characterMovementsInQueue;
            IEnumerator[] actionQueue;
            ActionType[] actionTypes;
            int queueSize;
            int currentIndex; //Points to the index after the last action

            public ActionQueue(int _queueSize) {
                queueSize = _queueSize + 1; //+ 1 because it's that many moves, plus an extra for action
                actionQueue = new IEnumerator[queueSize];
                characterMovementsInQueue = new List<Cell>[queueSize];
                actionTypes = new ActionType[queueSize];
                currentIndex = 0;
            }

            public void QueueAction(IEnumerator action, List<Cell> newMovementPath, ActionType actionType) {
                if (currentIndex >= queueSize)
                    throw new System.Exception("Adding to Action Queue when it's full");
                actionQueue[currentIndex] = action;
                actionTypes[currentIndex] = actionType;
                characterMovementsInQueue[currentIndex++] = newMovementPath;
            }

            public IEnumerator DequeueBackAction() {
                if (IsEmpty())
                    throw new System.Exception("Dequeuing Back Action Queue when it's empty");
                actionQueue[--currentIndex] = null;
                characterMovementsInQueue[currentIndex] = null;
                // Don't need to worry about the actiontypes since it's an enum type
                return actionQueue[currentIndex];
            }

            public IEnumerator DequeueFrontAction() {
                if (IsEmpty())
                    throw new System.Exception("Dequeuing Front Action Queue when it's empty");
                IEnumerator action = actionQueue[0];
                for (int i = 1; i < currentIndex; i++) {
                    actionQueue[i - 1] = actionQueue[i];
                    characterMovementsInQueue[i - 1] = characterMovementsInQueue[i];
                    actionTypes[i - 1] = actionTypes[i];
                }
                actionQueue[--currentIndex] = null;
                characterMovementsInQueue[currentIndex] = null;
                // Don't need to worry about the actiontypes since it's an enum type
                return action;
            }

            public List<List<Cell>> GetMovementPaths() {
                List<List<Cell>> movementPaths = new List<List<Cell>>();
                for (int i = 0; i < currentIndex; i++) {
                    movementPaths.Add(characterMovementsInQueue[i]);
                }
                return movementPaths;
            }

            public ActionType GetBackActionType() {
                return actionTypes[currentIndex - 1];
            }

            public Cell GetLastQueuedLocation() {
                if (IsEmpty()) { return null; }

                List<Cell> lastMovementPath = characterMovementsInQueue[currentIndex - 1];
                Cell projectedCell = lastMovementPath[lastMovementPath.Count - 1];
                return projectedCell;
            }

            public bool IsEmpty() {
                return currentIndex == 0;
            }
        }

        ActionQueue actionQueue;

        private LayerMask CELL_LAYER_MASK;
        
        private Cell currentLocation = null;
        public Cell GetCellLocation() {
            //Cell projectedCell = actionQueue.GetProjectedLocation();
            //return projectedCell ? projectedCell : getCellBelowCharacter();
            return currentLocation;
        }
        private Cell GetProjectedLocation() {
            Cell projectedCell = actionQueue.GetLastQueuedLocation();
            return projectedCell ? projectedCell : getCellBelowCharacter();
        }
        public Cell LinkToNewCellLocation(Cell newCell) {
            if (currentLocation) {
                currentLocation.clearCharacterOnCell();
            }
            currentLocation = newCell;
            currentLocation.setNewCharacterOnCell(this);
            print(gameObject.name + " is now at: " + currentLocation.gameObject.name);
            resetPossibleMovementLocations();
            weaponSystem.ResetTargets();
            return newCell;
        }

        //TODO integrate this
        private ThirdPersonCharacter thirdPersonCharacter;
        [SerializeField] private AnimatorOverrideController animatorOverride;
        public AnimatorOverrideController GetOverrideController() { return animatorOverride; }

        private State characterState = State.IDLE;
        [SerializeField] private int maxMovePoints = 2;
        [SerializeField] private int maxActionPoints = 2;
        private int numMovesLeft;
        private int numActionsLeft;


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
            numMovesLeft = maxMovePoints;
            numActionsLeft = maxActionPoints;
            actionQueue = new ActionQueue(maxMovePoints + maxActionPoints);
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
        }

        private bool cellHasEnemy(Cell cell) {
            return cell.GetCharacterOnCell() != null && cell.GetCharacterOnCell() != this;
        }

        // Resets all characters possible movement locations to reflect a possible change in this character
        private void resetPossibleMovementLocations() {
            Character[] allCharacters = GameObject.FindObjectsOfType<Character>();
            foreach (Character character in allCharacters) {
                if (!character.GetCellLocation()) {
                    continue;
                }
                character.currentPossibleMovementLocations = GridSpace.GetPossibleMovementLocations(character.GetCellLocation(), character.travelDistance + character.movementOffsetModifier);
                //currentPossibleMovementLocations = GridSpace.GetPossibleMovementLocations(currentLocation, travelDistance + movementOffsetModifier);
            }
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
        }
        
        public HashSet<Character> GetTargetsInRange() {
            return weaponSystem.GetTargets_BasicMelee();
        }

        // ---------------------------------------
        // ---------- Action Queue Functions -------------
        // ---------------------------------------
        public void ExecuteActions() {
            StartCoroutine(executeActionsCoroutine());
        }

        private IEnumerator executeActionsCoroutine() {
            while (!actionQueue.IsEmpty()) {
                IEnumerator currentAction = actionQueue.DequeueFrontAction();
                yield return StartCoroutine(currentAction);
            }
            characterState = State.IDLE;
            if (numMovesLeft <= 0 && numActionsLeft <= 0) {
                characterState = State.FINISHED;
            }
        }
        
        public void QueueMovementAction(List<Cell> cellPathToMove) {
            if (!CanMove())
                throw new System.Exception("Trying to move when out of move turns");
            print("Queuing movement action");
            ActionType actionToUse = ActionType.MOVE;
            if (numMovesLeft > 0) {
                --numMovesLeft;
            }
            else {
                --numActionsLeft;
                actionToUse = ActionType.ACTION;
            }
            LinkToNewCellLocation(cellPathToMove[cellPathToMove.Count - 1]);
            actionQueue.QueueAction(moveCharacter(cellPathToMove), cellPathToMove, actionToUse);

        }

        public void QueueAttackTarget(Character target) {
            if (!GetTargetsInRange().Contains(target))
                throw new System.Exception("Trying to attack a target that's not in range");
            if (!HasActionPoints()) 
                throw new System.Exception("Trying to attack when no moves are available");
            print("Queuing attack action");
            --numActionsLeft;
            actionQueue.QueueAction(attackTarget(target), currentCellAsList(), ActionType.ACTION);
        }
        
        public void DequeueLastAction() {
            if (actionQueue.IsEmpty())
                throw new System.Exception("Dequeueing action when size is 0");
            ActionType lastActionType = actionQueue.GetBackActionType();
            if (lastActionType == ActionType.MOVE) {
                ++numMovesLeft;
            }
            else {
                ++numActionsLeft;
            }
            actionQueue.DequeueBackAction();
            // GetCellLocation looks into the actionQueue's last cell position
            LinkToNewCellLocation(GetProjectedLocation());
        }

        public List<List<Cell>> GetMovementPathsInfo() {
            return actionQueue.GetMovementPaths();
        }

        // ---------------------------------------
        // ---------- Getter Functions -------------
        // ---------------------------------------

        public bool CanMove() { return numMovesLeft > 0 || numActionsLeft > 0; }
        public bool FinishedActionQueue() { return !CanMove(); }
        public bool HasActionPoints() { return numActionsLeft > 0; }
        public bool UsedActions() {
            return numMovesLeft < maxMovePoints || numActionsLeft < maxActionPoints;
        }
        public int GetMaxMoves() { return maxMovePoints; }
        public int GetMaxActions() { return maxActionPoints; }
        public int GetNumMovesLeft() { return numMovesLeft; }
        public int GetNumActionsLeft() { return numActionsLeft; }

        public bool WithinMovementRangeOf(Cell cellToMoveTo) {
            return currentPossibleMovementLocations.costToGoThroughNode.ContainsKey(cellToMoveTo);
        }
        public bool InRangeOfTarget(Character target) { return GetTargetsInRange().Contains(target); }
        public bool CanAttackTarget(Character target) {
            return numActionsLeft > 0 && InRangeOfTarget(target);
        }
        public bool HasActionsQueued() { return !actionQueue.IsEmpty(); }
        public State GetCharacterState() { return characterState; }
        public bool IsIDLE() { return characterState == State.IDLE; }
        public bool IsFinished() { return characterState == State.FINISHED; }

        // ---------------------------------------
        // ---------- Setter Functions -------------
        // ---------------------------------------

        public void EndTurn() {
            numMovesLeft = 0;
            numActionsLeft = 0;
            characterState = State.FINISHED;
        }

        public void ResetCharacterState() {
            characterState = State.IDLE;
            numMovesLeft = maxMovePoints;
            numActionsLeft = maxActionPoints;
            weaponSystem.ResetTargets();
        }
        
    }
}