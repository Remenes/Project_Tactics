using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Tactics.Controller;
using Tactics.Grid;
using Tactics.Characters;

namespace Tactics.CameraUI {

    public class PlayerControlUI : MonoBehaviour {

        PlayerController playerControl;
        Character playerCharacter;

        [Header("Player UI")]
        [SerializeField] private GameObject abilitySelectionUI;
        private Text abilitySelectionText;

        private Color? initialCellColor = null;

        [Header("Enemy Controller")]
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
            playerControl.PlayerChangedAbilityObservers += updateAbilityUI;

            abilitySelectionText = abilitySelectionUI.GetComponentInChildren<Text>();
            updateAbilityUI(-1);
        }

        private void changeCurrentCellColorEntered(Cell cell) {
            Material cellMaterial = cell.GetComponent<Renderer>().material;
            if (!initialCellColor.HasValue) {
                initialCellColor = cellMaterial.color;
            }
            if (cellMaterial.color == initialCellColor.Value) {
                cellMaterial.color = Color.cyan;
            }

            if (!playerControl.IsUsingAbility()) {
                drawCurrentMovementLine(cell);
            }
            else {
                drawCurrentMovementLine(playerCharacter.GetCellLocation());
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

        // Updates the Ability Selection UI when a new ability is selected
        private void updateAbilityUI(int abilityIndex) {

            if (!playerControl.IsUsingAbility()) {
                abilitySelectionUI.SetActive(false);
                return;
            }
            abilitySelectionUI.SetActive(true);
            string newName = playerCharacter.GetAbilityConfig(abilityIndex).name;
            abilitySelectionText.text = newName;
            
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