using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tactics.Characters {

    [RequireComponent(typeof(Character))]
    public class WeaponSystem : MonoBehaviour {

        private const string AttackAnim = "AttackAnim";
        private const string AttackTrigger = "Attack";

        [Header("Basic Weapon")]
        [SerializeField] private Weapon weaponInUse;
        [SerializeField] private BasicMeleeConfig meleeConfig;
        BasicMeleeBehavior meleeBehavior;

        [Header("Abilities")]
        [SerializeField] private AbilityConfig[] abilityConfigs;
        private AbilityBehavior[] abilityBehaviors;

        // Use this for initialization
        void Start() {
            registerAbilities();
        }

        private void registerAbilities() {
            meleeBehavior = meleeConfig.AttachAbilityBehaviorTo(this.gameObject) as BasicMeleeBehavior;
            abilityBehaviors = new AbilityBehavior[abilityConfigs.Length];
            for (int i = 0; i < abilityConfigs.Length; i++) {
                AbilityConfig config = abilityConfigs[i];
                abilityBehaviors[i] = config.AttachAbilityBehaviorTo(this.gameObject);
            }
        }

        public void Attack_BasicMelee(Character target) {
            meleeBehavior.Use(target, weaponForAnimation: weaponInUse);
        }

        public void Attack_Ability(Character target, int abilityIndex) {
            if (abilityIndex < 0 && abilityIndex < abilityConfigs.Length)
                throw new System.Exception("Can't Use Ability: Ability Index must be a valid non-negative number and less than the length of the number of abilities");
            abilityBehaviors[abilityIndex].Use(target);
        }

        public HashSet<Character> GetTargets_BasicMelee() {
            return meleeBehavior.GetTargetsInRange();
        }

        public HashSet<Character> GetTargets_Ability(int abilityIndex) {
            if (abilityIndex < 0 && abilityIndex < abilityConfigs.Length)
                throw new System.Exception("Can't get ability targets: Ability Index must be a valid non-negative number and less than the length of the number of abilities");
            return abilityBehaviors[abilityIndex].GetTargetsInRange();
        }

        public AbilityConfig GetAbilityConfig(int abilityIndex) {
            return abilityConfigs[abilityIndex];
        }

        public AbilityConfig GetBasicAttackConfig() {
            return meleeConfig;
        }

        public AbilityConfig[] GetAllAbilityConfigs() {
            return abilityConfigs;
        }

        // Resets targets for all abilities
        public void ResetTargets() {
            meleeBehavior.ResetTargetsInRange();
            foreach (AbilityBehavior ability in abilityBehaviors) {
                ability.ResetTargetsInRange();
            }
        }

        public Weapon GetCurrentWeapon() {
            return weaponInUse;
        }
    }

}