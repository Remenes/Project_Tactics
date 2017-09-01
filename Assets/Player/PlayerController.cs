using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Tactics.CameraUI;
using Tactics.Grid;
using Tactics.Characters;

namespace Tactics.Controller {

    public class PlayerController : MonoBehaviour {

        private GameObject player;
        private NavMeshAgent playerAgent;
        private AICharacterControl playerControl;
        private Character playerCharacter;


        private CameraRaycast cameraRaycast;
        private Cell currentCell;
        private Color? initialCellColor = null;

        private Cell startCell = null;
        private Cell endCell = null;

        private List<Cell> shortestPath = null;

        LineRenderer movementLine;

        // Use this for initialization
        void Start() {
            player = GameObject.FindGameObjectWithTag("Player");
            playerAgent = player.GetComponent<NavMeshAgent>();
            playerControl = player.GetComponent<AICharacterControl>();
            playerCharacter = player.GetComponent<Character>();

            movementLine = GetComponent<LineRenderer>();


            cameraRaycast = Camera.main.GetComponent<CameraRaycast>();
            cameraRaycast.mouseOverCellObservers += movePlayer;
            //cameraRaycast.mouseOverCellObservers += drawMovementLine;
            cameraRaycast.mouseOverCellObservers += changeCurrentCellColorEntered;

            cameraRaycast.mouseExitCellObservers += changeCurrentCellColorExited;
            //cameraRaycast.mouseExitCellObservers += removeMovementLine;

        }

        // Update is called once per frame
        void Update() {
            drawMovementLine();
        }

        private void movePlayer(Cell cell) {
            if (Mouse.LeftClicked) {
                playerControl.SetTarget(cell.transform);
            }
        }

        private void drawMovementLine() {
            movementLine.enabled = true;

            NavMeshPath playerPath = playerAgent.path;
            int numVertices = playerPath.corners.Length;
            movementLine.positionCount = numVertices;
            
//            movementLine.SetPositions(playerPath.corners);
            for (int i = 0; i < numVertices; ++i) {
                movementLine.SetPosition(i, playerPath.corners[i]);
            }
        }

        private void removeMovementLine(Cell cell) {
            movementLine.enabled = false;
        }

        private void changeCurrentCellColorEntered(Cell cell) {
            Material cellMaterial = cell.GetComponent<Renderer>().material;
            if (!initialCellColor.HasValue) {
                initialCellColor = cellMaterial.color;
            }
            if (cellMaterial.color == initialCellColor.Value) {
                cellMaterial.color = Color.cyan;
            }
            
            if (Mouse.LeftClicked) {
                setAdjacentDiagonalCellColor(cell);
                endCell = cell;
                CreateGrid.pathInformation pathInfo = CreateGrid.getCellPathsInfo(playerCharacter.getCurrentCellLocation(), endCell);
                shortestPath = pathInfo.pathToGetToTarget;
                float cost = pathInfo.costToGetToTarget;
                print(cost);
                foreach (Cell cellPath in shortestPath) {
                    cellPath.GetComponent<Renderer>().material.color = Color.yellow;
                }
                playerCharacter.setCurrentCellLocation(cell);
            }
        }

        private void setAdjacentDiagonalCellColor(Cell cell) {
            foreach (Cell adjecentCell in cell.getOutAdjacentCells()) {
                adjecentCell.GetComponent<Renderer>().material.color = Color.blue;
            }
            foreach (Cell adjecentCell in cell.getOutDiagonalCells()) {
                adjecentCell.GetComponent<Renderer>().material.color = Color.magenta;
            }
        }

        private void changeCurrentCellColorExited(Cell cell) {
            Material cellMaterial = cell.GetComponent<Renderer>().material;
            cellMaterial.color = initialCellColor.Value;
            foreach (Cell adjecentCell in cell.getOutAdjacentCells()) {
                adjecentCell.GetComponent<Renderer>().material.color = initialCellColor.Value;
            }
            foreach (Cell adjecentCell in cell.getOutDiagonalCells()) {
                adjecentCell.GetComponent<Renderer>().material.color = initialCellColor.Value;
            }
            if (shortestPath != null) {
                foreach (Cell cellPath in shortestPath) {
                    cellPath.GetComponent<Renderer>().material.color = initialCellColor.Value;
                }
            }
            initialCellColor = null;
            
        }


    }

}