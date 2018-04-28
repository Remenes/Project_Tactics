using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tactics.Characters {

    [RequireComponent(typeof(Character))]
    public class WeaponSystem : MonoBehaviour {

        private const string AttackAnim = "AttackAnim";
        private const string AttackTrigger = "Attack";

        [SerializeField] private Weapon weaponInUse;
        private Character character;
        private Animator animator;

        [SerializeField] private BasicMeleeConfig meleeConfig;
        BasicMeleeBehavior meleeBehavior;

        // Use this for initialization
        void Start() {
            character = GetComponent<Character>();
            animator = GetComponent<Animator>();
            registerAbilities();
        }

        private void registerAbilities() {
            meleeBehavior = meleeConfig.AttachAbilityBehaviorTo(this.gameObject) as BasicMeleeBehavior;
        }

        public void Attack_BasicMelee() {
            meleeBehavior.Use(weaponForAnimation: weaponInUse);
        }

        public Weapon GetCurrentWeapon() {
            return weaponInUse;
        }
    }

}