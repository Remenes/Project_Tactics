using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tactics.Characters {

    public abstract class AbilityBehavior : MonoBehaviour {

        protected const string AttackAnim = "AttackAnim";
        protected const string AttackTrigger = "Attack";

        protected Animator animator;
        protected Character character;

        protected AbilityConfig abilityConfig;

        public void SetConfig(AbilityConfig toConfigureTo) {
            //TODO Account for other attributes such as sound and particles
            abilityConfig = toConfigureTo;
            character = GetComponent<Character>();
            animator = GetComponent<Animator>();
        }

        //TODO maybe change this to IEnumerator so that time can be managed better
        /// <summary>
        /// 
        /// </summary>
        /// <param name="weapon"> Pass in a weapon if you need to change animation to be with the weapon </param>
        public abstract void Use(Weapon weaponForAnimation = null);

        protected void overrideAttackAnimation(AnimationClip anotherAnimation = null) {
            animator.runtimeAnimatorController = character.GetOverrideController();
            character.GetOverrideController()[AttackAnim] = anotherAnimation == null ? abilityConfig.GetAbilityAnimation() : anotherAnimation;
        }

    }

}