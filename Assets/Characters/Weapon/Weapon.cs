﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tactics.Characters {

    [CreateAssetMenu(menuName = "Tactics/Weapon")]
    public class Weapon : ScriptableObject {

        [SerializeField] private int damage;
        [SerializeField] private float range;

        public int weaponDamage { get { return damage; } }
        public float weaponRange { get { return range; } }

    }

}