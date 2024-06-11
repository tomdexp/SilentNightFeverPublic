using System.Collections;
using System.Linq;
using FishNet.Object;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Networking
{
    public class OnBoardingReadyChecker : NetworkBehaviour
    {
        private OnBoardingTongueAnchor[] _onBoardingTongueAnchors;
        private bool _isReady = false;
        public override void OnStartServer()
        {
            base.OnStartServer();
            StartCoroutine(OnStartServerCoroutine());
        }
        
        private IEnumerator OnStartServerCoroutine()
        {
            yield return new WaitForSeconds(1);
            _onBoardingTongueAnchors = FindObjectsByType<OnBoardingTongueAnchor>(FindObjectsSortMode.None);
            while (_onBoardingTongueAnchors.Length != 4)
            {
                _onBoardingTongueAnchors = FindObjectsByType<OnBoardingTongueAnchor>(FindObjectsSortMode.None);
                yield return new WaitForSeconds(1);
            } 
            foreach (var onBoardingTongueAnchor in _onBoardingTongueAnchors)
            {
                onBoardingTongueAnchor.IsTongueBound.OnChange += OnIsTongueBoundChange;
            }
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            foreach (var onBoardingTongueAnchor in _onBoardingTongueAnchors)
            {
                onBoardingTongueAnchor.IsTongueBound.OnChange -= OnIsTongueBoundChange;
            }
        }

        private void OnIsTongueBoundChange(bool prev, bool next, bool asServer)
        {
            if (_isReady) return;
            if (next == false) return;
            if (_onBoardingTongueAnchors.Any(onBoardingTongueAnchor => onBoardingTongueAnchor.IsTongueBound.Value == false))
            {
                return;
            }
            _isReady = true;
            Logger.LogInfo("All players have their tongue bound in the OnBoarding, ready to start the game !", Logger.LogType.Server, this);
            GameManager.Instance.LoadGameScene();
        }
    }
}