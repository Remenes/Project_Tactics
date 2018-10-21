using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tactics.Grid;

namespace Tactics.Characters {

    [RequireComponent(typeof(Character))]
    public class WeaponSystem : MonoBehaviour {

        private const string AttackAnim = "AttackAnim";
        private const string AttackTrigger = "Attack";

        [Header("Basic Weapon")]
        [SerializeField] private Weapon weaponInUse;
        [SerializeField] private BasicAttackConfig meleeConfig;
        BasicAttackBehavior meleeBehavior;

        [Header("Abilities")]
        [SerializeField] private AbilityConfig[] abilityConfigs;
        private AbilityBehavior[] abilityBehaviors;

        // Use this for initialization
        void Start() {
            registerAbilities();
        }

        private void registerAbilities() {
            meleeBehavior = meleeConfig.AttachAbilityBehaviorTo(this.gameObject) as BasicAttackBehavior;
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

        public void Attack_Ability(Cell originPos, int abilityIndex) {
            if (abilityIndex < 0 && abilityIndex < abilityConfigs.Length)
                throw new System.Exception("Can't Use Ability: Ability Index must be a valid non-negative number and less than the length of the number of abilities");
            abilityBehaviors[abilityIndex].Use(originPos);
        }

        public HashSet<Character> GetTargets_BasicMelee() {
            return meleeBehavior.GetTargetsInRange();
        }

        public HashSet<Character> GetTargets_Ability(int abilityIndex) {
            if (abilityIndex < 0 && abilityIndex < abilityConfigs.Length)
                throw new System.Exception("Can't get ability targets: Ability Index must be a valid non-negative number and less than the length of the number of abilities");
            return abilityBehaviors[abilityIndex].GetTargetsInRange();
        }

        public int GetDamage_Ability(int abilityIndex) {
            if (abilityIndex < 0 && abilityIndex < abilityConfigs.Length)
                throw new System.Exception("Can't get ability damage: Ability Index must be a valid non-negative number and less than the length of the number of abilities");
            return abilityBehaviors[abilityIndex].GetDamage();
        }

        public int GetDamage_Basic() {
            return meleeBehavior.GetDamage();
        }

        public float GetRange_Ability(int abilityIndex) {
            if (abilityIndex < 0 && abilityIndex < abilityConfigs.Length)
                throw new System.Exception("Can't get ability range: Ability Index must be a valid non-negative number and less than the length of the number of abilities");
            return abilityBehaviors[abilityIndex].GetRange();
        }

        public float GetRange_Basic() {
            return meleeBehavior.GetRange();
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
        
        // Resets targets for all abilities if the ability's targets are only dependent on the characters's current position
        public void ResetTargets() {
            meleeBehavior.ResetTargetsInRange();
            foreach (AbilityBehavior ability in abilityBehaviors) {
                // If the ability is AOE, the range will get resetted from the mouse through the player controller
                if (!ability.UseMouseLocation) { 
                    ability.ResetTargetsInRange();
                }
            }
        }

        public void ResetTargetsForDifferentOriginAbility(int abilityIndex, Cell newOrigin) {
            if (!abilityConfigs[abilityIndex].UseMouseLocation)
                throw new System.Exception("Resetting targets for ability using mouse location when UseMouseLocation isn't a feature");
            abilityBehaviors[abilityIndex].ResetTargetsInRange(newOrigin);
        }

        // Resets targets for the chosen ability: 

        public Weapon GetCurrentWeapon() {
            return weaponInUse;
        }
    }

}