using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace Tactics.Characters {

    public class ShowMovesLeft : MonoBehaviour {

        [SerializeField] private RawImage movesLeftIcon;
        [SerializeField] private float distanceBetweenIcons;

        private Character character;
        private int maxNumIcons;

        private RawImage[] images;

        // Use this for initialization
        void Start() {
            character = transform.GetComponentInParent<Character>();
            maxNumIcons = character.GetMaxTurns();

            initializeIcons();
        }

        private void initializeIcons() {
            images = new RawImage[maxNumIcons];
            for (int i = 0; i < maxNumIcons; i++) {
                images[i] = Instantiate(movesLeftIcon, transform);
                Vector3 newPosition = new Vector3(i * distanceBetweenIcons, 0, 0);
                images[i].rectTransform.localPosition = newPosition;
            }
        }

        // Update is called once per frame
        void Update() {
            displayIcons();
        }

        private void displayIcons() {
            if (character.GetCharacterState() == State.FINISHED)
                return;
            int numTurnsLeft = character.GetNumTurnsLeft();

            for (int i = 0; i < maxNumIcons; i++) {
                images[i].enabled = i < numTurnsLeft;
            }
        }
    }

}