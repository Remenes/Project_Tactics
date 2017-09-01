using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tactics.Grid;

namespace Tactics.Characters {

    public class Character : MonoBehaviour {

        private LayerMask CELL_LAYER_MASK;
        
        [SerializeField] private Cell currentLocation = null;
        public Cell getCurrentCellLocation() { return currentLocation; }
        public void setCurrentCellLocation(Cell newCell) { currentLocation = newCell; }

        // Use this for initialization
        void Start() {
            CELL_LAYER_MASK = 1 << (int) Layer.CELL_LAYER;
            StartCoroutine( setStartingCellLocation() );
        }

        private IEnumerator setStartingCellLocation() {
            while (CreateGrid.gridCreated == false) {
                yield return 0;
            }
            Cell cellBelowCharacter = getCellBelowCharacter();
            setCurrentCellLocation(cellBelowCharacter);
            transform.position = cellBelowCharacter.transform.position;
        }

        private Cell getCellBelowCharacter() {
            Cell cellBelowCharacter = null;
            RaycastHit rayHit;
            if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 2f, CELL_LAYER_MASK)) {
                cellBelowCharacter = rayHit.collider.GetComponent<Cell>();
            }
            return cellBelowCharacter;
        }

    }
}