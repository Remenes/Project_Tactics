using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tactics.Grid;

namespace Tactics.Characters {

    public class BasicAOEBehavior : BasicAttackBehavior {
        
        private float AOERange() {
            return abilityConfig.AOERange;
        }

        public override void Use(Cell targetPos, Weapon weaponForAnimation = null) {
            ResetTargetsInRange(targetPos);
            overrideAnimationWithWeapon(weaponForAnimation);
            lookAtTarget(targetPos.transform.position);
            animator.SetTrigger(AttackTrigger);
            StartCoroutine(delayedAOEDamage(targetPos.transform.position, .5f));
        }

        public override void Use(Character target, Weapon weaponForAnimation = null) {
            Use(target.GetCellLocation(), weaponForAnimation);
        }

        public override void ResetTargetsInRange() {
            ResetTargetsInRange(character.GetCellLocation());
        }

        public override void ResetTargetsInRange(Cell cellOriginOfAttack) {
            string oppositeTeamTag = this.gameObject.CompareTag(ENEMY) ? PLAYER : ENEMY;
            GameObject[] characters = GameObject.FindGameObjectsWithTag(oppositeTeamTag);
            float weaponRange = AOERange();
            Vector3 originOfAttack = cellOriginOfAttack.transform.position;

            targetsInRange.Clear();

            Character characterOnCell = cellOriginOfAttack.GetCharacterOnCell();
            if (config.RequiresTarget && config.UseMouseLocation && !(characterOnCell && characterOnCell.CompareTag(oppositeTeamTag)))
                return;

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