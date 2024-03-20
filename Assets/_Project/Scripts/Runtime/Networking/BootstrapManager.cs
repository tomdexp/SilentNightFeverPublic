using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using _Project.Scripts.Runtime.Utils.Singletons;
using FishNet;
using FishNet.Object;
using FishNet.Transporting.UTP;
using Sirenix.OdinInspector;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace _Project.Scripts.Runtime.Networking
{
    /// <summary>
    /// This class is responsible for linking the lobby scene and the game scene
    /// </summary>
    public class BootstrapManager : PersistentSingleton<BootstrapManager>
    {
        [field: SerializeField] public string CurrentJoinCode { get; private set; }
        
        [field: SerializeField] public int CurrentAllocationId { get; private set; }
        
        public bool HasJoinCode => !string.IsNullOrEmpty(CurrentJoinCode);
        
        private string _inputFieldJoinCode;
        
        /// <summary>
        /// Called when we switch from local server to relay connection
        /// </summary>
        public event Action OnServerMigrationStarted;
        
        /// <summary>
        /// Called when the server migration is finished, so the relay server is ready to accept connections
        /// or client has connected to the relay server
        /// </summary>
        public event Action OnServerMigrationFinished;
        
        public event Action<string> OnJoinCodeReceived;
        public event Action<string> OnInvalidJoinCodeInput;
        
        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(SetupAndStartLocalHost());
        }

        /// <summary>
        /// We assume everytime bootstrap manager awake is called we are on the start of the app
        /// So we call the start connection here to start a local server
        /// We have to take care of the port to avoid conflicts with other local servers
        /// Since we use a Relay, having a random port is not a problem
        /// </summary>
        private IEnumerator SetupAndStartLocalHost()
        {
            // Setup random ports for the server to avoid conflicts with other local servers (multiple instances of the game on the same computer)
            var udp = new UdpClient(0, AddressFamily.InterNetwork);
            int port = ((IPEndPoint)udp.Client.LocalEndPoint).Port;
            Debug.Log("Found free port for local server: " + port, this);
            var transport = InstanceFinder.TransportManager.GetTransport<FishyUnityTransport>();
            transport.ConnectionData.Port = (ushort)port;
            Debug.Log("Setting local server port to: " + transport.ConnectionData.Port, this);
            if (InstanceFinder.ServerManager.StartConnection())
            {
                yield return new WaitForSecondsRealtime(.1f);
                InstanceFinder.ClientManager.StartConnection(); 
            }
            else
            {
                Debug.LogError("Failed to start local server", this);
            }
        }

        [Button]
        public void StopAndRestart()
        {
            StartCoroutine(StopAndRestartCoroutine());
        }

        private IEnumerator StopAndRestartCoroutine()
        {
            InstanceFinder.ServerManager.StopConnection(false);
            yield return new WaitForSecondsRealtime(2f);
            var fishyUnityTransport = FindAnyObjectByType<FishyUnityTransport>();
            fishyUnityTransport.SetProtocol(FishyUnityTransport.ProtocolType.RelayUnityTransport);
            InstanceFinder.ServerManager.StartConnection();
        }

        /// <summary>
        /// Starts a game host with a relay allocation:  starts the host with a new relay allocation.
        /// </summary>
        /// <param name="maxConnections">Maximum number of connections to the created relay.</param>
        /// <returns>The join code that a client can use.</returns>
        /// <exception cref="ServicesInitializationException"> Exception when there's an error during services initialization </exception>
        /// <exception cref="UnityProjectNotLinkedException"> Exception when the project is not linked to a cloud project id </exception>
        /// <exception cref="CircularDependencyException"> Exception when two registered <see cref="IInitializablePackage"/> depend on the other </exception>
        /// <exception cref="AuthenticationException"> The task fails with the exception when the task cannot complete successfully due to Authentication specific errors. </exception>
        /// <exception cref="RequestFailedException"> See <see cref="IAuthenticationService.SignInAnonymouslyAsync"/></exception>
        /// <exception cref="ArgumentException">Thrown when the maxConnections argument fails validation in Relay Service SDK.</exception>
        /// <exception cref="RelayServiceException">Thrown when the request successfully reach the Relay Allocation service but results in an error.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the UnityTransport component cannot be found.</exception>
        public async Task StartHostWithRelay()
        {
            if (!EnsureUnityGamingServicesAreInitialized()) return;
            
            // Stop local server
            InstanceFinder.ServerManager.StopConnection(false);
            OnServerMigrationStarted?.Invoke();
            
            // Request allocation and join code
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(SilentNightFeverSettings.MAX_PLAYERS);
            CurrentJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            OnJoinCodeReceived?.Invoke(CurrentJoinCode);
            Debug.Log("Join code received : " + CurrentJoinCode);
            
            // Configure transport
            var fishyUnityTransport = InstanceFinder.TransportManager.GetTransport<FishyUnityTransport>();
            if (fishyUnityTransport == null)
            {
                Debug.LogError("FishyUnityTransport not found, cannot start a host with relay service.");
                return;
            }
            fishyUnityTransport.SetProtocol(FishyUnityTransport.ProtocolType.RelayUnityTransport);
            fishyUnityTransport.SetRelayServerData(new RelayServerData(allocation, SilentNightFeverSettings.RELAY_CONNECTION_TYPE));

            // Start host
            if (InstanceFinder.ServerManager.StartConnection()) // Server is successfully started.
            {
                InstanceFinder.ClientManager.StartConnection();
                OnServerMigrationFinished?.Invoke();
            }
            else
            {
                Debug.LogError("Failed to start host with relay service.");
            }
        }

        /// <summary>
        /// Joins a game with relay: it will initialize the Unity services, sign in anonymously, join the relay with the given join code and start the client.
        /// </summary>
        /// <param name="joinCode">The join code of the allocation</param>
        /// <returns>True if starting the client was successful</returns>
        /// <exception cref="ServicesInitializationException"> Exception when there's an error during services initialization </exception>
        /// <exception cref="UnityProjectNotLinkedException"> Exception when the project is not linked to a cloud project id </exception>
        /// <exception cref="CircularDependencyException"> Exception when two registered <see cref="IInitializablePackage"/> depend on the other </exception>
        /// <exception cref="AuthenticationException"> The task fails with the exception when the task cannot complete successfully due to Authentication specific errors. </exception>
        /// <exception cref="RequestFailedException">Thrown when the request does not reach the Relay Allocation service.</exception>
        /// <exception cref="ArgumentException">Thrown if the joinCode has the wrong format.</exception>
        /// <exception cref="RelayServiceException">Thrown when the request successfully reach the Relay Allocation service but results in an error.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the UnityTransport component cannot be found.</exception>
        public async Task<bool> StartClientWithRelay(string joinCode)
        {
            if (!EnsureUnityGamingServicesAreInitialized()) return false;

            // Stop local server
            InstanceFinder.ServerManager.StopConnection(false);
            OnServerMigrationStarted?.Invoke();
            
            // Join allocation
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
            CurrentJoinCode = joinCode;
            // Configure transport
            var fishyUnityTransport = InstanceFinder.TransportManager.GetTransport<FishyUnityTransport>();
            if (fishyUnityTransport == null)
            {
                Debug.LogError("FishyUnityTransport not found, cannot start a host with relay service.");
                return false;
            }
            fishyUnityTransport.SetProtocol(FishyUnityTransport.ProtocolType.RelayUnityTransport);
            fishyUnityTransport.SetRelayServerData(new RelayServerData(joinAllocation, SilentNightFeverSettings.RELAY_CONNECTION_TYPE));
            // Start client
            return !string.IsNullOrEmpty(joinCode) && InstanceFinder.NetworkManager.ClientManager.StartConnection();
        }
        
        private bool EnsureUnityGamingServicesAreInitialized()
        {
            // Make sure the user is signed in
            if (AuthenticationService.Instance.IsSignedIn) return true;
            Debug.LogError("User is not signed in, cannot continue.");
            return false;
        }
        
        // Add a button with OnGui to the upper right corner of the screen
        private void OnGUI()
        {
            if (GUI.Button(new Rect(Screen.width - 100, 0, 100, 50), "Host Relay"))
            {
                TryStartHostWithRelay();
            }
            // create input field for join code, if current join code is not empty, fill it with the current join code
            if (HasJoinCode)
            {
                // write a label with the current join code
                GUI.Label(new Rect(Screen.width - 200, 0, 100, 50), CurrentJoinCode);
            }
            else
            {
                _inputFieldJoinCode = GUI.TextField(new Rect(Screen.width - 200, 0, 100, 50), _inputFieldJoinCode);
            }
            if (GUI.Button(new Rect(Screen.width - 300, 0, 100, 50), "Join Relay"))
            {
                if (SanitizeJoinCode(ref _inputFieldJoinCode))
                {
                    Debug.Log("Join code sanitized: " + _inputFieldJoinCode);
                    TryJoinAsClientWithRelay(_inputFieldJoinCode);
                }
                else
                {
                    Debug.LogError("Invalid join code input.");
                }
            }
            if (GUI.Button(new Rect(Screen.width - 400, 0, 100, 50), "Spawn Player"))
            {
                PlayerManager.Instance.TrySpawnPlayer();
            }
        }
        
        public async void TryStartHostWithRelay()
        {
            Debug.Log("Trying to start host with relay...");
            await StartHostWithRelay();
        }
        
        public async void TryJoinAsClientWithRelay(string joinCode)
        {
            Debug.Log("Trying to join as client to a relay...");
            bool result = await StartClientWithRelay(joinCode);
            if (result)
            {
                OnServerMigrationFinished?.Invoke();
            }
        }
        
        public bool SanitizeJoinCode(ref string joinCode)
        {
            if (string.IsNullOrEmpty(joinCode))
            {
                // TODO : Localization, english hardcoded for now
                OnInvalidJoinCodeInput?.Invoke("JoinCode cannot be empty.");
                Debug.LogError("JoinCode cannot be empty.");
                return false;
            }
            Debug.Log("Join code before sanitize: " + joinCode);
            joinCode = joinCode.Trim().ToUpper();
            // Remove any non-alphanumeric characters
            joinCode = Regex.Replace(joinCode, "[^A-Z0-9]", "");
            Debug.Log("Join code after sanitize: " + joinCode);
            if (joinCode.Length != 6)
            {
                // TODO : Localization, english hardcoded for now
                string errorMessage = "JoinCode are expected to be 6 characters long, but the input was " + joinCode.Length + " characters long.";
                OnInvalidJoinCodeInput?.Invoke(errorMessage);
                Debug.LogError(errorMessage);
                return false;
            }
            // that doesn't mean the join code exists, but at least it's sanitized
            return true;
        }
    }
}