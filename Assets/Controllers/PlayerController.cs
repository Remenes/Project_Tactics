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
            registerCharacters("Player", false);
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

            // Set the players turn to finished if all their characters are finished
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
                playerActionObservers();
            }
            if (Input.GetKeyDown(UserInput.SwitchCharacterDown)) {
                decCurrentIndex();
                playerActionObservers();
            }

            if (Input.GetKeyDown(UserInput.ExecuteActions)) {
                executeActions();
                playerActionObservers();
                return;
            }

            // These other commands require that the current character can actually move or has actions queued
            if (!currentCharacter.CanMove() && !currentCharacter.HasActionsQueued())
                return;

            //executingActions = false;
            if (Mouse.RightClicked) {
                inputUndoCommand();
                playerActionObservers();
            }

            if (Mouse.LeftClicked && highlighedCell != null) {
                inputActionCommand();
                playerActionObservers();
            }

            if (Input.GetKeyDown(UserInput.EndTurn)) {
                endTurnCommand();
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
        
    }

}