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

        public override void Use(Weapon weaponForAnimation = null) {
            if (weaponForAnimation == null)
                overrideAttackAnimation();
            else
                overrideAttackAnimation(weaponForAnimation.GetAnimationClip());
            animator.SetTrigger(AttackTrigger);
        }

    }

}