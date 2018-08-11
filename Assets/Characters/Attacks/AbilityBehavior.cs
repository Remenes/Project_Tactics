using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tactics.Grid;

namespace Tactics.Characters {

    public abstract class AbilityBehavior : MonoBehaviour {

        protected const string AttackAnim = "AttackAnim";
        protected const string AttackTrigger = "Attack";

        protected const string PLAYER = "Player";
        protected const string ENEMY = "Enemy";

        private const int OBSTACLE_LAYER = 10;
        protected LayerMask OBSTACLE_LAYER_MASK;

        protected Animator animator;
        protected Character character;
        protected Weapon weaponInUse;

        protected AbilityConfig abilityConfig;
        public bool IsAOE { get { return abilityConfig.IsAOE; } }
        public bool UseMouseLocation { get { return abilityConfig.UseMouseLocation; } }

        protected HashSet<Character> targetsInRange;

        public void SetConfig(AbilityConfig toConfigureTo) {
            //TODO Account for other attributes such as sound and particles
            abilityConfig = toConfigureTo;
            character = GetComponent<Character>();
            animator = GetComponent<Animator>();
            weaponInUse = GetComponent<WeaponSystem>().GetCurrentWeapon();
            targetsInRange = new HashSet<Character>();
            OBSTACLE_LAYER_MASK = 1 << OBSTACLE_LAYER;
        }
    
        //TODO change to abstracts so behaviours need both

        //TODO maybe change this to IEnumerator so that time can be managed better
        /// <summary>
        /// 
        /// </summary>
        /// <param name="weapon"> Pass in a weapon if you need to change animation to be with the weapon </param>
        public virtual void Use(Character target, Weapon weaponForAnimation = null) {
            throw new System.Exception("Using an ability with target as a parameter, but no target specified");
        }
        public virtual void Use(Cell originPos, Weapon weaponForAnimation = null) {
            throw new System.Exception("Using an ability with an originPos as a parameter, but no origin position specified");
        }

        protected void overrideAttackAnimation(AnimationClip anotherAnimation = null) {
            animator.runtimeAnimatorController = character.GetOverrideController();
            character.GetOverrideController()[AttackAnim] = anotherAnimation == null ? abilityConfig.GetAbilityAnimation() : anotherAnimation;
        }
        
        protected void lookAtTarget(Transform target) {
            lookAtTarget(target.position);
        }

        protected void lookAtTarget(Vector3 targetPos) {
            Vector3 lookAtPos = targetPos;
            lookAtPos.y = transform.position.y;
            transform.LookAt(lookAtPos);
        }

        protected bool targetObstructed(Character target) {
            // Use the offset to start raycast from somewhere above the current cell
            Vector3 upOffset = Vector3.up * Grid.GridSpace.cellSize; 
            Vector3 originCast = character.GetCellLocation().transform.position + upOffset;
            Vector3 targetCast = target.GetCellLocation().transform.position + upOffset;
            Vector3 directionToTarget = targetCast - originCast;

            return Physics.Raycast(originCast, directionToTarget, directionToTarget.magnitude, OBSTACLE_LAYER_MASK);
        }

        // Returns the targets in range. Only returns what was previously set, so be sure to set the targets before calling this getter function
        public HashSet<Character> GetTargetsInRange() {
            return targetsInRange;
        }

        // Only one reset target function needs to be overriden: TODO: change to abstracts and that behaviours require both

        // Sets the targets in range for this ability. This is so repeated calls of the getter will be quick, so call this whenever a character changes cell location
        public virtual void ResetTargetsInRange() {
            throw new System.Exception("No default ResetTargets implemented");
        }
        // Sets the targets in range for this ability using another origin position. This is so repeated calls of the getter will be quick, so call this whenever a character changes cell location
        public virtual void ResetTargetsInRange(Cell otherOriginPosition) {
            throw new System.Exception("No ResetTargets implemented for another origin position");
        }
    }

}