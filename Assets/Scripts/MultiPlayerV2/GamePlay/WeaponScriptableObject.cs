using System;
using UnityEngine;

namespace E2MultiPlayer
{
    public class WeaponScriptableObject : ScriptableObject
    {
        [Serializable]
        public struct OffsetData
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
        }

        [Serializable]
        public struct WeaponData
        {
            public GameObject weaponObject;
            public OffsetData handOffset;
            public OffsetData holsterOffset;
            public bool leftHand;
            public int holsterIndex;
            public AudioClip[] Sounds;
        }

        public WeaponData[] weapons;

    }
}