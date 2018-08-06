using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tactics.Characters {

    public class BasicMeleeBehavior : AbilityBehavior {
        
        protected BasicMeleeConfig config;
        public int GetDamage {
            get {
                return weaponInUse.weaponDamage;
            }
        }

        void Start() {
            config = abilityConfig as BasicMeleeConfig;
        }

        public override void Use(Character target, Weapon weaponForAnimation = null) {
            //Override Animation with Weapon animation, since basic attacks should use the weapon's animation
            overrideAnimationWithWeapon(weaponForAnimation);
            lookAtTarget(target.transform);
            animator.SetTrigger(AttackTrigger);
            StartCoroutine(delayedDamage(target, .5f));
        }

        public override void ResetTargetsInRange() {
            string oppositeTeamTag = this.gameObject.CompareTag(ENEMY) ? PLAYER : ENEMY;
            GameObject[] characters = GameObject.FindGameObjectsWithTag(oppositeTeamTag);
            float weaponRange = weaponInUse.weaponRange;
            Vector3 thisPosition = character.GetCellLocation().transform.position;

            targetsInRange.Clear();

            foreach (GameObject characterObj in characters) {
                Character foundCharacter = characterObj.GetComponent<Character>();
                if (!foundCharacter || !foundCharacter.GetCellLocation()) {
                    continue;
                }
                //Position is based on the cell the character's in
                Vector3 characterPosition = foundCharacter.GetCellLocation().transform.position;
                float distanceToCharacter = Vector3.Distance(characterPosition, thisPosition);
                if (distanceToCharacter <= weaponRange) {
                    if (!targetObstructed(foundCharacter)) {
                        targetsInRange.Add(foundCharacter);
                    }
                }
            }
        }

        private void overrideAnimationWithWeapon(Weapon weaponForAnimation) {
            if (weaponForAnimation == null)
                overrideAttackAnimation();
            else
                overrideAttackAnimation(weaponForAnimation.GetAnimationClip());
        }

        private IEnumerator delayedDamage(Character target, float delayTime) {
            Health targetHealth = target.GetComponent<Health>();
            if (!targetHealth) {
                throw new System.Exception("Target doesn't have a Health component");
            }
            else {
                yield return new WaitForSeconds(delayTime);
                targetHealth.TakeDamage(GetDamage);
            }
        }

    }

}