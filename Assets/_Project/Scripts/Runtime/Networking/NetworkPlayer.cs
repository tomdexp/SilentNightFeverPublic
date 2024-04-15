﻿using System;
using System.Collections.Generic;
using _Project.Scripts.Runtime.Player;
using _Project.Scripts.Runtime.Player.PlayerEffects;
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

       public override void OnStartClient()
       { 
           base.OnStartClient(); 
           Logger.LogTrace("Player spawned on client", Logger.LogType.Client, this);
           _realPlayerInfo.OnChange += OnRealPlayerInfoChange;
       }

       public override void OnStopClient()
       {
           base.OnStopClient();
           _realPlayerInfo.OnChange -= OnRealPlayerInfoChange;
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
           Logger.LogTrace("NetworkPlayer : Giving effect " + typeof(T).Name, Logger.LogType.Client, this);
           var effect = ScriptableObject.CreateInstance<T>();
           _appliedPlayerEffects.Add(effect);
           effect.ApplyEffect(this);
       }
    }
}