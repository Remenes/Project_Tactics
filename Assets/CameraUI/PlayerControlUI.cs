using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tactics.Controller;
using Tactics.Grid;

namespace Tactics.CameraUI {

    public class PlayerControlUI : MonoBehaviour {

        LineRenderer movementLine;
        PlayerController playerControl;
        Tactics.Characters.Character playerCharacter;

        private Color? initialCellColor = null;

        [SerializeField] private EnemyController enemyControl;

        // Use this for initialization
        void Start() {
            movementLine = GetComponent<LineRenderer>();
            registerPlayerController();
            registerCameraRaycast();
        }

        private void registerCameraRaycast() {
            CameraRaycast cameraRaycast = Camera.main.GetComponent<CameraRaycast>();
            cameraRaycast.mouseOverCellObservers += changeCurrentCellColorEntered;
            cameraRaycast.mouseExitCellObservers += changeCurrentCellColorExited;
        }

        private void registerPlayerController() {
            playerControl = GetComponent<PlayerController>();
            playerCharacter = playerControl.GetCurrentCharacter();
            playerControl.playerActionObservers += updateMovementLine;
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
            var playerCharacter = playerControl.GetCurrentCharacter();
            if (playerCharacter.GetPossibleMovementLocations().costToGoThroughNode.ContainsKey(cell)) {
                
            }

        }
        
        private void changeCurrentCellColorExited(Cell cell) {
            Material cellMaterial = cell.GetComponent<Renderer>().material;
            cellMaterial.color = initialCellColor.Value;
        }

        private void updateMovementLine() {
            drawMovementLine(playerCharacter.GetCellLocation());
        }

        // Update is called once per frame
        void Update() {

            //highlightPossibleMovementLocations();
            playerCharacter = playerControl.GetCurrentCharacter();
        }

        //TODO remove this method
        private List<Cell> highlightedCells = new List<Cell>();
        private void highlightPossibleMovementLocations() {
            var playerCharacter = /*playerControl.getCurrentPlayerCharacter() */enemyControl.GetCurrentCharacter();
            if (!playerCharacter.isIDLE() && playerCharacter.isFinished()) { 
                foreach (Cell cell in highlightedCells) {
                    Material movementLocationMaterial = cell.GetComponent<Renderer>().material;
                    movementLocationMaterial.color = initialCellColor.HasValue ? initialCellColor.Value : Color.blue;
                }
                highlightedCells.Clear();
                return;
            }
            if (!playerCharacter.GetCellLocation()) {
                return;
            }
            GridSpace.MovementLocationsInfo locationsInfo = playerCharacter.GetPossibleMovementLocations();
            foreach (KeyValuePair<Cell, float> cellInfo in locationsInfo.costToGoThroughNode) {
                Material movementLocationMaterial = cellInfo.Key.GetComponent<Renderer>().material;
                movementLocationMaterial.color = Color.red;
                highlightedCells.Add(cellInfo.Key);
            }
        }

        private void drawMovementLine(Cell endLocation) {
            if (!playerCharacter.isIDLE() || playerCharacter.isFinished()) {
                movementLine.enabled = false;
                return;
            }
            movementLine.enabled = true;

            List<Cell> cellPath = totalPathLink(endLocation);

            int numVertices = cellPath.Count;
            movementLine.positionCount = numVertices;
            for (int i = 0; i < numVertices; ++i) { 
                Vector3 vertexPosition = cellPath[i].transform.position;
                vertexPosition.y += GridSpace.cellSize / 2;
                movementLine.SetPosition(i, vertexPosition);
            }
        }

        private List<Cell> totalPathLink(Cell endLocation) {

            List<Cell> currentCellPath = GridSpace.GetPathFromLinks(playerCharacter.GetPossibleMovementLocations(), playerCharacter.GetCellLocation(), endLocation);
            List<Cell> cellPath = cellsFromCurrentMovementPath();
            
            if (currentCellPath != null && !endLocation.getCharacterOnCell() && playerCharacter.CanMove()) {
                if (cellPath.Count > 0) {
                    cellPath.RemoveAt(cellPath.Count - 1);
                }
                cellPath.AddRange(currentCellPath);
            }
            return cellPath;
        }

        private List<Cell> cellsFromCurrentMovementPath() {
            List<List<Cell>> movementPaths = playerCharacter.GetMovementPathsInfo();
            List<Cell> cellPath = new List<Cell>();
            if (movementPaths.Count > 0) {
                foreach (List<Cell> path in movementPaths) {
                    if (path != null) {
                        if (cellPath.Count > 0) {
                            // Remove the last one before every path to avoid duplicates, since the last and first are the same.
                            cellPath.RemoveAt(cellPath.Count - 1); 
                        }
                        cellPath.AddRange(path);
                    }
                }
            }
            return cellPath;
        }

    }

}