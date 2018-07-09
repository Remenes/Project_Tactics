using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tactics.Characters {

    public class Health : MonoBehaviour {

        HealthUI[] attachedHealthUIs;

        private int currentHealth;
        [SerializeField] private int maxHealth = 5;
        public int healthValue { get { return currentHealth; } }
        public float healthAsPercentage { get { return currentHealth / (float)maxHealth; } }
        public int maxHealthValue { get { return maxHealth; } }
        public bool isDead { get { return currentHealth == 0; } }

        // Use this for initialization
        void Start() {
            currentHealth = maxHealth;
            linkHealthUIs();
        }

        // For linking all corresponding healthUIs to this health class
        private void linkHealthUIs() {
            attachedHealthUIs = GetComponentsInChildren<HealthUI>();
            foreach (HealthUI hUI in attachedHealthUIs) {
                hUI.AttachToHealth(this);
            }
        }

        private void updateHealthUIs() {
            foreach (HealthUI hUI in attachedHealthUIs) {
                hUI.UpdateHealthBar();
            }
        }

        // ---------------
        // Setter Functions
        // ----------------

        public void TakeDamage(int damageAmount) {
            currentHealth = Mathf.Clamp(currentHealth -= damageAmount, 0, maxHealth);
            updateHealthUIs();
        }

        public void Heal(int healAmount) {
            currentHealth = Mathf.Clamp(currentHealth += healAmount, 0, maxHealth);
            updateHealthUIs();
        }

    }

}