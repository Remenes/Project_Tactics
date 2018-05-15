using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tactics.Characters;
using Tactics.Grid;

namespace Tactics.Controller {

    public class EnemyController : MonoBehaviour {

        Character enemy;
        public Character getCurrentEnemyCharacter() { return enemy; }
        EnemyMeleeAI enemyAI;

        Character playerTarget;


        private bool turnFinished = false;
        public bool getTurnFinished() { return turnFinished; }
        public void ResetCharacterTurn() {
            turnFinished = false;
            enemy.ResetCharacterState();
        }

        private bool executing = false;

        // Use this for initialization
        void Start() {
            enemy = GameObject.FindGameObjectWithTag("Enemy").GetComponent<Character>();
            enemyAI = enemy.GetComponent<EnemyMeleeAI>();
            playerTarget = GameObject.FindGameObjectWithTag("Player").GetComponent<Character>();
        }

        // Update is called once per frame

        void Update() {
            controlEnemy();
        }

        private void controlEnemy() {
            if (enemy.GetCharacterState() == State.FINISHED) {
                turnFinished = true;
                executing = false;
            }
            if (turnFinished || executing)
                return;
            //TODO make enemies working again
            print("Enemy State: " + enemy.GetCharacterState());
            if (enemy.GetCharacterState() == State.IDLE) {
                if (enemy.CanAttackTarget(playerTarget)) {

                    enemy.QueueAttackTarget(playerTarget);
                    StartCoroutine(enemy.ExecuteActions());
                    executing = true;
                }
                else if (enemy.CanMove()) {
                    List<Cell> pathToPlayer = enemyAI.GetWantedPath(playerTarget.GetCellLocation()); //getPathTowards(playerTarget.GetCellLocation(), enemy.getMovementDistance());
                    enemy.QueueMovementAction(pathToPlayer);
                }
                else {
                    StartCoroutine(enemy.ExecuteActions());
                    executing = true;
                }
            }
            
        }


    }
}