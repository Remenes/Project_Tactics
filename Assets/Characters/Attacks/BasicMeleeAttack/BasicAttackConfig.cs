using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tactics.Characters {

    [CreateAssetMenu(menuName = "Tactics/Abilities/BasicAttack")]
    public class BasicAttackConfig : AbilityConfig {

        protected override AbilityBehavior AddAbilityBehavior(GameObject objectToAddTo) {
            if (!isAOE) {
                return objectToAddTo.AddComponent<BasicAttackBehavior>();
            }
            else {
                return objectToAddTo.AddComponent<BasicAOEBehavior>();
            }
        }

    }

}