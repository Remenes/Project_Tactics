using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Tactics.CameraUI;
using Tactics.Grid;
using Tactics.Characters;

namespace Tactics.Controller {

    public class PlayerController : Controller {
        
        public static string PLAYER_TAG = "Player";

        private Cell highlightedCell;

        public delegate void OnPlayerAction();
        public event OnPlayerAction PlayerActionObservers;
        public event OnPlayerAction CharactersFinishedExecutingObservers;
        public event OnPlayerAction TurnResettedObservers;

        public delegate void OnPlayerChangeAbility(int newAbilityIndex);
        public event OnPlayerChangeAbility PlayerChangedAbilityObservers;

        // -1 means the player is not using ability
        private int currentAbilityIndex = -1;
        public int CurrentAbilityIndex() { return currentAbilityIndex; }
        public bool IsUsingAbility() { return currentAbilityIndex != -1; }
        public int NotUsingAbilityIndex() { return -1; }
        private void resetAbilityIndex() { currentAbilityIndex = NotUsingAbilityIndex(); PlayerChangedAbilityObservers(NotUsingAbilityIndex()); }

        private Character invisibleTarget;
        // Use this for initialization
        protected override void Awake() {
            registerCharacters(PLAYER_TAG, false);
            registerCameraRaycast();
        }

        // Registers the camera raycast so that the cell that is being operated on (using the mouse) can be seen here
        private void registerCameraRaycast() {
            CameraRaycast cameraRaycast = Camera.main.GetComponent<CameraRaycast>();
            cameraRaycast.mouseOverCellObservers += updatehighlightedCell;
        }

        protected override void Update() {
            // Can't do anything if it's not their turn (*or can they????*)
            if (turnFinished)
                return;

            // Check if the actions are still executing or not and return if they are still executing actions
            if (executingActions) {
                if (charactersDoneWithActions()) {
                    // Check to see if their characters are finished right after they are done with actions, so 
                    // the appropriate turnFinished variable may be passed into the Observers
                    if (compareAllCharactersStateTo(State.FINISHED)) {
                        turnFinished = true;
                    }
                    CharactersFinishedExecutingObservers();
                    executingActions = false;
                }
                if (executingActions) {
                    return;
                }
            }

            // Set the players turn to finished if all their characters are finished. This can happen
            // when actions are not executing when the player manually ends their characters turn
            if (compareAllCharactersStateTo(State.FINISHED)) {
                turnFinished = true;
                return;
            }
            checkPlayerInput();
        }

        private void updatehighlightedCell(Cell newCellLocation) {
            if (IsUsingAbility() && GetCurrentAbility().UseMouseLocation && highlightedCell != newCellLocation) {
                updateMouseLocationAbilityTargets(newCellLocation);
            }
            highlightedCell = newCellLocation;
        }

        // For when the player is using an ability that requires a new origin via the mouse
        private void updateMouseLocationAbilityTargets(Cell newCellLocation) {
            print("Updating Mouse Location ability");
            //Vector3 topOfCell = newCellLocation.transform.position + Vector3.up * GridSpace.cellSize / 2;
            ResetTargetsOfCurrentMouseLocationAbility(newCellLocation);
        }

        // Checks player inputs and perform corresponding tasks 
        private void checkPlayerInput() {
            // These commands can be used at any time in the player's turn and that actions are not being executed
            if (Input.GetKeyDown(UserInput.SwitchCharacterUp)) {
                resetAbilityIndex();
                incCurrentIndex();
                PlayerActionObservers();
            }
            if (Input.GetKeyDown(UserInput.SwitchCharacterDown)) {
                resetAbilityIndex();
                decCurrentIndex();
                PlayerActionObservers();
            }

            if (Input.GetKeyDown(UserInput.ExecuteActions)) {
                executeActions();
                PlayerActionObservers();
                return;
            }

            // These other commands require that the current character can actually move or has actions queued
            if (!currentCharacter.CanMove() && !currentCharacter.HasActionsQueued())
                return;
            
            if (Mouse.RightClicked) {
                // TODO: intuitively, when undoing unto a move action, the ability use is resetted
                inputUndoCommand();
                PlayerActionObservers();
            }

            checkAbilityInput();
            print("Using Ability: " + currentAbilityIndex);
            if (Mouse.LeftClicked) {
                if (currentAbilityIndex == -1 && highlightedCell != null) {
                    inputActionCommand();
                    PlayerActionObservers();
                }
                else if (currentAbilityIndex != -1) {
                    inputAbilityCommand(currentAbilityIndex);
                    PlayerActionObservers();
                }
            }

            if (Input.GetKeyDown(UserInput.EndTurn)) {
                endTurnCommand();
                PlayerActionObservers();
            }
        }

