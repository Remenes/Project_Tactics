using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tactics.Characters {

    public abstract class AbilityConfig : ScriptableObject {

        [Header("Ability Type")]
        [SerializeField] protected bool isAOE = false;
        public bool IsAOE { get { return isAOE; } }
        //[SerializeField] private bool requiresTarget = true; //TODO make this useful

        [Header("Stats Details")]
        [SerializeField] private int actionPointsNeeded = 1;
        public int GetActionPointsNeeded() { return actionPointsNeeded; }
        [SerializeField] private AnimationClip alternateAbilityAnimation;
        public AnimationClip GetAbilityAnimation() { return alternateAbilityAnimation; }

        //protected AbilityBehavior abilityBehavior;

        //public AbilityBehavior GetAbilityBehavior() { return abilityBehavior; }

        // Simply adds the correct behavior component to the object
        protected abstract AbilityBehavior AddAbilityBehavior(GameObject objectToAddTo);

        public AbilityBehavior AttachAbilityBehaviorTo(GameObject objectToAddTo) {
            AbilityBehavior newBehavior = AddAbilityBehavior(objectToAddTo);
            newBehavior.SetConfig(this);
            return newBehavior;
        }



    }

}