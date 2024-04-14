﻿using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using _Project.Scripts.Runtime.Utils.Singletons;
using FishNet;
using FishNet.Transporting.UTP;
using Sirenix.OdinInspector;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEditor.PackageManager;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

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
        /// Called when we failed to change server
        /// </summary>
        public event Action OnServerMigrationFailed;

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
            Logger.LogDebug("Found free port for local server: " + port, Logger.LogType.Server, this);
            var transport = InstanceFinder.TransportManager.GetTransport<FishyUnityTransport>();
            transport.ConnectionData.Port = (ushort)port;
            Logger.LogDebug("Setting local server port to: " + transport.ConnectionData.Port, Logger.LogType.Server, this);
            if (InstanceFinder.ServerManager.StartConnection())
            {
                yield return new WaitForSecondsRealtime(.1f);
                InstanceFinder.ClientManager.StartConnection();
            }
            else
            {
                Logger.LogError("Failed to start local server", Logger.LogType.Server, context:this);
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
            try
            {
                if (!EnsureUnityGamingServicesAreInitialized()) return;
                
                // Stop local server
                InstanceFinder.ServerManager.StopConnection(false);
                OnServerMigrationStarted?.Invoke();
                
                // Request allocation and join code
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(SilentNightFeverSettings.MAX_PLAYERS);
                CurrentJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                OnJoinCodeReceived?.Invoke(CurrentJoinCode);
                Logger.LogDebug("Join code received : " + CurrentJoinCode, Logger.LogType.Server, this);
                
                // Configure transport
                var fishyUnityTransport = InstanceFinder.TransportManager.GetTransport<FishyUnityTransport>();
                if (fishyUnityTransport == null)
                {
                    Logger.LogError("FishyUnityTransport not found, cannot start a host with relay service.", Logger.LogType.Server, this);
                    throw new Exception("FishyUnityTransport not found, cannot start a host with relay service.");
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
                    Logger.LogError("Failed to start host with relay service.", Logger.LogType.Server, this);
                    throw new Exception("Failed to start host with relay service.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to start host with relay service.", Logger.LogType.Server, context:this);
                OnServerMigrationFailed?.Invoke();
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
            try
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
                    throw new Exception("FishyUnityTransport not found, cannot start a host with relay service.");
                }
                fishyUnityTransport.SetProtocol(FishyUnityTransport.ProtocolType.RelayUnityTransport);
                fishyUnityTransport.SetRelayServerData(new RelayServerData(joinAllocation, SilentNightFeverSettings.RELAY_CONNECTION_TYPE));
                // Start client
                bool result = !string.IsNullOrEmpty(joinCode) && InstanceFinder.NetworkManager.ClientManager.StartConnection();
                if (!result) throw new Exception("Couldn't start connection.");
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError("FishyUnityTransport not found, cannot start a host with relay service.", Logger.LogType.Client, context:this);
                OnServerMigrationFailed?.Invoke();

                return false;
            }

        }

        private bool EnsureUnityGamingServicesAreInitialized()
        {
            // Make sure the user is signed in
            if (AuthenticationService.Instance.IsSignedIn) return true;
            Logger.LogError("User is not signed in, cannot continue.", Logger.LogType.Local, context:this);
            return false;
        }

        public async void TryStartHostWithRelay()
        {
            Logger.LogTrace("Trying to start host with relay...", Logger.LogType.Server, context:this);
            if (GameManager.Instance.IsGameStarted.Value)
            {
                Logger.LogWarning("Game already started, cannot start a new host.", Logger.LogType.Server, context:this);
                return;
            }
            await StartHostWithRelay();
        }

        public async void TryJoinAsClientWithRelay(string joinCode)
        {
            Logger.LogTrace("Trying to join as client to a relay...", Logger.LogType.Client, context:this);
            if (GameManager.Instance.IsGameStarted.Value)
            {
                Logger.LogWarning("Game already started, cannot join as client.", Logger.LogType.Client, context:this);
                return;
            }
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
                Logger.LogError("JoinCode cannot be empty.", context:this);
                return false;
            }
            Logger.LogTrace("Join code before sanitize: " + joinCode, context:this);
            joinCode = joinCode.Trim().ToUpper();
            // Remove any non-alphanumeric characters
            joinCode = Regex.Replace(joinCode, "[^A-Z0-9]", "");
            Logger.LogTrace("Join code after sanitize: " + joinCode, context:this);
            if (joinCode.Length != 6)
            {
                // TODO : Localization, english hardcoded for now
                string errorMessage = "JoinCode are expected to be 6 characters long, but the input was " + joinCode.Length + " characters long.";
                OnInvalidJoinCodeInput?.Invoke(errorMessage);
                Logger.LogError(errorMessage, context:this);
                return false;
            }
            // that doesn't mean the join code exists, but at least it's sanitized
            return true;
        }
    }
}