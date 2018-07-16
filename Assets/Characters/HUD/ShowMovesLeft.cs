using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace Tactics.Characters {
    
    // TODO make this a controller class so that is looks through all characters and adjusts UI accordingly,
    // rather than every character adjustsing their own UI. Possibly, make one UI controller for each "faction" 
    // so each "faction" can know their own specific character UIs.
    public class ShowMovesLeft : MonoBehaviour {

        private enum Points { MOVES, ACTIONS }
        private enum StartLocation { LEFT, RIGHT }

        [SerializeField] private Points poinstType;
        [SerializeField] private StartLocation startLocation;
        [SerializeField] private RawImage movesLeftIcon;
        [SerializeField] private float distanceBetweenIcons;

        private Character character;
        private int maxNumIcons;
        private Vector2 pivotPoint;
        private Rect rect;

        private RawImage[] images;

        private delegate float IconPosition(int index);
        private IconPosition iconPositionFormula;

        // Use this for initialization
        void Start() {
            character = transform.GetComponentInParent<Character>();
            maxNumIcons = character.GetMaxMoves();
            pivotPoint = GetComponent<RectTransform>().pivot;
            rect = GetComponent<RectTransform>().rect;

            initializeIcons();
        }
        
        private void initializeIcons() {
            images = new RawImage[maxNumIcons];
            if (startLocation == StartLocation.LEFT) {
                iconPositionFormula += iconPosFromLeft;
            }
            else {
                iconPositionFormula += iconPosFromRight;
            }

            for (int i = 0; i < maxNumIcons; i++) {
                images[i] = Instantiate(movesLeftIcon, transform);
                Vector3 newPosition = new Vector3(iconPositionFormula(i), 0, 0);
                images[i].rectTransform.localPosition = newPosition;
            }
        }

        // Update is called once per frame
        void Update() {
            displayIcons();
        }

        // TODO: think about putting this as a delegate and call this only when necessary to help performance
        private void displayIcons() {
            switch (poinstType) {
                case Points.ACTIONS:
                    displayActionsLeft();
                    break;
                case Points.MOVES:
                    displayMovesLeft();
                    break;
            }
        }

        private void displayMovesLeft() {
            int numTurnsLeft = character.GetNumMovesLeft();

            for (int i = 0; i < maxNumIcons; i++) {
                images[i].enabled = i < numTurnsLeft;
            }
        }

        private void displayActionsLeft() {
            int numActionsLeft = character.GetNumActionsLeft();

            for (int i = 0; i < maxNumIcons; i++) {
                images[i].enabled = i < numActionsLeft;
            }
        }

        private float iconPosFromLeft(int index) {
            return index * distanceBetweenIcons - pivotPoint.x * rect.width;
        }

        private float iconPosFromRight(int index) {
            print("Transform:" + transform.localPosition.x.ToString());
            print("Looking here: " + (pivotPoint.x - ((index + 1) * distanceBetweenIcons)).ToString());
            return (1 - pivotPoint.x) * rect.width - ((index + 1) * distanceBetweenIcons);
        }

    }

}