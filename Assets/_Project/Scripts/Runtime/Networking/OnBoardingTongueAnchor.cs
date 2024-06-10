using _Project.Scripts.Runtime.Player;
using _Project.Scripts.Runtime.Player.PlayerTongue;
using DG.Tweening;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Networking
{
    [RequireComponent(typeof(TongueAnchor))]
    public class OnBoardingTongueAnchor : NetworkBehaviour
    {
        [field: SerializeField] public PlayerIndexType PlayerIndexType { get; private set; } = PlayerIndexType.Z;
        [SerializeField, Required] private Light _light;
        [SerializeField] private Color _lightColorWhenReady = Color.green;
        [SerializeField] private Color _lightColorWhenNotReady = Color.red;
        [SerializeField] private float _lightChangeDuration = .5f;
        [SerializeField] private Ease _lightChangeEase = Ease.Linear;
        public readonly SyncVar<bool> IsTongueBound = new SyncVar<bool>();
        private TongueAnchor _tongueAnchor;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            IsTongueBound.OnChange += OnIsTongueBoundChange;
            OnIsTongueBoundChange(false, false, false);
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();
            IsTongueBound.OnChange -= OnIsTongueBoundChange;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            _tongueAnchor = GetComponent<TongueAnchor>();
            _tongueAnchor.OnTongueBindChange += OnTongueBindChange;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            _tongueAnchor.OnTongueBindChange -= OnTongueBindChange;
        }

        private void OnTongueBindChange(PlayerStickyTongue tongue)
        {
            if (!tongue)
            {
                IsTongueBound.Value = false;
                Logger.LogDebug($"Tongue unbound for Player {PlayerIndexType}", Logger.LogType.Server, context: this);
            }
            else
            {
                IsTongueBound.Value = true;
                PlayerIndexType = tongue.GetNetworkPlayer().GetPlayerIndexType();
                Logger.LogDebug("Tongue bound for Player " + PlayerIndexType, Logger.LogType.Server, context: this);
            }
        }
        
        private void OnIsTongueBoundChange(bool prev, bool next, bool asServer)
        {
            if (next == true)
            {
                _light.DOColor(_lightColorWhenReady, _lightChangeDuration).SetEase(_lightChangeEase);
            }
            else
            {
                _light.DOColor(_lightColorWhenNotReady, _lightChangeDuration).SetEase(_lightChangeEase);
            }
        }
        
    }
}