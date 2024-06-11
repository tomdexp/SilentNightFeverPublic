using System;
using System.Collections;
using _Project.Scripts.Runtime.Audio;
using _Project.Scripts.Runtime.Networking.Rounds;
using _Project.Scripts.Runtime.Player;
using _Project.Scripts.Runtime.Player.PlayerTongue;
using _Project.Scripts.Runtime.UI;
using _Project.Scripts.Runtime.Utils;
using _Project.Scripts.Runtime.Utils.Singletons;
using DG.Tweening;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Networking
{
    public class GameManager : NetworkPersistentSingleton<GameManager>
    {
        [Title("References")]
        [Required] public GameManagerData GameManagerData;
        public readonly SyncVar<bool> IsGameStarted = new SyncVar<bool>();
        public readonly SyncVar<bool> IsOnBoardingStarted = new SyncVar<bool>();
        public readonly SyncList<Round> Rounds = new SyncList<Round>();
        public readonly SyncList<RoundResult> RoundsResults = new SyncList<RoundResult>();
        public readonly SyncVar<byte> CurrentRoundNumber = new SyncVar<byte>(); // Starts at 1
        public readonly SyncVar<uint> CurrentRoundTimer = new SyncVar<uint>(new SyncTypeSettings(.5f));
        public readonly SyncVar<byte> RequiredRoundsToWin = new SyncVar<byte>();
        public event Action OnGameStarted;
        public event Action<PlayerTeamType> OnGameEnded; // arg = winning team
        public event Action<byte> OnAnyRoundStarted;
        public event Action<byte> OnAnyRoundEnded;
        public event Action OnFirstRoundStarted;
        public event Action OnFirstRoundEnded; // TODO : Implement
        public event Action OnFinalRoundStarted; // TODO : Implement
        public event Action OnFinalRoundEnded; // TODO : Implement
        public event Action OnBeforeSceneChange;
        public event Action OnAfterSceneChange;
        public RoundsConfig RoundsConfig => GameManagerData.RoundsConfig;
        
        private bool _isSubscribedToTongueChangeEvents;
        private float _deltaTimeCounter;
        private byte _teamATongueBindCount; // Count of players from team A that have their tongue binded to another player's anchor of the same team
        private byte _teamBTongueBindCount;
        private TongueAnchor _playerACharacterTongueAnchor;
        private TongueAnchor _playerBCharacterTongueAnchor;
        private TongueAnchor _playerCCharacterTongueAnchor;
        private TongueAnchor _playerDCharacterTongueAnchor;
        private PlayerStickyTongue _playerAStickyTongue;
        private PlayerStickyTongue _playerBStickyTongue;
        private PlayerStickyTongue _playerCStickyTongue;
        private PlayerStickyTongue _playerDStickyTongue;
        
        protected override void Awake()
        {
            base.Awake();
            if (!GameManagerData)
            {
                Logger.LogError("No GameManagerData found on the GameManager, please set it in the GameManager prefab !", Logger.LogType.Local, this);
            }
            if (!RoundsConfig)
            {
                Logger.LogError("No RoundsConfig found on the GameManager, please set it in the GameManagerData !", Logger.LogType.Local, this);
            }
        }

        private void Start()
        {
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            // Special conditions when the Editor does not load the StartScene first but another scene
            if (Enum.TryParse(currentSceneName, out SceneType sceneType))
            {
                switch (sceneType)
                {
                    case SceneType.StartScene:
                        CameraManager.Instance.TryDisableSplitScreenCameras();
                        LoadIntroScene();
                        break;
                    case SceneType.IntroScene:
                        CameraManager.Instance.TryDisableSplitScreenCameras();
                        break;
                    case SceneType.MenuScene:
                        CameraManager.Instance.TryDisableSplitScreenCameras();
                        PlayerManager.Instance.SetPlayerJoiningEnabled(false);
                        break;
                    case SceneType.OnBoardingScene:
                        CameraManager.Instance.TryEnableSplitScreenCameras(); // Special condition when the Editor directly loads the GameScene
                        PlayerManager.Instance.SetPlayerJoiningEnabled(true);
                        break;
                    case SceneType.GameScene:
                        CameraManager.Instance.TryEnableSplitScreenCameras(); // Special condition when the Editor directly loads the GameScene
                        PlayerManager.Instance.SetPlayerJoiningEnabled(true);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else if (currentSceneName == "MenuV2Scene")
            {
                Logger.LogWarning("The current scene name " + currentSceneName + " is not a valid SceneType enum value ! Special case where we disable Split Screen Cameras and Player Joining", Logger.LogType.Local, this);
                CameraManager.Instance.TryDisableSplitScreenCameras();
                PlayerManager.Instance.SetPlayerJoiningEnabled(false);
            }
            else if (currentSceneName == "TristanScene")
            {
                Logger.LogWarning("The current scene name " + currentSceneName + " is not a valid SceneType enum value ! Special case where we disable Split Screen Cameras and Player Joining", Logger.LogType.Local, this);
                CameraManager.Instance.TryDisableSplitScreenCameras();
                PlayerManager.Instance.SetPlayerJoiningEnabled(false);
            }
            else
            {
                Logger.LogWarning("The current scene name " + currentSceneName + " is not a valid SceneType enum value ! Enabling Split Screen Cameras and Player Joining per default", Logger.LogType.Local, this);
                CameraManager.Instance.TryEnableSplitScreenCameras();
                PlayerManager.Instance.SetPlayerJoiningEnabled(true);
            }
        }

        private void LoadIntroScene()
        {
            LoadGlobalScene(SceneType.IntroScene);
        }

        public void LoadMenuScene()
        {
            LoadGlobalScene(SceneType.MenuScene);
        }
        
        public void LoadOnBoardingScene()
        {
            LoadGlobalScene(SceneType.OnBoardingScene);
        }
        
        public void LoadGameScene()
        {
            PlayerManager.Instance.ResetPlayerSpawnedLocally(); // because they are destroyed when changing scene (coming from OnBoarding)
            LoadGlobalScene(SceneType.GameScene);
        }

        private void LoadGlobalScene(SceneType sceneType)
        {
            StartCoroutine(LoadGlobalSceneCoroutine(sceneType));
        }

        private IEnumerator LoadGlobalSceneCoroutine(SceneType sceneType)
        {
            if (!IsServerStarted)
            {
                Logger.LogWarning("Only the server can change scenes !", Logger.LogType.Local, this);
                yield break;
            }
            bool hasFinishedLoading = false;
            bool hasAllClientsLoaded = false;
            int totalNumberOfClients = NetworkManager.ClientManager.Clients.Count;
            int numberOfClientsLoaded = 0;
            Logger.LogInfo("Loading Scene : " + sceneType + "...", Logger.LogType.Server, this);
            OnBeforeSceneChange?.Invoke();
            yield return TransitionManager.Instance.BeginSceneChangeTransition();
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            SceneLoadData sld = new SceneLoadData(sceneType.ToString())
            {
                ReplaceScenes = ReplaceOption.All // That tells the server to replace all scenes with the new one
            };
            SceneManager.OnLoadEnd += (_) => hasFinishedLoading = true;
            SceneManager.OnClientPresenceChangeEnd += (args) =>
            {
                Logger.LogInfo($"Client ID:{args.Connection.ClientId}, has finished loading the new scene {sceneType.ToString()}", Logger.LogType.Server, this);
                numberOfClientsLoaded++;
                if (numberOfClientsLoaded == totalNumberOfClients)
                {
                    Logger.LogInfo("All clients have finished loading the new scene !", Logger.LogType.Server, this);
                    hasAllClientsLoaded = true;
                }
            };
            SceneManager.LoadGlobalScenes(sld);
            while (!hasFinishedLoading || !hasAllClientsLoaded)
            {
                //Logger.LogTrace("Waiting for scene to load...", Logger.LogType.Server, this);
                yield return null;
            }
            stopwatch.Stop();
            Logger.LogInfo("Scene loaded in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Server, this);
            OnAfterSceneChange?.Invoke();
            switch (sceneType)
            {
                case SceneType.StartScene:
                    break;
                case SceneType.IntroScene:
                    break;
                case SceneType.MenuScene:
                    PlayerManager.Instance.SetPlayerJoiningEnabled(false);
                    break;
                case SceneType.OnBoardingScene:
                    CameraManager.Instance.TryEnableSplitScreenCameras();
                    yield return new WaitForSeconds(1f);
                    TryStartOnBoarding();
                    break;
                case SceneType.GameScene:
                    CameraManager.Instance.TryEnableSplitScreenCameras();
                    yield return new WaitForSeconds(2f);
                    TryStartGame();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sceneType), sceneType, null);
            }
            yield return TransitionManager.Instance.EndSceneChangeTransition();
        }

        private IEnumerator UnLoadCurrentScene()
        {
            bool hasFinishedUnloading = false;
            Logger.LogInfo("Unloading current scene...", Logger.LogType.Local, this);
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            SceneUnloadData sld = new SceneUnloadData(currentSceneName);
            SceneManager.OnUnloadEnd += (_) => hasFinishedUnloading = true;
            SceneManager.UnloadGlobalScenes(sld);
            while (!hasFinishedUnloading)
            {
                yield return null;
            }
            stopwatch.Stop();
            Logger.LogInfo("Scene unloaded in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Local, this);
        }

        private void Update()
        {
            if (!IsServerStarted) return;
            if (IsGameStarted.Value)
            {
                if (CurrentRoundNumber.Value == 0) return;
                if (GetCurrentRound().IsRoundActive)
                {
                    _deltaTimeCounter += Time.deltaTime;
                    if (_deltaTimeCounter >= 1f)
                    {
                        _deltaTimeCounter = 0;
                        CurrentRoundTimer.Value++;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            UnsubscribeToTongueChangeEvents();
        }

        // Entry point
        [Button(ButtonSizes.Large)]
        public void TryStartGame()
        {
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentSceneName == SceneType.MenuScene.ToString() || currentSceneName == "MenuSceneV2")
            {
                Logger.LogWarning("The game cannot be started from the Menu Scene !", Logger.LogType.Server, this);
                return;
            }
            if (!IsServerStarted)
            {
                StartGameServerRpc();
            }
            else
            {
                StartCoroutine(StartGame());
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void StartGameServerRpc()
        {
            StartCoroutine(StartGame());
        }
        
        private IEnumerator StartGame()
        {
            if (!IsServerStarted) yield break;
            
            Logger.LogTrace("Attempting to start game...", Logger.LogType.Server, this);
            if (IsGameStarted.Value)
            {
                Logger.LogDebug("Game already started !", Logger.LogType.Server, this);
                yield break;
            }
            if (!PlayerManager.HasInstance)
            {
                Logger.LogError("No player manager instance found ! It should be spawned by the Default Spawn Objects script", Logger.LogType.Server, this);
                yield break;
            }
            if (PlayerManager.Instance.NumberOfPlayers != 4)
            {
                Logger.LogWarning("Not enough players to start the game ! (current : " + PlayerManager.Instance.NumberOfPlayers +"/4)", Logger.LogType.Server, this);
                yield break;
            }
            IsGameStarted.Value = true;
            yield return TransitionManager.Instance.BeginLoadingGameTransition();
            yield return new WaitForSeconds(1f);
            var procGen = FindAnyObjectByType<ProcGenInstanciator>();
            if (procGen)
            {
                // we found a procGenInstanciator, so we generate the map before starting the game and spawning the players
                yield return procGen.GenerateMap();
                yield return procGen.SpawnAllPrefabsCoroutine();
            }
            else
            {
                Logger.LogWarning("No ProcGenInstanciator found, the map will not be generated, ignore this if this is intended", Logger.LogType.Server, this);
            }
            PlayerManager.Instance.SpawnAllPlayers();
            PlayerManager.Instance.TrySetPlayerChangingTeamEnabled(false);
            SubscribeToTongueChangeEvents();
            SetupRounds();

            yield return new WaitUntil(() => PlayerManager.Instance.AreAllPlayerSpawnedLocally);
            
            yield return StartRounds();
            OnGameStarted?.Invoke();
            yield return TransitionManager.Instance.EndLoadingGameTransition();
            Logger.LogInfo("Game started !", Logger.LogType.Server, this);
        }
        
        public void TryStartOnBoarding()
        {
            if (!IsServerStarted)
            {
                StartOnBoardingServerRpc();
            }
            else
            {
                StartCoroutine(StartOnBoarding());
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void StartOnBoardingServerRpc()
        {
            StartCoroutine(StartOnBoarding());
        }
        
        private IEnumerator StartOnBoarding()
        {
            if (!IsServerStarted) yield break;
            
            Logger.LogTrace("Attempting to start on boarding...", Logger.LogType.Server, this);
            if (IsGameStarted.Value)
            {
                Logger.LogDebug("Game already started ! Can't start the OnBoarding", Logger.LogType.Server, this);
                yield break;
            }
            if (IsOnBoardingStarted.Value)
            {
                Logger.LogDebug("OnBoarding already started !", Logger.LogType.Server, this);
                yield break;
            }
            
            if (!PlayerManager.HasInstance)
            {
                Logger.LogError("No player manager instance found ! It should be spawned by the Default Spawn Objects script", Logger.LogType.Server, this);
                yield break;
            }
            if (PlayerManager.Instance.NumberOfPlayers != 4)
            {
                Logger.LogWarning("Not enough players to start the game ! (current : " + PlayerManager.Instance.NumberOfPlayers +"/4)", Logger.LogType.Server, this);
                yield break;
            }
            IsOnBoardingStarted.Value = true;
            
            yield return TransitionManager.Instance.BeginLoadingGameTransition();
            yield return new WaitForSeconds(1f);
            
            PlayerManager.Instance.SpawnAllPlayers();
            PlayerManager.Instance.TrySetPlayerChangingTeamEnabled(false);

            yield return new WaitUntil(() => PlayerManager.Instance.AreAllPlayerSpawnedLocally);
            
            PlayerManager.Instance.TeleportAllPlayerToOnBoardingSpawnPoints();
            PlayerManager.Instance.ForceAllPlayersCameraAngle(1.5f);
            
            yield return new WaitForSeconds(1f);
            yield return TransitionManager.Instance.EndLoadingGameTransition();
            
            PlayerManager.Instance.CanPlayerUseTongue.Value = true;
            Logger.LogInfo("OnBoarding started !", Logger.LogType.Server, this);
        }

        private void SubscribeToTongueChangeEvents()
        {
            if (_isSubscribedToTongueChangeEvents)
            {
                Logger.LogDebug("Already subscribed to tongue change events ! Which means the game is probably resetting", Logger.LogType.Server, this);
                return;
            }
            
            _playerAStickyTongue = PlayerManager.Instance.GetNetworkPlayer(PlayerIndexType.A).GetPlayerController().GetTongue();
            _playerBStickyTongue = PlayerManager.Instance.GetNetworkPlayer(PlayerIndexType.B).GetPlayerController().GetTongue();
            _playerCStickyTongue = PlayerManager.Instance.GetNetworkPlayer(PlayerIndexType.C).GetPlayerController().GetTongue();
            _playerDStickyTongue = PlayerManager.Instance.GetNetworkPlayer(PlayerIndexType.D).GetPlayerController().GetTongue();

            _playerACharacterTongueAnchor = PlayerManager.Instance.GetNetworkPlayer(PlayerIndexType.A).GetPlayerController().GetCharacterTongueAnchor();
            _playerBCharacterTongueAnchor = PlayerManager.Instance.GetNetworkPlayer(PlayerIndexType.B).GetPlayerController().GetCharacterTongueAnchor();
            _playerCCharacterTongueAnchor = PlayerManager.Instance.GetNetworkPlayer(PlayerIndexType.C).GetPlayerController().GetCharacterTongueAnchor();
            _playerDCharacterTongueAnchor = PlayerManager.Instance.GetNetworkPlayer(PlayerIndexType.D).GetPlayerController().GetCharacterTongueAnchor();
            
            _playerACharacterTongueAnchor.OnTongueBindChange += OnAnyPlayerTongueBindChange;
            _playerBCharacterTongueAnchor.OnTongueBindChange += OnAnyPlayerTongueBindChange;
            _playerCCharacterTongueAnchor.OnTongueBindChange += OnAnyPlayerTongueBindChange;
            _playerDCharacterTongueAnchor.OnTongueBindChange += OnAnyPlayerTongueBindChange;
            
            _isSubscribedToTongueChangeEvents = true;
        }

        private void OnAnyPlayerTongueBindChange(PlayerStickyTongue tongue)
        {
            Logger.LogTrace("OnAnyPlayerTongueBindChange called, checking win conditions...", Logger.LogType.Server, this);
            
            // check how many players have their tongue bind to the other player's anchor of the same team
            // we can get the current bind anchor by a tongue with tongue.GetCurrentBindTongueAnchor()
            _teamATongueBindCount = 0;
            _teamBTongueBindCount = 0;
            
            Logger.LogTrace("Player A Anchor is bind to : " + _playerACharacterTongueAnchor.GetCurrentStickTongue(), Logger.LogType.Server, _playerACharacterTongueAnchor.GetCurrentStickTongue());
            Logger.LogTrace("Player B Anchor is bind to : " + _playerBCharacterTongueAnchor.GetCurrentStickTongue(), Logger.LogType.Server, _playerBCharacterTongueAnchor.GetCurrentStickTongue());
            Logger.LogTrace("Player C Anchor is bind to : " + _playerCCharacterTongueAnchor.GetCurrentStickTongue(), Logger.LogType.Server, _playerCCharacterTongueAnchor.GetCurrentStickTongue());
            Logger.LogTrace("Player D Anchor is bind to : " + _playerDCharacterTongueAnchor.GetCurrentStickTongue(), Logger.LogType.Server, _playerDCharacterTongueAnchor.GetCurrentStickTongue());
            
            // Team composition : team A (player A, player C) vs team B (player b, player D)
            if(_playerAStickyTongue == _playerCCharacterTongueAnchor.GetCurrentStickTongue())
            {
                _teamATongueBindCount++;
                Logger.LogTrace("Player A tongue is bind to player C", Logger.LogType.Server, this);
            }
            if(_playerCStickyTongue == _playerACharacterTongueAnchor.GetCurrentStickTongue())
            {
                _teamATongueBindCount++;
                Logger.LogTrace("Player C tongue is bind to player A", Logger.LogType.Server, this);
            }
            if(_playerBStickyTongue == _playerDCharacterTongueAnchor.GetCurrentStickTongue())
            {
                _teamBTongueBindCount++;
                Logger.LogTrace("Player B tongue is bind to player D", Logger.LogType.Server, this);
            }
            if(_playerDStickyTongue == _playerBCharacterTongueAnchor.GetCurrentStickTongue())
            {
                _teamBTongueBindCount++;
                Logger.LogTrace("Player D tongue is bind to player B", Logger.LogType.Server, this);
            }

            if (_teamATongueBindCount == 2)
            {
                Logger.LogInfo("Team A won the round !", Logger.LogType.Server, this);
                EndCurrentRound(PlayerTeamType.A);
            }
            else if (_teamBTongueBindCount == 2)
            {
                Logger.LogInfo("Team B won the round !", Logger.LogType.Server, this);
                EndCurrentRound(PlayerTeamType.B);
            }
            
            Logger.LogTrace("Team A tongue bind count : " + _teamATongueBindCount, Logger.LogType.Server, this);
            Logger.LogTrace("Team B tongue bind count : " + _teamBTongueBindCount, Logger.LogType.Server, this);
        }

        private void UnsubscribeToTongueChangeEvents()
        {
            if(_playerACharacterTongueAnchor) _playerACharacterTongueAnchor.OnTongueBindChange -= OnAnyPlayerTongueBindChange;
            if(_playerBCharacterTongueAnchor) _playerBCharacterTongueAnchor.OnTongueBindChange -= OnAnyPlayerTongueBindChange;
            if(_playerCCharacterTongueAnchor) _playerCCharacterTongueAnchor.OnTongueBindChange -= OnAnyPlayerTongueBindChange;
            if(_playerDCharacterTongueAnchor) _playerDCharacterTongueAnchor.OnTongueBindChange -= OnAnyPlayerTongueBindChange;
            _isSubscribedToTongueChangeEvents = false;
        }

        private void SetupRounds()
        {
            Logger.LogDebug("Setting up rounds...", Logger.LogType.Server, this);
            Rounds.Clear();
            switch (RoundsConfig.RoundsWinType)
            {
                case RoundsWinType.BestOfX:
                    for (byte i = 1; i <= RoundsConfig.RoundsCount; i++)
                    {
                        var round = new Round();
                        round.SetRoundNumber(i);
                        Rounds.Add(round);
                        Logger.LogDebug("Round " + i + " added", Logger.LogType.Server, this);
                    }
                    break;
                case RoundsWinType.FirstToX:
                    for (byte i = 1; i <= RoundsConfig.RoundsCount*2-1; i++)
                    {
                        var round = new Round();
                        round.SetRoundNumber(i);
                        Rounds.Add(round);
                        Logger.LogDebug("Round " + i + " added", Logger.LogType.Server, this);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            RequiredRoundsToWin.Value = RoundsConfig.RoundsCount;
        }
        
        private IEnumerator StartRounds()
        {
            Logger.LogDebug($"Starting first round in {GameManagerData.SecondsBetweenStartOfTheGameAndFirstRound} seconds...", Logger.LogType.Server, this);
            yield return new WaitForSeconds(GameManagerData.SecondsBetweenStartOfTheGameAndFirstRound);
            StartCoroutine(StartNextRound());
        }

        public IEnumerator StartNextRound()
        {
            if (CurrentRoundNumber.Value >= RoundsConfig.GetMaxRounds())
            {
                Logger.LogDebug("No more rounds to play, the current round number is superior or equal to the number of round in the RoundsConfig!", Logger.LogType.Server, this);
                yield return null;
            }
            CurrentRoundTimer.Value = 0;
            CurrentRoundNumber.Value++;
            if (CurrentRoundNumber.Value == 1)
            {
                OnFirstRoundStarted?.Invoke();
            }
            else
            {
                yield return TransitionManager.Instance.BeginLoadingRoundTransition();
                yield return new WaitForSeconds(GameManagerData.SecondsBetweenRounds);
            }
            OnAnyRoundStarted?.Invoke(CurrentRoundNumber.Value);
            Logger.LogInfo("Starting round " + CurrentRoundNumber.Value, Logger.LogType.Server, this);
            GetCurrentRound().StartRound();
            AudioManager.Instance.PlayAudioNetworked(AudioManager.Instance.AudioManagerData.EventRoundHideScoreFade, AudioManager.Instance.gameObject);
            yield return TransitionManager.Instance.EndLoadingRoundTransition();
        }

        private void EndCurrentRound(PlayerTeamType teamType)
        {
            if (teamType == PlayerTeamType.Z)
            {
                Logger.LogError("The current round was ended by the Z team, this should not happen", Logger.LogType.Server, this);
                return;
            }
            if (CurrentRoundNumber.Value == 0)
            {
                Logger.LogError("No round is currently active !", Logger.LogType.Server, this);
                return;
            }
            if (GetCurrentRound().IsRoundActive)
            {
                RoundResult roundResult = new RoundResult
                {
                    SecondsElapsed = CurrentRoundTimer.Value,
                    RoundNumber = CurrentRoundNumber.Value,
                    WinningTeam = teamType
                };
                OnAnyRoundEnded?.Invoke(CurrentRoundNumber.Value);
                RoundsResults.Add(roundResult);
                GetCurrentRound().EndRound();
                Logger.LogInfo("Round " + CurrentRoundNumber.Value + $" ended ! {roundResult}", Logger.LogType.Server, this);
                bool isGameFinished = CheckIfGameIsFinished();
                if(!isGameFinished) StartCoroutine(StartNextRound());
            }
            else
            {
                Logger.LogError("There is no active round to win !", Logger.LogType.Server, this);
            }
        }

        private bool CheckIfGameIsFinished()
        {
            int teamWinsA = RoundsResults.Collection.FindAll(result => result.WinningTeam == PlayerTeamType.A).Count;
            int teamWinsB = RoundsResults.Collection.FindAll(result => result.WinningTeam == PlayerTeamType.B).Count;
            switch (RoundsConfig.RoundsWinType)
            {
                case RoundsWinType.BestOfX:
                    if (teamWinsA > RoundsConfig.RoundsCount / 2)
                    {
                        StartCoroutine(EndGame(PlayerTeamType.A));
                        return true;
                    }
                    if (teamWinsB > RoundsConfig.RoundsCount / 2)
                    {
                        StartCoroutine(EndGame(PlayerTeamType.B));
                        return true;
                    }
                    return false;
                case RoundsWinType.FirstToX:
                    if (teamWinsA >= RoundsConfig.RoundsCount)
                    {
                        StartCoroutine(EndGame(PlayerTeamType.A));
                        return true;
                    }
                    if (teamWinsB >= RoundsConfig.RoundsCount)
                    {
                        StartCoroutine(EndGame(PlayerTeamType.B));
                        return true;
                    }
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IEnumerator EndGame(PlayerTeamType winningTeam)
        {
            Logger.LogInfo("Game finished ! The winning team is Team " + winningTeam, Logger.LogType.Server, this);
            yield return new WaitForSeconds(GameManagerData.SecondsBetweenLastRoundCompletionAndEndOfTheGame);
            OnGameEnded?.Invoke(winningTeam);
            Logger.LogTrace("OnGameEnded event invoked", Logger.LogType.Server, this);
        }

        public Round GetRound(byte roundNumber)
        {
            if (roundNumber < 1 || roundNumber > RoundsConfig.GetMaxRounds())
            {
                Logger.LogError("Round number " + roundNumber + " is out of bounds !", Logger.LogType.Server, this);
                return null;
            }
            return Rounds[roundNumber - 1];
        }
        
        public Round GetCurrentRound()
        {
            return GetRound(CurrentRoundNumber.Value);
        }
        
        public void TryForceRoundWinner(PlayerTeamType teamType)
        {
            if (!IsServerStarted)
            {
                ForceRoundWinnerServerRpc(teamType);
            }
            else
            {
                ForceRoundWinner(teamType);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ForceRoundWinnerServerRpc(PlayerTeamType teamType)
        {
            ForceRoundWinner(teamType);
        }

        private void ForceRoundWinner(PlayerTeamType teamType)
        {
            if (teamType == PlayerTeamType.Z)
            {
                Logger.LogError("You can't force the Z team to win the round !", Logger.LogType.Server, this);
                return;
            }
            if (!IsGameStarted.Value)
            {
                Logger.LogError("The game is not started ! The round cannot be forced to win", Logger.LogType.Server, this);
                return;
            }
            if (CurrentRoundNumber.Value == 0)
            {
                Logger.LogError("No round is currently active !", Logger.LogType.Server, this);
                return;
            }
            if (GetCurrentRound().IsRoundActive)
            {
                EndCurrentRound(teamType);
            }
            else
            {
                Logger.LogError("The current round is already ended !", Logger.LogType.Server, this);
            }
        }

        public void ResetGame()
        {
            if (!IsServerStarted) return;
            
            IsGameStarted.Value = false;
            Rounds.Clear();
            RoundsResults.Clear();
            CurrentRoundNumber.Value = 0;
            CurrentRoundTimer.Value = 0;
            _deltaTimeCounter = 0;
            _teamATongueBindCount = 0;
            _teamBTongueBindCount = 0;
            StartCoroutine(StartGame());
        }
        
        public int GetWinCount(PlayerTeamType teamType)
        {
            return RoundsResults.Collection.FindAll(result => result.WinningTeam == teamType).Count;
        }
    }
}