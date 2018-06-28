﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Tactics.CameraUI;
using Tactics.Grid;
using Tactics.Characters;

namespace Tactics.Controller {

    public class PlayerController : MonoBehaviour {

        //TODO get rid of navmesh and aiCharacterControl
        private GameObject player;
        //private NavMeshAgent playerAgent;
        //private AICharacterControl playerControl;
        private Character playerCharacter;
        public Character getCurrentPlayerCharacter() { return playerCharacter; }

        private bool turnFinished = false;
        public bool getTurnFinished() { return turnFinished; }

        private Cell highlighedCell;
        
        // Use this for initialization
        void Start() {
            player = GameObject.FindGameObjectWithTag("Player");
            //playerAgent = player.GetComponent<NavMeshAgent>();
            //playerControl = player.GetComponent<AICharacterControl>();
            playerCharacter = player.GetComponent<Character>();
            
            CameraRaycast cameraRaycast = Camera.main.GetComponent<CameraRaycast>();
            //cameraRaycast.mouseOverCellObservers += movePlayer;
            //cameraRaycast.mouseOverCellObservers += drawMovementLine;
            //cameraRaycast.mouseExitCellObservers += removeMovementLine;
            cameraRaycast.mouseOverCellObservers += updateHighlighedCell;
        }

        void Update() {
            checkPlayerInput();

            if (playerCharacter.GetCharacterState() == State.FINISHED) {
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
            if (Mouse.RightClicked)
                inputUndoCommand();

            if (turnFinished || playerCharacter.GetCharacterState() != State.IDLE)
                return;

            if (Mouse.LeftClicked && highlighedCell != null) 
                inputActionCommand();
        }

        private void inputActionCommand(){
            Character target = highlighedCell.getCharacterOnCell();
            if (target != null) {
                if (playerCharacter.CanAttackTarget(target)) {
                    playerCharacter.QueueAttackTarget(target);
                }
            }
            else if (playerCharacter.CanMove() && playerCharacter.isWithinMovementRangeOf(highlighedCell)) {
                List<Cell> path = GridSpace.GetPathFromLinks(playerCharacter.GetPossibleMovementLocations(), playerCharacter.GetCellLocation(), highlighedCell);
                playerCharacter.QueueMovementAction(path);
            }
        }

        private void inputUndoCommand() {
            print("Do undo command");
            if (playerCharacter.UsedActions()) {
                print("Start Dequeue");
                playerCharacter.DequeueLastAction();
            }
        }

        private void checkExecuteActions() {
            //TODO make an "executing" variable in this class that prevents this from being called multiple times
            if (Input.GetKeyDown(KeyCode.Space)) {
                //TODO consider that this won't need to start a coroutine
                StartCoroutine(playerCharacter.ExecuteActions());
            }
        }

        public List<Cell> findPathTowardsAdjacentCharacter(Cell cellWithCharacter) {
            HashSet<Cell> surroundingCells = cellWithCharacter.getAllSurroundingCells();
            /*TODO move to closeest path or another method of choosing a path
                right now, it finds a "random" path that the player can move through to the cell
             */
            foreach (Cell cell in surroundingCells) {
                if (playerCharacter.isWithinMovementRangeOf(cell)) {
                    return GridSpace.GetPathFromLinks(playerCharacter.GetPossibleMovementLocations(), playerCharacter.GetCellLocation(), cell);
                }
            }
            return null;
        }

        public void ResetCharacterTurn() {
            turnFinished = false;
            playerCharacter.ResetCharacterState();
        }

    }

}