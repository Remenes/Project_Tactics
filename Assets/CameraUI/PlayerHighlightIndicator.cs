using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tactics.Controller;
using Tactics.Grid;
using Tactics.Characters;
using System;

namespace Tactics.CameraUI {

    public class PlayerHighlightIndicator : MonoBehaviour {

        [SerializeField] private GameObject highlightIndicator;
        [SerializeField] private GameObject turnEndedIndicator;
        [SerializeField] private GameObject enemyTargetIndicator;
        [SerializeField] private GameObject enemyInRangeIndicator;

        private GameObject highlightPlayerIndicator;
        private GameObject highlightCursorIndicator;
        // For assosciating a highlight with every enemy and can be simply enabled/disabled when the time comes
        private Dictionary<Character, GameObject> enemyHighlights;

        EnemyController enemyControl;
        PlayerController playerControl;
        Character currentPlayerCharacter;

        private Character targettingEnemy = null;

        // ---------------------------------------------------------------------------------------
        // ---------------- Helper Functions for changing Highlight -------------------------------
        // ---------------------------------------------------------------------------------------

        private void signalAllHighlightDisable() {
            disableHighlight(highlightCursorIndicator);
            disableHighlight(highlightPlayerIndicator);
            //highlightCursorIndicator.SetActive(false);
            //highlightPlayerIndicator.SetActive(false);
            foreach (GameObject highlights in enemyHighlights.Values) {
                disableHighlight(highlights);
                //highlights.SetActive(false);
            }
        }

        private void switchHighlight(ref GameObject highlightInstance, GameObject highlightPrefabToBecome) {
            Transform highlightInstanceTransform = highlightInstance.transform;
            Destroy(highlightInstance);
            highlightInstance = Instantiate(highlightPrefabToBecome);
            highlightInstance.transform.position = highlightInstanceTransform.position;
        }

        private void disableHighlight(GameObject highlightInstance) {
            highlightInstance.SetActive(false);
        }

        private void setHighlightActivePosition(GameObject highlight, Cell newCell) {
            highlight.SetActive(true);
            // Add an offset so that the indicator is slightly above the top of the cell;
            Vector3 yOffset = Vector3.up * GridSpace.cellSize / 2 + Vector3.up * Mathf.Epsilon;
            Vector3 newHighlightPos = newCell.transform.position + Vector3.up * GridSpace.cellSize / 2;
            highlight.transform.position = newHighlightPos;
        }

        // Helper for changing the indicator to the enemy target indicator, which checks whether it highlight needs to be changed
        // and changes it if it does, and disables it when the enemy isn't in range
        private void switchToEnemyIndicator(Cell cell) {
            Character enemyOnCell = cell.GetCharacterOnCell();
            bool cellHasEnemyCharacter = enemyOnCell && enemyOnCell.CompareTag(EnemyController.ENEMY_TAG);
            
            // If the player had previously targetted an enemy, but is no longer targetting him
            if (targettingEnemy && targettingEnemy != enemyOnCell) {
                // Re-highlight all enemies
                highlightEnemiesInRange();
            }

            if (cellHasEnemyCharacter) {
                // If the current character can attack the target, switch to the attack indicator if need be
                if (currentPlayerCharacter.CanAttackTarget(enemyOnCell)) {
                    if (!targettingEnemy || targettingEnemy != enemyOnCell) {
                        targettingEnemy = cell.GetCharacterOnCell();
                        switchHighlight(ref highlightCursorIndicator, enemyTargetIndicator);
                        disableHighlight(enemyHighlights[enemyOnCell]);
                    }
                }
                // Else, show no indicator, but switch the highlight to the normal indicator
                else {
                    switchHighlight(ref highlightCursorIndicator, highlightIndicator);
                    highlightCursorIndicator.SetActive(false);
                }
            }
            else {
                if (targettingEnemy) {
                    targettingEnemy = null;
                    switchHighlight(ref highlightCursorIndicator, highlightIndicator);
                }
            }
        }

        // Helper function to get called in updateOnPlayerActions
        // which highlights enemies in range after the player performs an action
        private void highlightEnemiesInRange() {
            foreach (Character enemy in enemyControl.GetCharacters()) {
                if (currentPlayerCharacter.CanAttackTarget(enemy)) {
                    setHighlightActivePosition(enemyHighlights[enemy], enemy.GetCellLocation());
                }
                else {
                    disableHighlight(enemyHighlights[enemy]);
                }
            }
        }

