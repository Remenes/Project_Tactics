using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tactics.Grid;

namespace Tactics.CameraUI {

    public class CameraRaycast : MonoBehaviour {

        private int CELL_LAYER = 8;
        private LayerMask cellLayer;
        [SerializeField] private float maxDistanceRaycast = 100;

        public delegate void OnMouseOverCell(Cell currentCell);
        public event OnMouseOverCell mouseOverCellObservers;
        public delegate void OnMouseExitCell(Cell cellLeft);
        public event OnMouseExitCell mouseExitCellObservers;
        private Cell currentCell;

        // Use this for initialization
        void Start() {
            cellLayer = 1 << CELL_LAYER;
        }

        // Update is called once per frame
        void Update() {
            raycastForCell();
        }

        private void raycastForCell() {
            RaycastHit rayHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Cell cellHit = null;
            bool mouseOverCell = Physics.Raycast(ray, out rayHit, maxDistanceRaycast, cellLayer) && (cellHit = rayHit.collider.GetComponent<Cell>());
            if (mouseOverCell) {
                updateCellEnteredAndCallObservers(cellHit);
            }
            else {
                updateCellExitedAndCallObservers();
            }
        }

        private void updateCellEnteredAndCallObservers(Cell newCell) {
            if (currentCell && newCell != currentCell) {
                mouseExitCellObservers(currentCell);
            }
            currentCell = newCell;
            mouseOverCellObservers(newCell);
        }

        private void updateCellExitedAndCallObservers() {
            if (currentCell) {
                mouseExitCellObservers(currentCell);
            }
            currentCell = null;
        }
        

    }

}