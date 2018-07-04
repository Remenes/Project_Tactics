using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Tactics.CameraUI;
using Tactics.Grid;
using Tactics.Characters;

namespace Tactics.Controller {

    public class PlayerController : Controller {

        private Cell highlighedCell;

        public delegate void OnPlayerAction();
        public event OnPlayerAction playerActionObservers;
        
        // Use this for initialization
        protected override void Start() {
            registerCharacters("Player");
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
                    executingActions = false;
                }
                if (executingActions) {
                    return;
                }
            }

            checkPlayerInput();

            // Set the players turn to finished if all their characters are finished
            if (compareAllCharactersStateTo(State.FINISHED)) {
                turnFinished = true;
            }
            if (!turnFinished) {
                checkExecuteActions();
            }
        }

        private void updateHighlighedCell(Cell newCellLocation) {
            highlighedCell = newCellLocation;
        }

        private void checkPlayerInput() {
            // TODO put this somewhere better
            // TODO make a keyboard input class/struct for this
            if (Input.GetKeyDown(KeyCode.Tab)) {
                incCurrentIndex();
            }

            executingActions = false;
            if (Mouse.RightClicked) {
                inputUndoCommand();
                playerActionObservers();
            }

            if (Mouse.LeftClicked && highlighedCell != null) {
                inputActionCommand();
                playerActionObservers();
            }
        }

        // Perform a move or attack depending on which cell was checked
        private void inputActionCommand(){
            Character target = highlighedCell.getCharacterOnCell();
            if (target != null) {
                if (currentCharacter.CanAttackTarget(target)) {
                    currentCharacter.QueueAttackTarget(target);
                }
            }
            else if (currentCharacter.CanMove() && 
                currentCharacter.isWithinMovementRangeOf(highlighedCell)) {
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

        // Check if the player pressed the button to execute actions, and then execute them if they did
        private void checkExecuteActions() {
            if (Input.GetKeyDown(KeyCode.Space) && !executingActions) {
                executingActions = true;
                foreach (Character playerChar in characters) {
                    if (playerChar.HasActionsQueued()) {
                        //TODO consider that this won't need to start a coroutine
                        StartCoroutine(playerChar.ExecuteActions());
                    }
                }
            }
        }
        
    }

}