        // Checks if the player has inputted ability keys and changes the currentAbilityIndex, which in turn, 
        // makes it so that the player is choosing targets for the corresponding ability
        private void checkAbilityInput() {
            for (int number = 1; number < 10; number++) {
                if (Input.GetKeyDown(KeyCode.Alpha0 + number) && number < currentCharacter.GetNumberOfAbilities() + 1 ) {
                    currentAbilityIndex = number - 1;
                    if (GetCurrentAbility().UseMouseLocation) {
                        updateMouseLocationAbilityTargets(highlightedCell);
                    }
                    PlayerChangedAbilityObservers(currentAbilityIndex);
                    //inputAbilityCommand(currentAbilityIndex);
                    //PlayerActionObservers();
                }
            }
            if (Input.GetKeyDown(KeyCode.Escape)) {
                resetAbilityIndex();
                PlayerChangedAbilityObservers(currentAbilityIndex);
            }
        }

        // Perform a move or attack depending on which cell was checked
        private void inputActionCommand(){
            Character target = highlightedCell.GetCharacterOnCell();
            if (target != null) {
                if (currentCharacter.CanAttackTarget(target)) {
                    currentCharacter.QueueAttackTarget(target);
                }
            }
            else if (currentCharacter.CanMove() && 
                currentCharacter.WithinMovementRangeOf(highlightedCell)) {
                List<Cell> path = GridSpace.GetPathFromLinks(
                    currentCharacter.GetPossibleMovementLocations(),
                    currentCharacter.GetCellLocation(), highlightedCell);
                currentCharacter.QueueMovementAction(path);
            }
        }

        private void inputAbilityCommand(int abilityIndex) {
            Character target = highlightedCell.GetCharacterOnCell();
            // If the current ability doesn't require a target, use it regardless of where the mouse is
            if (!GetCurrentAbility().RequiresTarget) {
                // If the ability uses the origin of the mouse location, however, center it on the mouse location
                if (GetCurrentAbility().UseMouseLocation) {
                    currentCharacter.QueueAbilityUse(highlightedCell, abilityIndex);
                }
                // Else, use it on the current character's cell
                else {
                    currentCharacter.QueueAbilityUse(currentCharacter.GetCellLocation(), abilityIndex);
                }
            }
            else if (currentCharacter.CanUseAbilitiesOn(target, abilityIndex)) {
                currentCharacter.QueueAbilityUse(target, abilityIndex);
            }
            // Change ability back to not using abilities if they finished all their action points
            if (!currentCharacter.HasActionPoints()) {
                resetAbilityIndex();
            }
        }
        
        // Perform an undo action if the undo command was pressed
        private void inputUndoCommand() {
            // Check if the player character actually used actions or not
            if (currentCharacter.UsedActions()) {
                currentCharacter.DequeueLastAction();
            }
        }

        // Execute all the player character's actions
        private void executeActions() {
            //if (!executingActions) {
                executingActions = true;
                resetAbilityIndex();
                foreach (Character playerChar in characters) {
                    if (playerChar.HasActionsQueued()) {
                        playerChar.ExecuteActions();
                    }
                }
            //}
        }
        
        // Ends the turn for the current player
        // Can only be used if the player has no actions queued
        private void endTurnCommand() {
            if (!currentCharacter.HasActionsQueued()) {
                currentCharacter.EndTurn();
            }
        }

        // --------------------------------------------------------------------------------------
        // ----------Getters and Setters for the current Character's selected ability------------
        // --------------------------------------------------------------------------------------

        // Call this to see if the current character can target the target character using the ability (or basic attack)
        // that is currently selected.
        public bool CurrCharacterCanTarget(Character target) {
            print("Check can target");
            if (IsUsingAbility()) {
                print("Checking using ability");
                return currentCharacter.CanUseAbilitiesOn(target, currentAbilityIndex);
            }
            return currentCharacter.CanAttackTarget(target);
        }

        // Gets the chosen ability of the current character
        public AbilityConfig GetCurrentAbility() {
            if (!IsUsingAbility())
                throw new System.Exception("Getting current ability, but no abilities are being used");
            return currentCharacter.GetAbilityConfig(currentAbilityIndex);
        }

        // Gets the targets of the chosen ability of the current character
        public HashSet<Character> GetTargetsOfCurrentAbility() {
            if (!IsUsingAbility())
                throw new System.Exception("Can't get targets of current chosen ability: no abilities being used");
            return currentCharacter.GetTargetsUsingAbility(currentAbilityIndex);
        }

        // Resets the targets of the current AOE ability() {
        public void ResetTargetsOfCurrentMouseLocationAbility(Cell newOrigin) {
            if (!IsUsingAbility())
                throw new System.Exception("Can't reset targets of current chosen ability: no abilities being used");
            currentCharacter.ResetTargetsForDifferentOriginAbility(currentAbilityIndex, newOrigin);
        }


        // Modify's ResetTurn to additionally call TurnResettedObservers so that other things may know when this turn was resetted
        public override void ResetTurn() {
            base.ResetTurn();
            resetAbilityIndex();
            TurnResettedObservers();
        }

    }

}