using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tactics.Characters {

    public class Health : MonoBehaviour {

        private int currentHealth;
        [SerializeField] private int maxHealth = 5;
        public int healthValue { get { return currentHealth; } }
        public float healthAsPercentage { get { return currentHealth / (float)maxHealth; } }
        public int maxHealthValue { get { return maxHealth; } }
        public bool isDead { get { return currentHealth == 0; } }

        public void TakeDamage(int damageAmount) {
            currentHealth = Mathf.Clamp(currentHealth -= damageAmount, 0, maxHealth);
        }

        public void HealHealth(int healAmount) {
            currentHealth = Mathf.Clamp(currentHealth += healAmount, 0, maxHealth);
        }

        // Use this for initialization
        void Start() {
            currentHealth = maxHealth;
        }

    }

}