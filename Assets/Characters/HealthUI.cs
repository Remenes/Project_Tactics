using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tactics.Characters {

    public class HealthUI : MonoBehaviour {

        RawImage image;
        Health health;

        void Start() {
            image = GetComponent<RawImage>();    
        }

        public void AttachToHealth(Health attachTo) {
            health = attachTo;
        }

        // Called by other classes (namely Health) to update the UI of the healthbar
        public void UpdateHealthBar() {
            if (!health) {
                throw new System.Exception("HealthUI not linked to an actual Health class. \n Make sure the Health class is present in the actual character parent.");
            }
            
            image.uvRect = newOffset();
        }

        // Calculation used to calculate the new offset
        private Rect newOffset() {
            Rect newOffset = image.uvRect;
            newOffset.x = 0.5f - 0.5f * (health.healthAsPercentage);
            return newOffset;
        }

    }

}