using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tactics.Grid {

    public class Cell : MonoBehaviour {

        private HashSet<Cell> outAdjacentCells = new HashSet<Cell>(); //TODO make an inCells;
        public HashSet<Cell> getOutAdjacentCells() { return outAdjacentCells; }
        public void AddOutAdjacentCell(Cell newCell) {
            outAdjacentCells.Add(newCell);
        }
        private HashSet<Cell> outDiagonalCells = new HashSet<Cell>(); //TODO make an inCells;
        public HashSet<Cell> getOutDiagonalCells() { return outDiagonalCells; }
        public void AddOutDiagonolCell(Cell newCell) {
            outDiagonalCells.Add(newCell);
        }

        public void linkCellTo(Cell otherCell, bool isDiagnol) {
            if (isDiagnol) {
                this.AddOutDiagonolCell(otherCell);
                otherCell.AddOutDiagonolCell(this);
            }
            else {
                this.AddOutAdjacentCell(otherCell);
                otherCell.AddOutAdjacentCell(this);
            }
        }
        

    }

}