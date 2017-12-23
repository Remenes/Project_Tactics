using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tactics.Characters;

namespace Tactics.Grid {

    public class Cell : MonoBehaviour {

        private Character characterOnCell;
        public Character getCharacterOnCell() { return characterOnCell; }
        public void clearCharacterOnCell() { characterOnCell = null; }
        /// <summary>
        /// Do not call this method. All cell linking should be called in the character class.
        /// </summary>
        /// <param name="newCharacter"></param>
        public void setNewCharacterOnCell(Character newCharacter) { characterOnCell = newCharacter; }

        private HashSet<Cell> outAdjacentCells = new HashSet<Cell>(); //TODO make an inAdjacentCells;
        public HashSet<Cell> getOutAdjacentCells() { return outAdjacentCells; }
        public void AddOutAdjacentCell(Cell newCell) {
            outAdjacentCells.Add(newCell);
        }
        private HashSet<Cell> outDiagonalCells = new HashSet<Cell>(); //TODO make an inDiagonalCells;
        public HashSet<Cell> getOutDiagonalCells() { return outDiagonalCells; }
        public void AddOutDiagonolCell(Cell newCell) {
            outDiagonalCells.Add(newCell);
        }

        public HashSet<Cell> getAllSurroundingCells() {
            HashSet<Cell> surroundingCells = new HashSet<Cell>(outAdjacentCells);
            surroundingCells.UnionWith(outDiagonalCells);
            return surroundingCells;
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