using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tactics.Grid;
using Tactics.Characters;

namespace Tactics.CameraUI {

    public class MovementIndicator : MonoBehaviour {

        LineRenderer movementLine;
        Character character;

        // Use this for initialization
        void Start() {
            character = GetComponent<Character>();
            movementLine = GetComponentInChildren<LineRenderer>();
        }
        
        public void DrawMovementLine(Cell endLocation) {
            if (!character.isIDLE() || character.isFinished()) {
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

            List<Cell> currentCellPath = GridSpace.GetPathFromLinks(character.GetPossibleMovementLocations(), character.GetCellLocation(), endLocation);
            List<Cell> cellPath = cellsFromCurrentMovementPath();

            if (currentCellPath != null && !endLocation.getCharacterOnCell() && character.CanMove()) {
                if (cellPath.Count > 0) {
                    cellPath.RemoveAt(cellPath.Count - 1);
                }
                cellPath.AddRange(currentCellPath);
            }
            return cellPath;
        }

        private List<Cell> cellsFromCurrentMovementPath() {
            List<List<Cell>> movementPaths = character.GetMovementPathsInfo();
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