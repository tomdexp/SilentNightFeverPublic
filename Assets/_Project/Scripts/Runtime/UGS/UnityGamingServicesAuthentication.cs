using System;
using System.Threading.Tasks;
using _Project.Scripts.Runtime.Utils.Singletons;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace _Project.Scripts.Runtime.UGS
{
    public class UnityGamingServicesAuthentication : PersistentSingleton<UnityGamingServicesAuthentication>
    {
        public event Action OnAuthenticationSuccess; 
        public event Action<Exception> OnAuthenticationFailed;

        protected override void Awake()
        {
            base.Awake();
            var unityGamingServicesInitializer = UnityGamingServicesInitializer.TryGetInstance();
            if (unityGamingServicesInitializer == null)
            {
                Debug.LogError("UnityGamingServicesInitializer not found. Make sure it is present in the scene.");
                return;
            }
            unityGamingServicesInitializer.OnInitializationSuccess += OnInitializationSuccess;
        }

        private async void OnInitializationSuccess()
        {
            await AuthenticateAsync();
        }

        private async Task AuthenticateAsync()
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Sign in anonymously succeeded!");
        
                // Shows how to get the playerID
                Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
                OnAuthenticationSuccess?.Invoke();

            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
                OnAuthenticationFailed?.Invoke(ex);
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
                OnAuthenticationFailed?.Invoke(ex);
            }
            catch (Exception ex)
            {
                // Notify the player with the proper error message
                Debug.LogException(ex);
                OnAuthenticationFailed?.Invoke(ex);
            }
        }
    }
}