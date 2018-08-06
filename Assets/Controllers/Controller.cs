using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tactics.CameraUI;
using Tactics.Grid;
using Tactics.Characters;

namespace Tactics.Controller {

    public abstract class Controller : MonoBehaviour {

        protected Character[] characters;
        protected int currCharacterIndex;
        public Character[] GetCharacters() { return characters; }
        public Character GetCurrentCharacter() { return characters[currCharacterIndex]; }
        protected Character currentCharacter { get { return GetCurrentCharacter(); } }

        protected bool executingActions = false;

        protected bool turnFinished = false;
        public bool GetTurnFinished() { return turnFinished; }
        public bool GetExecutingActions() { return executingActions; }

        protected abstract void Awake();
        protected abstract void Update();

        // Assigns the Character list to contain all of the characters that have their corresponding tag,
        // along with whether they start in the finished state or not
        protected void registerCharacters(string tag, bool startFinished) {
            GameObject[] characterObjects = GameObject.FindGameObjectsWithTag(tag);

            characters = new Character[characterObjects.Length];
            for (int i = 0; i < characterObjects.Length; ++i) {
                characters[i] = characterObjects[i].GetComponent<Character>();
                if (startFinished) {
                    characters[i].EndTurn();
                }
            }
            currCharacterIndex = 0;
        }

        // Checks if all characters owned have the corresponding state
        protected bool compareAllCharactersStateTo(State state) {
            foreach (Character character in characters) {
                if (character.GetCharacterState() != state) {
                    return false;
                }
            }
            return true;
        }

        // Checks if all characters are either IDLE or FINISHED
        protected bool charactersDoneWithActions() {
            foreach (Character character in characters) {
                if (!character.IsFinished() && !character.IsIDLE()) {
                    return false;
                }
            }
            return true;
        }

        // Increments the current index by 1
        protected void incCurrentIndex() {
            changeCurrentIndex(1);
        }

        // Decrements the current index by 1
        protected void decCurrentIndex() {
            changeCurrentIndex(-1);
        }

        // Helper function to keep the index in range when incrementing or decrementing
        private void changeCurrentIndex(int direction) {
            int amount = direction > 0 ? 1 : -1;
            currCharacterIndex += amount;
            currCharacterIndex %= characters.Length;
        }
        /*
        // Perform a move or attack depending on which cell was checked
        private void inputActionCommand() {
            Character target = highlighedCell.GetCharacterOnCell();
            if (target != null) {
                if (getCurrentPlayerCharacter().CanAttackTarget(target)) {
                    getCurrentPlayerCharacter().QueueAttackTarget(target);
                }
            }
            else if (getCurrentPlayerCharacter().CanMove() &&
                getCurrentPlayerCharacter().WithinMovementRangeOf(highlighedCell)) {
                List<Cell> path = GridSpace.GetPathFromLinks(
                    getCurrentPlayerCharacter().GetPossibleMovementLocations(),
                    getCurrentPlayerCharacter().GetCellLocation(), highlighedCell);
                getCurrentPlayerCharacter().QueueMovementAction(path);
            }
        }
        */
        
        // Returns a list of the path torwards the targeted character using a *random* location next to them
        public List<Cell> FindPathTowardsAdjacentCharacter(Cell cellWithCharacter) {
            HashSet<Cell> surroundingCells = cellWithCharacter.getAllSurroundingCells();
            foreach (Cell cell in surroundingCells) {
                if (GetCurrentCharacter().WithinMovementRangeOf(cell)) {
                    return GridSpace.GetPathFromLinks(
                        GetCurrentCharacter().GetPossibleMovementLocations(),
                        GetCurrentCharacter().GetCellLocation(), cell);
                }
            }
            return null;
        }

        // Resets the controller's turn and all their characters
        public virtual void ResetTurn() {
            turnFinished = false;
            foreach (Character character in characters) {
                character.ResetCharacterState();
            }
        }

    }
}
