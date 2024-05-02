﻿using System;
using _Project.Scripts.Runtime.Networking;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Player.PlayerEffects
{
    [CreateAssetMenu(fileName = nameof(PE_KitchenFood), menuName = "Scriptable Objects/" + nameof(PE_KitchenFood))]
    public class PE_KitchenFood : PlayerEffect
    {
        [Title("PE_KitchenFood Settings")]
        [SerializeField] private EffectStep[] _effectSteps;

        [Serializable]
        public class EffectStep
        {
            public float SpeedMultiplier;
            public float SizeMultiplier;
        }
        public override void ApplyEffect(NetworkPlayer player)
        {
            throw new System.NotImplementedException();
        }
    }
}