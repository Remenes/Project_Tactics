using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tactics.Controller;
using Tactics.Grid;
using Tactics.Characters;
using System;

namespace Tactics.CameraUI {

    public class PlayerHighlightIndicator : MonoBehaviour {

        [SerializeField] private GameObject playerIndicator;
        [SerializeField] private GameObject playerSelectedIndicator;
        [SerializeField] private GameObject highlightIndicator;
        [SerializeField] private GameObject turnEndedIndicator;
        [SerializeField] private GameObject enemyTargetIndicator;
        [SerializeField] private GameObject enemyInRangeIndicator;

        private GameObject highlightCursorIndicator;
        // For assosciating a highlight with every enemy/player and can be simply enabled/disabled when the time comes
        private Dictionary<Character, GameObject> enemyHighlights;
        private Dictionary<Character, GameObject> playerHighlights;

        EnemyController enemyControl;
        PlayerController playerControl;
        Character currentPlayerCharacter;

        private Character targettingEnemy = null;

        // ---------------------------------------------------------------------------------------
        // ---------------- Helper Functions for changing Highlight -------------------------------
        // ---------------------------------------------------------------------------------------

        private void signalAllHighlightDisable() {
            disableHighlight(highlightCursorIndicator);
            foreach (GameObject highlights in playerHighlights.Values) {
                disableHighlight(highlights);
            }
            foreach (GameObject highlights in enemyHighlights.Values) {
                disableHighlight(highlights);
            }
        }

        // For passing in a variable reference
        private void switchHighlight(ref GameObject highlightInstance, GameObject highlightPrefabToBecome) {
            Transform highlightInstanceTransform = highlightInstance.transform;
            Destroy(highlightInstance);
            highlightInstance = Instantiate(highlightPrefabToBecome);
            highlightInstance.transform.position = highlightInstanceTransform.position;
        }

        // For passing in a key to a dictionary and switching that one highlight specified by the key in the dictionary with the highlightPrefab
        private void switchHighlight(Dictionary<Character, GameObject> groupHighlight, Character key, GameObject highlightPrefabToBecome) {
            Transform highlightInstanceTransform = groupHighlight[key].transform;
            Destroy(groupHighlight[key]);
            groupHighlight[key] = Instantiate(highlightPrefabToBecome);
            groupHighlight[key].transform.position = highlightInstanceTransform.position;
        }

        // For passing in a dictionary to change each highlight in that dictionary
        private void switchGroupHighlight(Dictionary<Character, GameObject> groupHighlight, GameObject highlightPrefabToBecome) {
            List<Character> keys = new List<Character>(groupHighlight.Keys);
            foreach (Character character in keys) {
                Transform highlightInstanceTransform = groupHighlight[character].transform;
                Destroy(groupHighlight[character]);
                groupHighlight[character] = Instantiate(highlightPrefabToBecome);
                groupHighlight[character].transform.position = highlightInstanceTransform.position;
            }
        }

        private void disableHighlight(GameObject highlightInstance) {
            highlightInstance.SetActive(false);
        }

        private void setHighlightActivePosition(GameObject highlight, Cell newCell) {
            highlight.SetActive(true);
            // Add an offset so that the indicator is slightly above the top of the cell;
            Vector3 yOffset = Vector3.up * GridSpace.cellSize / 2 + Vector3.up * Mathf.Epsilon;
            Vector3 newHighlightPos = newCell.transform.position + Vector3.up * GridSpace.cellSize / 2 + yOffset;
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
                highlightEnemiesInAbilityRange();
            }

