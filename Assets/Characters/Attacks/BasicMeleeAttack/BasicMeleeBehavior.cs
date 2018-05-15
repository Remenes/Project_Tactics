using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tactics.Characters {

    public class BasicMeleeBehavior : AbilityBehavior {

        protected BasicMeleeConfig config;

        void Start() {
            config = abilityConfig as BasicMeleeConfig;
        }

        public override void Use(Character target, Weapon weaponForAnimation = null) {
            //Override Animation with Weapon animation, since basic attacks should use the weapon's animation
            overrideAnimationWithWeapon(weaponForAnimation);

            lookAtTarget(target.transform);
            
            animator.SetTrigger(AttackTrigger);
        }

        private void overrideAnimationWithWeapon(Weapon weaponForAnimation) {
            if (weaponForAnimation == null)
                overrideAttackAnimation();
            else
                overrideAttackAnimation(weaponForAnimation.GetAnimationClip());
        }



    }

}