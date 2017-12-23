using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tactics.Controller {

    public class GameManager : MonoBehaviour {

        PlayerController playerController;
        EnemyController enemyController;

        private enum Turn { Enemy, Player }
        private Turn factionTurn = Turn.Player;

        // Use this for initialization
        void Start() {
            playerController = GetComponentInChildren<PlayerController>();
            enemyController = GetComponent<EnemyController>();

            switchTurnTo(factionTurn);
        }

        // Update is called once per frame
        void Update() {
            if (factionTurn == Turn.Player && playerController.getTurnFinished() == true) {
                //Move this out of a loop so that this won't happen when they are already on their respective states
                enemyController.resetStates();
                switchTurnTo(Turn.Enemy);
                factionTurn = Turn.Enemy;
            }
            else if (factionTurn == Turn.Enemy && enemyController.getTurnFinished() == true) {
                switchTurnTo(Turn.Player);
                playerController.resetCharacterTurn();
                factionTurn = Turn.Player;
            }
        }

        private void switchTurnTo(Turn newTurn) {
            print("Turn switcehd to: " + newTurn.ToString());
            if (newTurn == Turn.Player) {
                enemyController.enabled = false;
                playerController.enabled = true;
                
            }
            else {
                enemyController.enabled = true;
                playerController.enabled = false;
            }
        }
    }

}