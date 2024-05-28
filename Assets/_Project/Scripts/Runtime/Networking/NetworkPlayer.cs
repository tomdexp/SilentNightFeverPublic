using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Runtime.Audio;
using _Project.Scripts.Runtime.Player;
using _Project.Scripts.Runtime.Player.PlayerEffects;
using DG.Tweening;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Networking
{
    [RequireComponent(typeof(PlayerController))]
    public class NetworkPlayer : NetworkBehaviour
    {
       [field: SerializeField] public PlayerData PlayerData { get; private set; }
       public bool IsOnline => !Owner.IsHost;
       private readonly SyncVar<RealPlayerInfo> _realPlayerInfo = new SyncVar<RealPlayerInfo>(new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));
       private PlayerController _playerController;
       private List<PlayerEffect> _appliedPlayerEffects = new List<PlayerEffect>();

       private void Awake()
       { 
           _playerController = GetComponent<PlayerController>();
           if (PlayerData == null)
           {
               Logger.LogError("PlayerData is null on NetworkPlayer. Set it on the prefab.", context: this);
           }
           var copy = Instantiate(PlayerData);
           PlayerData = copy;
       }

       public override void OnStartServer()
       {
           base.OnStartServer();
           Logger.LogTrace($"Player {GetPlayerIndexType()} spawned on server", Logger.LogType.Server, this);
           ResetPlayerClientRpc(0);
           StartCoroutine(TrySubscribingToRoundEndEvent());
       }

       public override void OnStartClient()
       { 
           base.OnStartClient(); 
           Logger.LogTrace($"Player {GetPlayerIndexType()} spawned on client", Logger.LogType.Client, this);
           _realPlayerInfo.OnChange += OnRealPlayerInfoChange;
       }

       public override void OnStopClient()
       {
           base.OnStopClient();
           _realPlayerInfo.OnChange -= OnRealPlayerInfoChange;
       }

       private IEnumerator TrySubscribingToRoundEndEvent()
       {
           while (!GameManager.HasInstance)
           {
               yield return null;
           }
           GameManager.Instance.OnAnyRoundEnded += ResetPlayerClientRpc;
       }
       
       private void OnDestroy()
       {
           if (GameManager.HasInstance) GameManager.Instance.OnAnyRoundEnded -= ResetPlayerClientRpc;
       }
       
       [ObserversRpc]
       private void ResetPlayerClientRpc(byte _)
       {
           if (!Owner.IsLocalClient) return;
           // Called when the player is reset, for example when a new round starts or at the beginning of the game
           _appliedPlayerEffects.Clear();
           TrySetSize(PlayerData.PlayerDefaultSize);
           Logger.LogDebug($"Player {GetPlayerIndexType()} reset", Logger.LogType.Client, this);
       }

       private void OnRealPlayerInfoChange(RealPlayerInfo prev, RealPlayerInfo next, bool asServer)
       { 
           Logger.LogTrace("RealPlayerInfo changed for player " + next.PlayerIndexType + " (" + next.ClientId + "|" + next.DevicePath + ")", Logger.LogType.Client, this);
           _playerController.SetRealPlayerInfo(next);
       }

       public void SetRealPlayerInfo(RealPlayerInfo realPlayerInfo)
       {
           _realPlayerInfo.Value = realPlayerInfo;
       }
       
       public PlayerIndexType GetPlayerIndexType()
       {
           return _realPlayerInfo.Value.PlayerIndexType;
       }
       
       public RealPlayerInfo GetRealPlayerInfo()
       {
           return _realPlayerInfo.Value;
       }

       public void GiveEffect<T>() where T : PlayerEffect
       {
           Logger.LogDebug("Giving effect " + typeof(T).Name, Logger.LogType.Client, this);
           T effect = PlayerEffectHelper.LoadPlayerEffect<T>();
           if (effect)
           {
               _appliedPlayerEffects.Add(effect);
               effect.ApplyEffect(this);
           }
           else
           {
               Logger.LogError("Failed to load PlayerEffect of type " + typeof(T).Name, Logger.LogType.Client, this);
           }
       } 
       
       public PlayerController GetPlayerController()
       {
           return _playerController;
       }

       public PlayerTeamType GetPlayerTeamType()
       {
           return _realPlayerInfo.Value.PlayerIndexType switch
           {
               PlayerIndexType.A => PlayerTeamType.A,
               PlayerIndexType.B => PlayerTeamType.B,
               PlayerIndexType.C => PlayerTeamType.A,
               PlayerIndexType.D => PlayerTeamType.B,
               PlayerIndexType.Z => PlayerTeamType.Z,
               _ => throw new ArgumentOutOfRangeException()
           };
       }
       
       public void TrySetSize(float newSize)
       {
           if (IsServerStarted)
           {
               SetSize(newSize);
           }
           SetSizeServerRpc(newSize);
       }

       [ServerRpc(RequireOwnership = false)]
       private void SetSizeServerRpc(float newSize)
       {
           SetSize(newSize);
           SetSizeClientRpc(newSize);
       }

       [ObserversRpc(ExcludeServer = true)]
       private void SetSizeClientRpc(float newSize)
       {
           SetSize(newSize);
       }
       
       private void SetSize(float newSize)
       {
           StartCoroutine(SetSizeCoroutine(newSize));
       }
       
       private IEnumerator SetSizeCoroutine(float newSize)
       {
           if (newSize > PlayerData.PlayerMaxSize || newSize < PlayerData.PlayerMinSize)
           {
               Logger.LogWarning("Tried to set size to " + newSize + " which is out of bounds", Logger.LogType.Client, this);
               yield break;
           }
           
           // check if we are scaling up or down compared to our current size, based on the scale.x
           var currentSize = transform.localScale.x;
           var scaleDirection = newSize > currentSize ? 1 : -1;
           // TODO : Fix ground alignment after scaling
           var newY = newSize/2;
           AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventPlayerSizeChange, gameObject);
           if (scaleDirection == 1) // scaling up
           {
               // use dotween to scale up
               transform.DOScale(Vector3.one * newSize, PlayerData.PlayerSizeUpChangeDuration).SetEase(PlayerData.PlayerSizeUpChangeEase);
               transform.DOMoveY(newY, PlayerData.PlayerSizeUpChangeDuration/2);
               yield return new WaitForSeconds(PlayerData.PlayerSizeUpChangeDuration);
           }
           else
           {
                // use dotween to scale down
                transform.DOScale(Vector3.one * newSize, PlayerData.PlayerSizeDownChangeDuration).SetEase(PlayerData.PlayerSizeDownChangeEase);
                transform.DOMoveY(newY, PlayerData.PlayerSizeDownChangeDuration/2);
                yield return new WaitForSeconds(PlayerData.PlayerSizeDownChangeDuration);
           }
           yield return null;
       }
       
       /// <summary>
       /// Wrapper of PlayerController.Teleport
       /// </summary>
       public void Teleport(Transform tr)
       {
           _playerController.Teleport(tr);
       }
        
       /// <summary>
       /// Wrapper of PlayerController.Teleport
       /// </summary>
       public void Teleport(Vector3 position)
       { 
           _playerController.Teleport(position);
       }
    }
}