using System.Collections;
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
            cameraRaycast.mouseOverCellObservers += movePlayerToNewCell;
        }

        void Update() {
            if (playerCharacter.getCharacterState() == State.FINISHED) {
                turnFinished = true;
            }
        }

        private void movePlayerToNewCell(Cell newCellLocation) {
            if (Mouse.LeftClicked) {
                Character target = newCellLocation.getCharacterOnCell();
                if (target != null) {
                    List<Cell> pathTowardsTarget = findPathTowardsAdjacentCharacter(newCellLocation);
                    if (pathTowardsTarget != null) {
                        playerCharacter.setAttackTarget(pathTowardsTarget, target);
                        playerCharacter.resetPossibleMovementLocations();
                        return;
                    }
                }
                else if (playerCharacter.isWithinMovementRangeOf(newCellLocation)) {

                    List<Cell> path = CreateGrid.getPathFromLinks(playerCharacter.getPossibleMovementLocations(), playerCharacter.getCellLocation(), newCellLocation);

                    playerCharacter.setMovementPath(path);
                    playerCharacter.resetPossibleMovementLocations();
                }
            }
        }

        public List<Cell> findPathTowardsAdjacentCharacter(Cell cellWithCharacter) {
            HashSet<Cell> surroundingCells = cellWithCharacter.getAllSurroundingCells();
            /*TODO move to closeest path or another method of choosing a path
                right now, it finds a "random" path that the player can move through to the cell
             */
            foreach (Cell cell in surroundingCells) {
                if (playerCharacter.isWithinMovementRangeOf(cell)) {
                    return CreateGrid.getPathFromLinks(playerCharacter.getPossibleMovementLocations(), playerCharacter.getCellLocation(), cell);
                }
            }
            return null;
        }

        public void resetCharacterTurn() {
            turnFinished = false;
            playerCharacter.resetCharacterState();
            playerCharacter.resetPossibleMovementLocations();
        }

    }

}