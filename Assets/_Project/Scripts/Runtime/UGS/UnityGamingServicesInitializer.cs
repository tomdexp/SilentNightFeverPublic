using System;
using System.Linq;
using _Project.Scripts.Runtime.Utils.Singletons;
using Unity.Multiplayer.Playmode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UGS
{
    [DefaultExecutionOrder(-100000)]
    public class UnityGamingServicesInitializer : PersistentSingleton<UnityGamingServicesInitializer>
    {
        public event Action OnInitializationSuccess;
        public event Action<Exception> OnInitializationFailed;
        
        private async void Start()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Logger.LogWarning("No internet connection detected. Unity Gaming Services will not be initialized.");
                OnInitializationFailed?.Invoke(new Exception("No internet connection detected."));
                return;
            }
            
            try
            {
                var options = new InitializationOptions();
#if UNITY_EDITOR
                var mppmTag = CurrentPlayer.ReadOnlyTags();
                string playerNumber = "Player1";
                if (mppmTag.Length == 0)
                {
                    Logger.LogWarning("No Multiplayer Playmode tag detected. We consider this instance is Player1, but no other instances will be able to join. Please add a tag to each in the Multiplayer Playmode window.");
                }
                else
                {
                    playerNumber = mppmTag.ToList().Find(number => number.StartsWith("Player"));
                }
                options.SetProfile(playerNumber);
                Logger.LogDebug($"Editor detected. Setting profile to {playerNumber}");
#endif
#if DEVELOPMENT_BUILD
                // set a random alphanumeric profile for development builds
                var randomString = Guid.NewGuid().ToString("N").Substring(0, 8);
                options.SetProfile(randomString);
                Logger.LogDebug($"Development build detected. Setting profile to {randomString}");
#endif
                await UnityServices.InitializeAsync(options);
                Logger.LogDebug("Unity Gaming Services initialized successfully.");
                OnInitializationSuccess?.Invoke();
            }
            catch (Exception e)
            {
                Logger.LogError($"Unity Gaming Services initialization failed : {e}");
                OnInitializationFailed?.Invoke(e);
            }
        }
    }
}