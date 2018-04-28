using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tactics.Characters {

    [CreateAssetMenu(menuName = "Tactics/Abilities/BasicMelee")]
    public class BasicMeleeConfig : AbilityConfig {
        
        protected override AbilityBehavior AddAbilityBehavior(GameObject objectToAddTo) {
            return objectToAddTo.AddComponent<BasicMeleeBehavior>();
        }

    }

}