            if (cellHasEnemyCharacter) {
                // If the current character can attack the target, switch to the attack indicator if need be
                //if (currentPlayerCharacter.CanAttackTarget(enemyOnCell)) {
                if (playerControl.CurrCharacterCanTarget(enemyOnCell)) {
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
        // which highlights enemies in range after the player performs an action.
        // Does not take into account abilities, so use highlightEnemiesInAbilityRange if checking abilities are needed
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

        // Helper function to get called in when the player changes abilities
        // which highlights enemies in range of the corresponding ability
        private void highlightEnemiesInRange(int abilityIndex) {
            print("Now on ability " + abilityIndex);
            // If the player switched to not using an ability, use the default highlight instead
            if (!playerControl.IsUsingAbility()) {
                highlightEnemiesInRange();
                return;
            }
            foreach (Character enemy in enemyControl.GetCharacters()) {
                if (currentPlayerCharacter.CanUseAbilitiesOn(enemy, abilityIndex)) {
                    setHighlightActivePosition(enemyHighlights[enemy], enemy.GetCellLocation());
                }
                else {
                    disableHighlight(enemyHighlights[enemy]);
                }
            }
        }

        // Helper function for general functions. Use this if taking into account the current active abilities to highlight enemies
        private void highlightEnemiesInAbilityRange() {
            highlightEnemiesInRange(playerControl.CurrentAbilityIndex());
        }


        // ---------------------------------------------------------------------------------------
        // ------- Functions to use in events ---------------
        // ---------------------------------------------------------------------------------------

        private void signalPlayersReactivateHighlights() {
            if (playerControl.GetTurnFinished()) {
                return;
            }
            foreach (Character character in playerControl.GetCharacters()) {
                playerHighlights[character].SetActive(true);
                setHighlightActivePosition(playerHighlights[character], character.GetCellLocation());
            }
        }

        private void resetPlayerIndicator() {
            switchGroupHighlight(playerHighlights, playerIndicator);
            switchHighlight(playerHighlights, currentPlayerCharacter, playerSelectedIndicator);
            signalPlayersReactivateHighlights();
            highlightEnemiesInRange();
        }

        private void updateOnCellEntered(Cell cell) {
            Character characterOnCell = cell.GetCharacterOnCell();
            bool cellHasPlayerCharacter = characterOnCell && 
                                          characterOnCell.CompareTag(PlayerController.PLAYER_TAG);
            bool cellHasTargetButCantAttack = cell.GetCharacterOnCell() &&
                                          //!currentPlayerCharacter.CanAttackTarget(characterOnCell);
                                          !playerControl.CurrCharacterCanTarget(characterOnCell);

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
                // TODO: instead of changing highlight, add an animation to it
                if (!currentPlayerCharacter.FinishedActionQueue()) {
                    switchHighlight(playerHighlights, currentPlayerCharacter, playerIndicator);
                }
                currentPlayerCharacter = playerControl.GetCurrentCharacter();
            }
            if (currentPlayerCharacter.FinishedActionQueue()) {
                // Also reset the highlight for the cursor, just in case they were over an enemy at the time
                switchHighlight(ref highlightCursorIndicator, highlightIndicator);
                switchHighlight(playerHighlights, currentPlayerCharacter, turnEndedIndicator);
            }
            else {
                switchHighlight(playerHighlights, currentPlayerCharacter, playerSelectedIndicator);
            }
            if (playerControl.GetTurnFinished() || playerControl.GetExecutingActions()) {
                signalAllHighlightDisable();
                return;
            }
            signalPlayersReactivateHighlights();
            
            highlightEnemiesInAbilityRange();
            // Let's the cursor refresh itself to remove the highlight for the enemy that the cursor is over
            if (currentPlayerCharacter.HasActionPoints()) {
                targettingEnemy = null;
            }
        }

        // ---------------------------------------------------------------------------------------
        // ------------Register necessary objects --------------
        // ---------------------------------------------------------------------------------------

        private void registerPlayerObservers() {
            playerControl.PlayerActionObservers += updateOnPlayerAction;
            playerControl.CharactersFinishedExecutingObservers += signalPlayersReactivateHighlights;
            playerControl.TurnResettedObservers += resetPlayerIndicator;
            playerControl.PlayerChangedAbilityObservers += highlightEnemiesInRange;
            signalPlayersReactivateHighlights();
        }

        private void registerCameraRaycast() {
            CameraRaycast cameraRaycast = Camera.main.GetComponent<CameraRaycast>();
            cameraRaycast.mouseOverCellObservers += updateOnCellEntered;
            cameraRaycast.mouseExitCellObservers += updateOnCellExit;
        }

        private void registerHighlightedObjects() {
            highlightCursorIndicator = Instantiate(highlightIndicator);
            highlightCursorIndicator.SetActive(false);
            registerEnemyHighlights();
            registerPlayerHighlightsAndController();
        }

        private void registerPlayerHighlightsAndController() {
            playerControl = GetComponent<PlayerController>();
            currentPlayerCharacter = playerControl.GetCurrentCharacter();
            playerHighlights = new Dictionary<Character, GameObject>();
            foreach (Character playerChar in playerControl.GetCharacters()) {
                playerHighlights[playerChar] = Instantiate(playerIndicator);
                playerHighlights[playerChar].SetActive(false);
            }
            switchHighlight(playerHighlights, currentPlayerCharacter, playerSelectedIndicator);
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
            registerPlayerObservers();
            registerCameraRaycast();
        }
        
        bool playerCharacterChanged() {
            return currentPlayerCharacter != playerControl.GetCurrentCharacter();
        }
    }

}