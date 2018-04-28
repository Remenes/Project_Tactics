using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Tactics.Grid;

namespace Tactics.Characters { 

    public class EnemyMeleeAI : MonoBehaviour {

        private Character enemy;

	    // Use this for initialization
	    void Start () {
            enemy = GetComponent<Character>();
	    }

        public List<Cell> GetWantedPath(Cell target) {
            return GetPathTowards(target);
        }
	
        //TODO remove the lag spike that occurs here from all the calculations: by using Coroutine or switching to A*
        private List<Cell> GetPathTowards(Cell goal) {
            Cell enemyLocation = enemy.getCellLocation();
            HashSet<Cell> evaluatedCells = new HashSet<Cell>();
            HashSet<Cell> discoveredCells = new HashSet<Cell> { enemyLocation };

            Dictionary<Cell, Cell> cameFrom = new Dictionary<Cell, Cell>();
            Dictionary<Cell, float> costToGoalThroughNode = new Dictionary<Cell, float>();
            costToGoalThroughNode.Add(enemyLocation, 0);

            HashSet<Cell> cellsWithCharacters = new HashSet<Cell>();

            while (discoveredCells.Count != 0) {
                Cell currentCell = null;
                //TODO for more efficiency, implement own data structure
                foreach (var cellPair in costToGoalThroughNode.OrderBy(x => x.Value)) {
                    if (discoveredCells.Contains(cellPair.Key)) {
                        currentCell = cellPair.Key;
                        break;
                    }
                }
                if (currentCell == null)
                    throw new System.Exception("No current cells");
                if (currentCell == goal) {
                    //Return path to go to goal
                    return getClosestMovementPathFromLinks(cameFrom, costToGoalThroughNode, enemyLocation, currentCell);
                }
                discoveredCells.Remove(currentCell);
                if (!evaluatedCells.Contains(currentCell)) {
                    evaluatedCells.Add(currentCell);
                }
                GridSpace.CheckAndAddDiscoveredCells(evaluatedCells, discoveredCells, cameFrom, costToGoalThroughNode, currentCell, 200, cellsWithCharacters);
                if (cellsWithCharacters.Contains(goal)) {
                    //Return path to go to goal
                    return getClosestMovementPathFromLinks(cameFrom, costToGoalThroughNode, enemyLocation, currentCell);
                }
            }
            //return new MovementLocationsInfo(cameFrom, costToGoalThroughNode);
            throw new System.Exception("No paths found");
        }

        private List<Cell> getClosestMovementPathFromLinks(Dictionary<Cell, Cell> cellCameFroms, Dictionary<Cell, float> costToGoThroughNodes, Cell startingCell, Cell goal) {
            List<Cell> cellsInPath = new List<Cell>();
            if (costToGoThroughNodes[goal] <= enemy.getMovementDistance()) {
                cellsInPath.Add(goal);
            }
            Cell currentCellInPath = goal;
            while (cellCameFroms.ContainsKey(currentCellInPath)) {
                currentCellInPath = cellCameFroms[currentCellInPath];
                if (costToGoThroughNodes[currentCellInPath] <= enemy.getMovementDistance()) {
                    cellsInPath.Add(currentCellInPath);
                }
            }
            cellsInPath.Reverse();
            return cellsInPath;
        }
    }
}

