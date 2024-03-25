using System;
using _Project.Scripts.Runtime.Player;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace _Project.Scripts.Runtime.Networking
{
    [RequireComponent(typeof(PlayerController))]
    public class NetworkPlayer : NetworkBehaviour
    {
       [field: SerializeField] public PlayerData PlayerData { get; private set; }
       private readonly SyncVar<RealPlayerInfo> _realPlayerInfo = new SyncVar<RealPlayerInfo>(new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));
       private PlayerController _playerController;

       private void Awake()
       { 
           _playerController = GetComponent<PlayerController>();
           if (PlayerData == null)
           {
               Debug.LogError("PlayerData is null on NetworkPlayer. Set it on the prefab.", this);
           }
       }

       public override void OnStartClient()
       { 
           base.OnStartClient(); 
           Debug.Log("Player spawned on client");
           _realPlayerInfo.OnChange += OnRealPlayerInfoChange;
       }

       public override void OnStopClient()
       {
           base.OnStopClient();
           _realPlayerInfo.OnChange -= OnRealPlayerInfoChange;
       }

       private void OnRealPlayerInfoChange(RealPlayerInfo prev, RealPlayerInfo next, bool asServer)
       { 
           Debug.Log("RealPlayerInfo changed for player " + next.PlayerIndexType + " (" + next.ClientId + "|" + next.DevicePath + ")");
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
    }
}