        // ---------------------------------------------------------------------------------------
        // ------- Functions to use in events ---------------
        // ---------------------------------------------------------------------------------------

        private void signalPlayerIndicatorChange() {
            if (playerControl.GetTurnFinished()) {
                return;
            }
            highlightPlayerIndicator.SetActive(true);
            setHighlightActivePosition(highlightPlayerIndicator, currentPlayerCharacter.GetCellLocation());
        }

        private void resetPlayerIndicator() {
            switchHighlight(ref highlightPlayerIndicator, highlightIndicator);
            signalPlayerIndicatorChange();
            highlightEnemiesInRange();
        }

        private void updateOnCellEntered(Cell cell) {
            Character characterOnCell = cell.GetCharacterOnCell();
            bool cellHasPlayerCharacter = characterOnCell && 
                                          characterOnCell.CompareTag(PlayerController.PLAYER_TAG);
            bool cellHasTargetButCantAttack = cell.GetCharacterOnCell() &&
                                          !currentPlayerCharacter.CanAttackTarget(characterOnCell);

            if (currentPlayerCharacter.FinishedActionQueue() || cellHasPlayerCharacter || cellHasTargetButCantAttack) {
                highlightCursorIndicator.SetActive(false);
                return;
            }
            switchToEnemyIndicator(cell);
            setHighlightActivePosition(highlightCursorIndicator, cell);
        }

        private void updateOnCellExit(Cell cellLeft) {
             highlightCursorIndicator.SetActive(false);
        }

        private void updateOnPlayerAction() {
            if (playerCharacterChanged()) {
                currentPlayerCharacter = playerControl.GetCurrentCharacter();
            }
            if (currentPlayerCharacter.FinishedActionQueue()) {
                switchHighlight(ref highlightPlayerIndicator, turnEndedIndicator);
            }
            else {
                switchHighlight(ref highlightPlayerIndicator, highlightIndicator);
            }
            if (playerControl.GetTurnFinished() || playerControl.GetExecutingActions()) {
                signalAllHighlightDisable();
                return;
            }
            signalPlayerIndicatorChange();
            highlightEnemiesInRange();
            // Let's the cursor refresh itself to remove the highlight for the enemy that the cursor is over
            if (currentPlayerCharacter.HasActionPoints()) {
                targettingEnemy = null;
            }
        }

        // ---------------------------------------------------------------------------------------
        // ------------Register necessary objects --------------
        // ---------------------------------------------------------------------------------------

        private void registerPlayerController() {
            playerControl = GetComponent<PlayerController>();
            currentPlayerCharacter = playerControl.GetCurrentCharacter();
            playerControl.PlayerActionObservers += updateOnPlayerAction;
            playerControl.CharactersFinishedExecutingObservers += signalPlayerIndicatorChange;
            playerControl.TurnResettedObservers += resetPlayerIndicator;
            signalPlayerIndicatorChange();
        }

        private void registerCameraRaycast() {
            CameraRaycast cameraRaycast = Camera.main.GetComponent<CameraRaycast>();
            cameraRaycast.mouseOverCellObservers += updateOnCellEntered;
            cameraRaycast.mouseExitCellObservers += updateOnCellExit;
        }

        private void registerHighlightedObjects() {
            highlightPlayerIndicator = Instantiate(highlightIndicator);
            highlightPlayerIndicator.SetActive(false);
            highlightCursorIndicator = Instantiate(highlightIndicator);
            highlightCursorIndicator.SetActive(false);
            registerEnemyHighlights();
        }

        private void registerEnemyHighlights() {
            enemyHighlights = new Dictionary<Character, GameObject>();
            // Used to find the current enemies that the controller has
            enemyControl = FindObjectOfType<EnemyController>();
            // Puts a highlight in every enemy and enables/disables it whenn it needs to
            foreach (Character enemy in enemyControl.GetCharacters()) {
                enemyHighlights[enemy] = Instantiate(enemyInRangeIndicator);
                enemyHighlights[enemy].SetActive(false);
            }
        }

        // Use this for initialization
        void Start() {
            StartCoroutine(registerObjects());
        }

        // Load in after a delay so that characters are loaded in first
        private IEnumerator registerObjects() {
            yield return new WaitForSeconds(0.5f);
            registerHighlightedObjects();
            registerPlayerController();
            registerCameraRaycast();
        }
        
        bool playerCharacterChanged() {
            return currentPlayerCharacter != playerControl.GetCurrentCharacter();
        }
    }

}