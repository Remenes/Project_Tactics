using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tactics.Characters {

    [CreateAssetMenu(menuName = "Tactics/Abilities/BasicMelee")]
    public class BasicMeleeConfig : AbilityConfig {

        [SerializeField] private bool useWeaponDmg = true;
        [SerializeField] private int additionalDmg;
        [SerializeField] private bool useWeaponRange = true;
        [SerializeField] private float additionalRange;

        public bool UseWeaponDmg { get { return useWeaponDmg; } }
        public bool UseWeaponRange { get { return useWeaponRange; } }
        public int GetAdditionalDmg { get { return additionalDmg; } }
        public float GetAdditionalRange { get { return additionalRange; } }

        protected override AbilityBehavior AddAbilityBehavior(GameObject objectToAddTo) {
            return objectToAddTo.AddComponent<BasicMeleeBehavior>();
        }

    }

}