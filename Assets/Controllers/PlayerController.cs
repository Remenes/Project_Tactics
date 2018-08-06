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

        private Cell highlighedCell;

        public delegate void OnPlayerAction();
        public event OnPlayerAction PlayerActionObservers;
        public event OnPlayerAction CharactersFinishedExecutingObservers;
        public event OnPlayerAction TurnResettedObservers;
        
        // Use this for initialization
        protected override void Awake() {
            registerCharacters(PLAYER_TAG, false);
            registerCameraRaycast();
        }

        // Registers the camera raycast so that the cell that is being operated on (using the mouse) can be seen here
        private void registerCameraRaycast() {
            CameraRaycast cameraRaycast = Camera.main.GetComponent<CameraRaycast>();
            cameraRaycast.mouseOverCellObservers += updateHighlighedCell;
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

        private void updateHighlighedCell(Cell newCellLocation) {
            highlighedCell = newCellLocation;
        }

        // Checks player inputs and perform corresponding tasks 
        private void checkPlayerInput() {
            // TODO put this somewhere better
            // TODO make a keyboard input class/struct for this
            if (Input.GetKeyDown(UserInput.SwitchCharacterUp)) {
                incCurrentIndex();
                PlayerActionObservers();
            }
            if (Input.GetKeyDown(UserInput.SwitchCharacterDown)) {
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

            //executingActions = false;
            if (Mouse.RightClicked) {
                inputUndoCommand();
                PlayerActionObservers();
            }

            if (Mouse.LeftClicked && highlighedCell != null) {
                inputActionCommand();
                PlayerActionObservers();
            }

            if (Input.GetKeyDown(UserInput.EndTurn)) {
                endTurnCommand();
                PlayerActionObservers();
            }
        }

        // Perform a move or attack depending on which cell was checked
        private void inputActionCommand(){
            Character target = highlighedCell.GetCharacterOnCell();
            if (target != null) {
                if (currentCharacter.CanAttackTarget(target)) {
                    currentCharacter.QueueAttackTarget(target);
                }
            }
            else if (currentCharacter.CanMove() && 
                currentCharacter.WithinMovementRangeOf(highlighedCell)) {
                List<Cell> path = GridSpace.GetPathFromLinks(
                    currentCharacter.GetPossibleMovementLocations(),
                    currentCharacter.GetCellLocation(), highlighedCell);
                currentCharacter.QueueMovementAction(path);
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
            if (!executingActions) {
                executingActions = true;
                foreach (Character playerChar in characters) {
                    if (playerChar.HasActionsQueued()) {
                        playerChar.ExecuteActions();
                    }
                }
            }
        }
        
        // Ends the turn for the current player
        // Can only be used if the player has no actions queued
        private void endTurnCommand() {
            if (!currentCharacter.HasActionsQueued()) {
                currentCharacter.EndTurn();
            }
        }

        // Modify's ResetTurn to additionally call TurnResettedObservers so that other things may know when this turn was resetted
        public override void ResetTurn() {
            base.ResetTurn();
            TurnResettedObservers();
        }

    }

}