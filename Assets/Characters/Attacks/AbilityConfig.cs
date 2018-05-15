using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tactics.Characters {

    public abstract class AbilityConfig : ScriptableObject {

        [SerializeField] private AnimationClip alternateAbilityAnimation;
        public AnimationClip GetAbilityAnimation() { return alternateAbilityAnimation; }

        //protected AbilityBehavior abilityBehavior;

        //public AbilityBehavior GetAbilityBehavior() { return abilityBehavior; }

        protected abstract AbilityBehavior AddAbilityBehavior(GameObject objectToAddTo);

        public AbilityBehavior AttachAbilityBehaviorTo(GameObject objectToAddTo) {
            AbilityBehavior newBehavior = AddAbilityBehavior(objectToAddTo);
            newBehavior.SetConfig(this);
            return newBehavior;
        }



    }

}