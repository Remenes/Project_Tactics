using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tactics.Characters {

    [RequireComponent(typeof(Character))]
    public class WeaponSystem : MonoBehaviour {

        private const string AttackAnim = "AttackAnim";
        private const string AttackTrigger = "Attack";

        [SerializeField] private Weapon weaponInUse;

        [SerializeField] private BasicMeleeConfig meleeConfig;
        BasicMeleeBehavior meleeBehavior;

        // Use this for initialization
        void Start() {
            registerAbilities();
        }

        private void registerAbilities() {
            meleeBehavior = meleeConfig.AttachAbilityBehaviorTo(this.gameObject) as BasicMeleeBehavior;
        }

        public void Attack_BasicMelee(Character target) {
            meleeBehavior.Use(target, weaponForAnimation: weaponInUse);
        }

        public Weapon GetCurrentWeapon() {
            return weaponInUse;
        }
    }

}