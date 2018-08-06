using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tactics.Characters;
using Tactics.Grid;

namespace Tactics.Controller {

    public class EnemyController : Controller {
        
        public static string ENEMY_TAG = "Enemy";

        PlayerController playerControl;
        
        // Use this for initialization
        protected override void Awake() {
            playerControl = GameObject.FindObjectOfType<PlayerController>();
            registerCharacters(ENEMY_TAG, true);
        }

        // Update is called once per frame
        protected override void Update() {
            controlEnemies();
        }

        // Function that is called every frame to control all the enemies actions
        private void controlEnemies() {
            if (compareAllCharactersStateTo(State.FINISHED)) {
                turnFinished = true;
            }
            if (charactersDoneWithActions()) {
                executingActions = false;
            }
            if (turnFinished || executingActions)
                return;

            for (int index = 0; index < characters.Length; ++index) {
                currCharacterIndex = index;
                assignEnemyActions();
                currentCharacter.ExecuteActions();
            }
            executingActions = true;
        }

        // Assigns the currentCharacter's actions
        // TODO: make this work for enemies of a general type rather than just enemyMeleeAI
        private void assignEnemyActions() {
            Character enemy = currentCharacter;

            //TODO integrate all this into just the enemyAI component and call one function there
            EnemyMeleeAI enemyAI = currentCharacter.GetComponent<EnemyMeleeAI>();
            Character target = getClosestTarget();
            while (enemy.CanMove()) { 
                if (enemy.InRangeOfTarget(target)) {
                    if (enemy.CanAttackTarget(target)) {
                        enemy.QueueAttackTarget(target);
                    }
                    else {
                        enemy.EndTurn();
                    }
                }
                else if (enemy.CanMove()) {
                    List<Cell> pathToPlayer = enemyAI.GetWantedPath(target.GetCellLocation()); //getPathTowards(playerTarget.GetCellLocation(), enemy.getMovementDistance());
                    enemy.QueueMovementAction(pathToPlayer);
                }
                else {
                    enemy.EndTurn();
                }
            }
        }

        private Character getClosestTarget() {
            float closestDistSqr = int.MaxValue;
            Character closestTarget = null;
            Vector3 enemyPosition = GetCurrentCharacter().transform.position;
            foreach (Character playerChar in playerControl.GetCharacters()) {
                Vector3 playerCharPos = playerChar.transform.position;
                float distanceBetween = Vector3.SqrMagnitude(playerCharPos - enemyPosition);
                if (distanceBetween < closestDistSqr) {
                    closestDistSqr = distanceBetween;
                    closestTarget = playerChar;
                }
            }
            return closestTarget;
        }
        
    }
}