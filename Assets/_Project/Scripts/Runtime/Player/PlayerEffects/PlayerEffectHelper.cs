using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;


namespace _Project.Scripts.Runtime.Player.PlayerEffects
{
    public static class PlayerEffectHelper
    {
        private static Dictionary<Type, byte> typeToByteMap = new Dictionary<Type, byte>();
        private static Dictionary<byte, Type> byteToTypeMap = new Dictionary<byte, Type>();

        static PlayerEffectHelper()
        {
            InitializeTypeMappings();
        }

        private static void InitializeTypeMappings()
        {
            // Get all types that inherit from PlayerEffect
            var playerEffectTypes = Assembly.GetAssembly(typeof(PlayerEffect)).GetTypes()
                .Where(t => t.IsSubclassOf(typeof(PlayerEffect)) && !t.IsAbstract);

            byte index = 0;
            foreach (var type in playerEffectTypes)
            {
                if (typeToByteMap.TryAdd(type, index))
                {
                    byteToTypeMap[index] = type;
                    index++;
                }
            }
        }

        public static byte EffectToByte<T>() where T : PlayerEffect
        {
            return typeToByteMap[typeof(T)];
        }

        public static Type ByteToEffect(byte effectByte)
        {
            if (byteToTypeMap.TryGetValue(effectByte, out Type type))
            {
                return type;
            }

            Logger.LogError($"No PlayerEffect type found for byte {effectByte}.", Logger.LogType.Client);
            return null;
        }
        
        public static T LoadPlayerEffect<T>() where T : PlayerEffect
        {
            string path = "PlayerEffects/" + typeof(T).Name;
            T effect = Resources.Load<T>(path);
            if (!effect)
            {
                Logger.LogError("PlayerEffect of type " + typeof(T).Name + " not found at path: " + path, Logger.LogType.Client);
            }
            return effect;
        }
    }
}