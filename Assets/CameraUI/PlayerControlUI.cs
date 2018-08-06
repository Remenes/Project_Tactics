using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tactics.Controller;
using Tactics.Grid;
using Tactics.Characters;

namespace Tactics.CameraUI {

    public class PlayerControlUI : MonoBehaviour {

        PlayerController playerControl;
        Character playerCharacter;

        private Color? initialCellColor = null;

        [SerializeField] private EnemyController enemyControl;

        // Use this for initialization
        void Start() {
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
            playerControl.PlayerActionObservers += updateAllMovementLine;
        }

        private void changeCurrentCellColorEntered(Cell cell) {
            Material cellMaterial = cell.GetComponent<Renderer>().material;
            if (!initialCellColor.HasValue) {
                initialCellColor = cellMaterial.color;
            }
            if (cellMaterial.color == initialCellColor.Value) {
                cellMaterial.color = Color.cyan;
            }

            drawCurrentMovementLine(cell);
            var playerCharacter = playerControl.GetCurrentCharacter();
            if (playerCharacter.GetPossibleMovementLocations().costToGoThroughNode.ContainsKey(cell)) {
                
            }

        }
        
        private void changeCurrentCellColorExited(Cell cell) {
            Material cellMaterial = cell.GetComponent<Renderer>().material;
            cellMaterial.color = initialCellColor.Value;
        }

        // Updates all the characters movement line when the player presses a key that might change the movement line 
        private void updateAllMovementLine() {
            foreach (Character character in playerControl.GetCharacters()) {
                MovementIndicator charIndicator = character.GetComponent<MovementIndicator>();
                charIndicator.DrawMovementLine(character.GetCellLocation());
            }
        }

        // Update is called once per frame
        void Update() {

            //highlightPossibleMovementLocations();
            playerCharacter = playerControl.GetCurrentCharacter();
        }

        private void drawCurrentMovementLine(Cell endLocation) {
            MovementIndicator playerIndicator = playerCharacter.GetComponent<MovementIndicator>();
            playerIndicator.DrawMovementLine(endLocation);
        }
        
    }

}