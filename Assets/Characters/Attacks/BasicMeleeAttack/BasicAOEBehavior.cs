using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tactics.Characters {

    public class BasicAOEBehavior : BasicAttackBehavior {
        
        public override void Use(Vector3 targetPos, Weapon weaponForAnimation = null) {
            ResetTargetsInRange(targetPos);
            overrideAnimationWithWeapon(weaponForAnimation);
            lookAtTarget(targetPos);
            animator.SetTrigger(AttackTrigger);
            StartCoroutine(delayedAOEDamage(targetPos, .5f));
        }

        public override void ResetTargetsInRange(Vector3 originOfAttack) {
            string oppositeTeamTag = this.gameObject.CompareTag(ENEMY) ? PLAYER : ENEMY;
            GameObject[] characters = GameObject.FindGameObjectsWithTag(oppositeTeamTag);
            float weaponRange = GetRange();

            targetsInRange.Clear();

            print("Resetting targets in range with a new origin");

            foreach (GameObject characterObj in characters) {
                Character foundCharacter = characterObj.GetComponent<Character>();
                if (!foundCharacter || !foundCharacter.GetCellLocation()) {
                    continue;
                }
                //Position is based on the cell the character's in
                Vector3 characterPosition = foundCharacter.GetCellLocation().transform.position;
                float distanceToCharacter = Vector3.Distance(characterPosition, originOfAttack);
                if (distanceToCharacter <= weaponRange) {
                    targetsInRange.Add(foundCharacter);
                    print("Target aquired: " + foundCharacter.gameObject.name);
                    
                }
            }
        }

        private void overrideAnimationWithWeapon(Weapon weaponForAnimation) {
            if (weaponForAnimation == null)
                overrideAttackAnimation();
            else
                overrideAttackAnimation(weaponForAnimation.GetAnimationClip());
        }

        private IEnumerator delayedAOEDamage(Vector3 originOfAOE, float delayTime) {
            yield return new WaitForSeconds(delayTime);
            foreach (Character target in targetsInRange) {
                Health targetHealth = target.GetComponent<Health>();
                if (!targetHealth) {
                    throw new System.Exception("Target doesn't have a Health component");
                }
                else {
                    targetHealth.TakeDamage(GetDamage());
                }
            }
        }

    }

}