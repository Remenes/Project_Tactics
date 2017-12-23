using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tactics.Controller;
using Tactics.Grid;

namespace Tactics.CameraUI {

    public class PlayerControlUI : MonoBehaviour {

        LineRenderer movementLine;
        PlayerController playerControl;
        
        private Cell currentCell;
        private Color? initialCellColor = null;

        [SerializeField] EnemyController enemyControl;

        // Use this for initialization
        void Start() {
            movementLine = GetComponent<LineRenderer>();
            playerControl = GetComponent<PlayerController>();

            registerCameraRaycast();
        }

        private void registerCameraRaycast() {
            CameraRaycast cameraRaycast = Camera.main.GetComponent<CameraRaycast>();
            cameraRaycast.mouseOverCellObservers += changeCurrentCellColorEntered;
            cameraRaycast.mouseExitCellObservers += changeCurrentCellColorExited;
        }

        private void changeCurrentCellColorEntered(Cell cell) {
            Material cellMaterial = cell.GetComponent<Renderer>().material;
            if (!initialCellColor.HasValue) {
                initialCellColor = cellMaterial.color;
            }
            if (cellMaterial.color == initialCellColor.Value) {
                cellMaterial.color = Color.cyan;
            }

            drawMovementLine(cell);
            var playerCharacter = playerControl.getCurrentPlayerCharacter();
            if (playerCharacter.getPossibleMovementLocations().costToGoThroughNode.ContainsKey(cell)) {
                
            }

        }
        
        private void changeCurrentCellColorExited(Cell cell) {
            Material cellMaterial = cell.GetComponent<Renderer>().material;
            cellMaterial.color = initialCellColor.Value;
        }

        // Update is called once per frame
        void Update() {
            highlightPossibleMovementLocations();
        }

        //TODO remove this method
        private List<Cell> highlightedCells = new List<Cell>();
        private void highlightPossibleMovementLocations() {
            var playerCharacter = /*playerControl.getCurrentPlayerCharacter() */enemyControl.getCurrentEnemyCharacter();
            if (!playerCharacter.isIDLE() && playerCharacter.getCharacterState() != Characters.State.FINISHED) { 
                foreach (Cell cell in highlightedCells) {
                    Material movementLocationMaterial = cell.GetComponent<Renderer>().material;
                    movementLocationMaterial.color = initialCellColor.HasValue ? initialCellColor.Value : Color.blue;
                }
                highlightedCells.Clear();
                return;
            }
            if (!playerCharacter.getCellLocation()) {
                return;
            }
            CreateGrid.movementLocationsInfo locationsInfo = playerCharacter.getPossibleMovementLocations();
            foreach (KeyValuePair<Cell, float> cellInfo in locationsInfo.costToGoThroughNode) {
                Material movementLocationMaterial = cellInfo.Key.GetComponent<Renderer>().material;
                movementLocationMaterial.color = Color.red;
                highlightedCells.Add(cellInfo.Key);
            }
        }

        private void drawMovementLine(Cell endLocation) {
            var playerCharacter = playerControl.getCurrentPlayerCharacter();
            if (!playerCharacter.isIDLE()) {
                movementLine.enabled = false;
                return;
            }
                
            movementLine.enabled = true;

            List<Cell> cellPath = CreateGrid.getPathFromLinks(playerCharacter.getPossibleMovementLocations(), playerCharacter.getCellLocation(), endLocation);
            if (endLocation.getCharacterOnCell()) {
                cellPath = playerControl.findPathTowardsAdjacentCharacter(endLocation);
            }
            if (cellPath == null) {
                movementLine.enabled = false;
                return;
            }

            int numVertices = cellPath.Count;
            movementLine.positionCount = numVertices;
            
            for (int i = 0; i < numVertices; ++i) { 
                Vector3 vertexPosition = cellPath[i].transform.position;
                vertexPosition.y += Tactics.Grid.CreateGrid.cellSize / 2;
                movementLine.SetPosition(i, vertexPosition);
            }
        }

    }

}