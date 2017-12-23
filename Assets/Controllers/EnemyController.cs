using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Tactics.Characters;
using Tactics.Grid;

namespace Tactics.Controller {

    public class EnemyController : MonoBehaviour {

        Character enemy;
        public Character getCurrentEnemyCharacter() { return enemy; }

        Character playerTarget;


        private bool turnFinished = false;
        public bool getTurnFinished() { return turnFinished; }
        public void resetStates() {
            turnFinished = false;
            enemy.resetCharacterState();
        }
        
        // Use this for initialization
        void Start() {
            enemy = GameObject.FindGameObjectWithTag("Enemy").GetComponent<Character>();
            playerTarget = GameObject.FindGameObjectWithTag("Player").GetComponent<Character>();
        }

        // Update is called once per frame
        void Update() {
            if (turnFinished)
                return;
            if (enemy.getCharacterState() == State.IDLE) {
                List<Cell> pathToPlayer = getPathTowards(playerTarget.getCellLocation(), enemy.getMovementDistance());
                if (pathToPlayer[pathToPlayer.Count - 1] == playerTarget.getCellLocation()) { //atack target if the target is within range
                    pathToPlayer.RemoveAt(pathToPlayer.Count - 1);
                    enemy.setAttackTarget(pathToPlayer, playerTarget);
                }
                else {
                    enemy.setMovementPath(pathToPlayer);   
                }
                enemy.resetPossibleMovementLocations(); //TODO think of a better way of doing this for the enemies (if need be)
            }
            else if (enemy.getCharacterState() == State.FINISHED) {
                turnFinished = true;
            }
        }

        public List<Cell> getPathTowards(Cell goal, float maxMovementDistance) {
            Cell enemyLocation = enemy.getCellLocation();
            HashSet<Cell> evaluatedCells = new HashSet<Cell>();
            HashSet<Cell> discoveredCells = new HashSet<Cell> { enemyLocation };

            Dictionary<Cell, Cell> cameFrom = new Dictionary<Cell, Cell>();
            Dictionary<Cell, float> costToGoalThroughNode = new Dictionary<Cell, float>();
            costToGoalThroughNode.Add(enemyLocation, 0);
            
            while (discoveredCells.Count != 0) {
                Cell currentCell = null;
                //TODO for more efficiency, change this to a regular for loop
                foreach (var cellPair in costToGoalThroughNode.OrderBy(x => x.Value)) {
                    if (discoveredCells.Contains(cellPair.Key)) {
                        currentCell = cellPair.Key;
                        break;
                    }
                }
                if (currentCell == null)
                    throw new System.Exception("No current cells");
                if (currentCell == goal) {
                    //Reeturn path to go to goal
                    return getClosestMovementPathFromLinks(cameFrom, costToGoalThroughNode, enemyLocation, currentCell, maxMovementDistance);
                }
                discoveredCells.Remove(currentCell);
                if (!evaluatedCells.Contains(currentCell)) {
                    evaluatedCells.Add(currentCell);
                }
                checkAndAddDiscoveredCells(evaluatedCells, discoveredCells, cameFrom, costToGoalThroughNode, currentCell, goal);
            }
            //return new movementLocationsInfo(cameFrom, costToGoalThroughNode);
            throw new System.Exception("No paths found");
        }

        private void checkAndAddDiscoveredCells(HashSet<Cell> evaluatedCells, HashSet<Cell> discoveredCells, Dictionary<Cell, Cell> cameFrom, Dictionary<Cell, float> costToGoalThroughNode, Cell currentCell, Cell targetCell) {
            HashSet<Cell> allCellsToDiscover = currentCell.getAllSurroundingCells();

            foreach (Cell neighbor in allCellsToDiscover) {
                bool isInDiagonal = currentCell.getOutDiagonalCells().Contains(neighbor);
                float distanceFromStartToNeighbor = costToGoalThroughNode[currentCell] + (isInDiagonal ? Mathf.Sqrt(2) : 1);

                if (evaluatedCells.Contains(neighbor) || (neighbor.getCharacterOnCell() && neighbor != targetCell)) {
                    continue;
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

        private List<Cell> getClosestMovementPathFromLinks(Dictionary<Cell, Cell> cellCameFroms, Dictionary<Cell, float> costToGoThroughNodes, Cell startingCell, Cell goal, float maxMovementDistance) {
            List<Cell> cellsInPath = new List<Cell> ();
            if (costToGoThroughNodes[goal] <= maxMovementDistance) {
                cellsInPath.Add(goal);
            }
            Cell currentCellInPath = goal;
            while (cellCameFroms.ContainsKey(currentCellInPath)) {
                currentCellInPath = cellCameFroms[currentCellInPath];
                if (costToGoThroughNodes[currentCellInPath] <= maxMovementDistance) {
                    cellsInPath.Add(currentCellInPath);
                }
            }
            cellsInPath.Reverse();
            return cellsInPath;
        }
    }

}