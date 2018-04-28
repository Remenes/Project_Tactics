using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Tactics.Grid {

    public enum Layer { CELL_LAYER = 8 }

    public class GridSpace : MonoBehaviour {

        //TODO this should also contain the starting cell
        public struct MovementLocationsInfo {

            public Dictionary<Cell, Cell> cameFrom;
            public Dictionary<Cell, float> costToGoThroughNode;
            //Creates a list of characters on the possible movement cells. This well help treat characters as obstacles
            //but keep track of the characaters as obstacles to handle other scenarios like clicking an enemy while they are in range
            public HashSet<Cell> cellsWithCharacter;

            public MovementLocationsInfo(Dictionary<Cell, Cell> cameFromInfo, Dictionary<Cell, float> costToGoThroughNodeInfo, HashSet<Cell> cellsWithCharacterInfo) {
                cameFrom = cameFromInfo;
                costToGoThroughNode = costToGoThroughNodeInfo;
                cellsWithCharacter = cellsWithCharacterInfo;
            }

        }

        private LayerMask CELL_LAYER_MASK;
        [SerializeField] public const float cellSize = 1;
        //private static Vector3 cellVectorSize;

        [SerializeField] private GameObject cellObject;

        private static int cellNumber = 0;

        public static bool gridCreated = false;

        private Vector3[] adjacentVectors = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        private Vector3[] diagonalVectors = { Vector3.forward + Vector3.right, Vector3.forward + Vector3.left,
                                              Vector3.back + Vector3.right, Vector3.back + Vector3.left};

        
        // Use this for initialization
        void Start() {
            CELL_LAYER_MASK = 1 << (int) Layer.CELL_LAYER;
            //cellVectorSize = Vector3.one * cellSize;
            
            replaceBlocksWithGrid();

            gridCreated = true;
        }

        private void replaceBlocksWithGrid() {
            foreach (Transform child in transform) {
                GameObject blockToGrid = child.gameObject;
                Bounds blockBounds = blockToGrid.GetComponent<Collider>().bounds;
                Destroy(blockToGrid);
                createGrid(blockBounds);
            }
        }
        
        private void createGrid(Bounds blockBounds) {
            int cellXAmount = (int)(blockBounds.size.x / cellSize), cellZAmount = (int)(blockBounds.size.z / cellSize);
            int cellX = 0;
            for (float xMin = blockBounds.min.x; xMin < blockBounds.min.x + cellXAmount * cellSize; xMin += cellSize, ++cellX) {
                int cellZ = 0;
                for (float zMin = blockBounds.min.z; zMin < blockBounds.min.z + cellZAmount * cellSize; zMin += cellSize, ++cellZ) {
                    addCell(xMin + cellSize / 2, zMin + cellSize / 2, blockBounds);
                }
            }
        }

        /// <summary>
        /// Adds a cell given the center of the coordinates and the cell numbers that it belongs in the grid of cells
        /// Puts the cell in the world with each cell's position rounded down 
        /// </summary>
        private void addCell(float cellCenterX, float cellCenterZ, Bounds blockBounds) {
            Vector3 cellLocation = new Vector3(Mathf.Floor(cellCenterX - cellSize / 2), Mathf.Floor(blockBounds.center.y), Mathf.Floor(cellCenterZ - cellSize / 2));
            cellLocation += Vector3.one * (cellSize / 2);
            Cell newCellObject = Instantiate(cellObject, cellLocation, Quaternion.identity, GameObject.Find("CellLocations").transform).GetComponent<Cell>();
            newCellObject.name = "Cell Object " + cellNumber++;
            addAdjacentCellsTo(newCellObject, adjacentVectors, false);
            addAdjacentCellsTo(newCellObject, diagonalVectors, true);
        }

        private void addAdjacentCellsTo(Cell cell, Vector3[] directionalVectors, bool isDiagnol) {
            
            foreach (Vector3 direction in directionalVectors) {
                Vector3 checkForCellAtPos = cell.transform.position + direction.normalized * cellSize;
                Collider[] cellCollidersHit = Physics.OverlapSphere(checkForCellAtPos, .01f, CELL_LAYER_MASK);
                //Check the spot near the cell for another cell. Checks a small radius so it should only hit one cell
                //If it hits a cell, then that cell's the adjacent cell.
                if (cellCollidersHit.Length == 0) { continue; }

                Cell cellHit = cellCollidersHit[0].GetComponent<Cell>();
                cell.linkCellTo(cellHit, isDiagnol);
            }
        }

        /// <summary>
        /// Returns information regarding every cell that is within the maxMovementDistance and sends that information back as a struct
        /// containing the cost to get to each node and the node that comes right before each node.
        /// </summary>
        /// <param name="start"> The node that is the center of the search through nodes </param>
        /// <param name="maxMovementDistance"> The total distance a character can move</param>
        public static MovementLocationsInfo GetPossibleMovementLocations(Cell start, float maxMovementDistance) {
            HashSet<Cell> evaluatedCells = new HashSet<Cell>();
            HashSet<Cell> discoveredCells = new HashSet<Cell> { start };

            Dictionary<Cell, Cell> cameFrom = new Dictionary<Cell, Cell>();
            Dictionary<Cell, float> costToGoalThroughNode = new Dictionary<Cell, float>();
            costToGoalThroughNode.Add(start, 0);

            HashSet<Cell> cellsWithCharacter = new HashSet<Cell>();
            
            while (discoveredCells.Count != 0) {
                Cell currentCell = null;
                foreach (var cellPair in costToGoalThroughNode.OrderBy(x => x.Value)) {
                    if (discoveredCells.Contains(cellPair.Key)) {
                        currentCell = cellPair.Key;
                        break;
                    }
                }
                if (currentCell == null)
                    throw new System.Exception("No current cells");
                
                discoveredCells.Remove(currentCell);
                if (!evaluatedCells.Contains(currentCell)) {
                    evaluatedCells.Add(currentCell);
                }
                CheckAndAddDiscoveredCells(evaluatedCells, discoveredCells, cameFrom, costToGoalThroughNode, currentCell, maxMovementDistance, cellsWithCharacter);
            }
            return new MovementLocationsInfo(cameFrom, costToGoalThroughNode, cellsWithCharacter);
        }

        /// <summary>
        /// Only call this function if the script is making it's own path finding AI (Ex. To get the full path to a character)
        /// </summary>
        /// <param name="evaluatedCells"> Set of cells already evaluvated </param>
        /// <param name="discoveredCells"> Set of cells already discovered </param>
        /// <param name="cameFrom"> Dictionary with keys of a cell and value of whichever cell lead to it </param>
        /// <param name="costToGoalThroughNode"> Dictionary with keys of a cell and value of the distance it took to get there </param>
        /// <param name="cellsWithCharacter"> Set of cells that have a character on them: Adds on to this list as it discovers cells </param>
        /// <param name="currentCell"></param>
        /// <param name="maxMovementDistance"></param>
        public static void CheckAndAddDiscoveredCells(HashSet<Cell> evaluatedCells, HashSet<Cell> discoveredCells, Dictionary<Cell, Cell> cameFrom, Dictionary<Cell, float> costToGoalThroughNode, Cell currentCell, float maxMovementDistance, HashSet<Cell> cellsWithCharacter) {
            HashSet<Cell> allCellsToDiscover = currentCell.getAllSurroundingCells();

            foreach (Cell neighbor in allCellsToDiscover) {
                bool isInDiagonal = currentCell.getOutDiagonalCells().Contains(neighbor);
                float distanceFromStartToNeighbor = costToGoalThroughNode[currentCell] + (isInDiagonal ? Mathf.Sqrt(2) : 1);

                if (evaluatedCells.Contains(neighbor) || distanceFromStartToNeighbor > maxMovementDistance) {
                    continue;
                }
                if (cellsWithCharacter != null) {
                    if (neighbor.getCharacterOnCell()) {
                        if (!cellsWithCharacter.Contains(neighbor)) {
                            cellsWithCharacter.Add(neighbor);
                        }
                        continue;
                    }
                }
                if (!discoveredCells.Contains(neighbor)) {
                    discoveredCells.Add(neighbor);
                }
                if (costToGoalThroughNode.ContainsKey(neighbor) && distanceFromStartToNeighbor >= costToGoalThroughNode[neighbor]) {
                    continue;
                }
                cameFrom[neighbor] = currentCell;
                costToGoalThroughNode[neighbor] = distanceFromStartToNeighbor;
            }
        }

        //TODO linked with the struct: take out the currentCell in the parameter (It should be given in the cellLinks info)
        public static List<Cell> GetPathFromLinks(MovementLocationsInfo cellLinks, Cell currentCell, Cell goal) {
            if (!cellLinks.cameFrom.ContainsKey(goal))
                return null;
            List<Cell> cellsInPath = new List<Cell> { goal };
            Cell currentCellInPath = goal;
            while (cellLinks.cameFrom.ContainsKey(currentCellInPath) && currentCellInPath != currentCell) {
                currentCellInPath = cellLinks.cameFrom[currentCellInPath];
                cellsInPath.Add(currentCellInPath);
            }
            cellsInPath.Reverse();
            return cellsInPath;
        }

        private void OnDrawGizmos() { //TODO replace gizmos with actual thing
            if (!Application.isPlaying)
                return;
            foreach (Transform cell in GameObject.Find("CellLocations").transform) {
                Gizmos.color = Color.green;
                Collider collider = cell.GetComponent<Collider>();
                Vector3 squareSize = Vector3.Scale(collider.bounds.size, new Vector3(1, .01f, 1));
                Vector3 squarePos = collider.bounds.center;
                squarePos.y = collider.bounds.max.y - (squareSize.y / 2);
                Gizmos.DrawWireCube(squarePos, squareSize);
            }
        }
        
    }

}