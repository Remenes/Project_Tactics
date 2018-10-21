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
        [SerializeField] private GameObject aoeHighlightIndicator;
        [SerializeField] private GameObject enemyTargetIndicator;
        [SerializeField] private GameObject enemyInRangeIndicator;

        private GameObject cursorHighlight;
        // For assosciating a highlight with every enemy/player and can be simply enabled/disabled when the time comes
        private Dictionary<Character, GameObject> enemyHighlights;
        private Dictionary<Character, GameObject> playerHighlights;

        EnemyController enemyControl;
        PlayerController playerControl;
        Character currentPlayerCharacter;
        AbilityConfig CurrentAbilityConfig { get { return playerControl.GetCurrentAbility(); } }

        private Character targettingEnemy = null;


        // ---------------------------------------------------------------------------------------
        // ---------------- Helper Functions for changing Highlight -------------------------------
        // ---------------------------------------------------------------------------------------

        private void signalAllHighlightDisable() {
            disableHighlight(cursorHighlight);
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
                highlightEnemiesInCurrentAbilityRange();
            }

            if (cellHasEnemyCharacter) {
                // If the current character can attack the target, switch to the attack indicator if need be
                //if (currentPlayerCharacter.CanAttackTarget(enemyOnCell)) {
                if (playerControl.CurrCharacterCanTarget(enemyOnCell)) {
                    if (!targettingEnemy || targettingEnemy != enemyOnCell) {
                        targettingEnemy = cell.GetCharacterOnCell();
                        switchHighlight(ref cursorHighlight, enemyTargetIndicator);
                        disableHighlight(enemyHighlights[enemyOnCell]);
                    }
                }
                // Else, show no indicator, but switch the highlight to the normal indicator
                else {
                    switchHighlight(ref cursorHighlight, highlightIndicator);
                    cursorHighlight.SetActive(false);
                }
            }
        }

        private void switchToAOEHighlight(int newAbilityIndex, Cell cellPosition) {
            switchHighlight(ref cursorHighlight, aoeHighlightIndicator);
            Vector3 newScale = cursorHighlight.transform.localScale;
            // 2 times the range + 1, since the scale is in diameter and the range is in radius, and 1 is needed for the center
            newScale.x = newScale.z = playerControl.GetCurrentAbilityAOEEffectRange() * 2 + 1;
            cursorHighlight.transform.localScale = newScale;
            // Still highlight enemies in range if it doesn't use mouse location, and then set the cursor highlight to the character
            if (!CurrentAbilityConfig.UseMouseLocation) {
                highlightEnemiesInRange(newAbilityIndex);
                setHighlightActivePosition(cursorHighlight, cellPosition);
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
        private void highlightEnemiesInCurrentAbilityRange() {
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

        //private void updateOnUsingAOEAbility(Cell cell) {
        //    if (playerControl.IsUsingAbility() && playerControl.GetCurrentAbility().IsAOE) {
        //        foreach (Character targetEnemy in playerControl.GetTargetsOfCurrentAbility()) {
        //            switchHighlight(enemyHighlights, targetEnemy, enemyTargetIndicator);
        //        }
        //    }
        //}

        private void updateOnCellEntered(Cell cell) {
            Character characterOnCell = cell.GetCharacterOnCell();
            bool cellHasPlayerCharacter = characterOnCell && 
                                          characterOnCell.CompareTag(PlayerController.PLAYER_TAG);
            bool cellHasEnemyCharacter = characterOnCell &&
                                          characterOnCell.CompareTag(EnemyController.ENEMY_TAG);

            if (currentPlayerCharacter.FinishedActionQueue() || cellHasPlayerCharacter) {
                cursorHighlight.SetActive(false);
                return;
            }
            updateOnAOEAbilityOrNot(cell);
            // Don't switch on enemy entered if the current ability is an aoe ability
            bool usingAOEAbility = playerControl.IsUsingAbility() && playerControl.GetCurrentAbility().IsAOE;
            bool usingTargettedAbility = playerControl.IsUsingAbility() && playerControl.GetCurrentAbility().RequiresTarget;
            if (!usingAOEAbility) {
                switchToEnemyIndicator(cell);
            }
            // But if the ability is a target ability also, then show the aoe indicator if there's a target and that target's in range
            else if (usingTargettedAbility && cellHasEnemyCharacter && currentAbilityInRangeOfCell(cell)) {
                targettingEnemy = characterOnCell;
                switchToAOEHighlight(playerControl.CurrentAbilityIndex(), cell);
            }
            // Don't set the position if the current ability is AOE and doesn't use mouse location
            bool usingMouseLocation = playerControl.IsUsingAbility() && playerControl.GetCurrentAbility().UseMouseLocation;
            if (usingAOEAbility && !usingMouseLocation) {
                cursorHighlight.SetActive(true);
                return;
            }
            else {
                setHighlightActivePosition(cursorHighlight, cell);
            }

            // If the ability is AOE and uses mouse location, make sure the target is in range to show the aoe highlight
            if (usingAOEAbility && usingMouseLocation) {
                if (!currentAbilityInRangeOfCell(cell)) {
                    disableHighlight(cursorHighlight);
                }
            }

            if (!cellHasEnemyCharacter) {
                if (targettingEnemy) {
                    targettingEnemy = null;
                    switchHighlight(ref cursorHighlight, highlightIndicator);
                }
            }

        }

        // Highlights enemies if the player is on an AOE ability of not
        private void updateOnAOEAbilityOrNot(Cell cell) {
            Character characterOnCell = cell.GetCharacterOnCell();
            bool cellHasTargetButCantAttack = cell.GetCharacterOnCell() &&
                                          //!currentPlayerCharacter.CanAttackTarget(characterOnCell);
                                          !playerControl.CurrCharacterCanTarget(characterOnCell);
            if (playerControl.IsUsingAbility()) {
                if (!playerControl.GetCurrentAbility().IsAOE) {
                    if (cellHasTargetButCantAttack) {
                        cursorHighlight.SetActive(false);
                        return;
                    }
                }
                else if (currentAbilityInRangeOfCell(cell)) {
                    highlightEnemiesInCurrentAbilityRange();
                }
            }
        }

        // Helper for seeing if the current cell is within player range
        private bool currentAbilityInRangeOfCell(Cell cell) {
            int currentAbilityIndex = playerControl.CurrentAbilityIndex();
            return playerControl.IsUsingAbility() && currentPlayerCharacter.InAbilityRangeOfTarget(cell, currentAbilityIndex);
        }

        // Update when the player changes abilities
        private void updateOnPlayerChangeAbilities(int newAbilityIndex) {
            // Reset the cursor highlight when the player changes ability
            switchHighlight(ref cursorHighlight, highlightIndicator);
            // Re-update normal targets if player changed to basic attack
            if (!playerControl.IsUsingAbility()) {
                highlightEnemiesInRange();
                return;
            }
            AbilityConfig currentAbilityConfig = currentPlayerCharacter.GetAbilityConfig(newAbilityIndex);

            // Highlight Enemies in Range initially only if the ability requires a target
            if (currentAbilityConfig.RequiresTarget) {
                highlightEnemiesInRange(newAbilityIndex);
            }
            // Otherwise, check if the ability is an AOE, and if it is, show the aoe ability indicator
            else if (currentAbilityConfig.IsAOE) {
                switchToAOEHighlight(newAbilityIndex, currentPlayerCharacter.GetCellLocation());
            }
        }

        private void updateOnCellExit(Cell cellLeft) {
            // Don't deactivate highlight if the ability is AOE and doesn't UseMouseLocation
            if (playerControl.IsUsingAbility() && CurrentAbilityConfig.IsAOE && !CurrentAbilityConfig.UseMouseLocation)
                return;
            cursorHighlight.SetActive(false);
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
                switchHighlight(ref cursorHighlight, highlightIndicator);
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
            
            highlightEnemiesInCurrentAbilityRange();
            // If the character is using an aoe ability without mouse location, move it back to where the player is
            if (playerControl.IsUsingAbility() && CurrentAbilityConfig.IsAOE && !CurrentAbilityConfig.UseMouseLocation) {
                setHighlightActivePosition(cursorHighlight, currentPlayerCharacter.GetCellLocation());
            }
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
            playerControl.PlayerChangedAbilityObservers += updateOnPlayerChangeAbilities;
            signalPlayersReactivateHighlights();
        }

        private void registerCameraRaycast() {
            CameraRaycast cameraRaycast = Camera.main.GetComponent<CameraRaycast>();
            cameraRaycast.mouseOverCellObservers += updateOnCellEntered;
            cameraRaycast.mouseExitCellObservers += updateOnCellExit;
        }

        private void registerHighlightedObjects() {
            cursorHighlight = Instantiate(highlightIndicator);
            cursorHighlight.SetActive(false);
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