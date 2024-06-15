using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Networking.Rounds;
using _Project.Scripts.Runtime.Player;
using DG.Tweening;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI
{
    public class UI_TeamScore : NetworkBehaviour
    {
        [Title("References")]
        [SerializeField, Required] private UIData _uiData;
        [SerializeField, Required] private Image _teamImage;
        [SerializeField, Required] private UI_TeamPoint _teamPointPrefab;
        [SerializeField, Required] private HorizontalLayoutGroup _teamPointParent;
        
        [Title("Settings")]
        [SerializeField] private PlayerTeamType _teamType = PlayerTeamType.Z;
        
        private List<UI_TeamPoint> _teamPoints = new List<UI_TeamPoint>();
        
        private void Awake()
        {
            if (!_teamPointPrefab)
            {
                Logger.LogError("Team point prefab is null, this should not happen", Logger.LogType.Local, this);
            }
            if (!_teamPointParent)
            {
                Logger.LogError("Team point parent is null, this should not happen", Logger.LogType.Local, this);
            }
            if (!_teamImage)
            {
                Logger.LogError("Team image is null, this should not happen", Logger.LogType.Local, this);
            }
            if (!_uiData)
            {
                Logger.LogError("UI data is null, this should not happen", Logger.LogType.Local, this);
            }
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            StartCoroutine(OnStartNetworkCoroutine());
        }
        
        private IEnumerator OnStartNetworkCoroutine()
        {
            _teamImage.sprite = _teamType == PlayerTeamType.A ? _uiData.TeamASprite : _uiData.TeamBSprite;
            yield return new WaitUntil(() => GameManager.HasInstance);
            GameManager.Instance.Rounds.OnChange += OnRoundsChange;
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();
            if(GameManager.HasInstance) GameManager.Instance.Rounds.OnChange -= OnRoundsChange;
        }

        private void OnRoundsChange(SyncListOperation op, int index, Round oldItem, Round newItem, bool asServer)
        {
            SetupUI(GameManager.Instance.RequiredRoundsToWin.Value);
        }

        [Button(ButtonStyle.FoldoutButton)]
        public void SetupUI(int numberOfWinningPoints)
        {
            if (_teamPoints.Count > 0)
            {
                foreach (var teamPoint in _teamPoints)
                {
                    Destroy(teamPoint.gameObject);
                }
                _teamPoints.Clear();
            }
            if (_teamType == PlayerTeamType.Z)
            {
                Logger.LogError("Team type is Z, this should not happen", Logger.LogType.Local, this);
                return;
            }
            for (int i = 0; i < numberOfWinningPoints; i++)
            {
                var teamPoint = Instantiate(_teamPointPrefab, _teamPointParent.transform);
                teamPoint.Initialize(_teamType);
                teamPoint.SetFilled(false);
                _teamPoints.Add(teamPoint);
            }
        }
        
        
        private void UpdateUI()
        {
            if (_teamType == PlayerTeamType.Z)
            {
                Logger.LogError("Team type is Z, this should not happen", Logger.LogType.Local, this);
                return;
            }
            int teamRoundWins = GameManager.Instance.RoundsResults.Count(x => x.WinningTeam == _teamType);
            for (int i = 0; i < _teamPoints.Count; i++)
            {
                _teamPoints[i].SetFilled(i < teamRoundWins);
            }
        }

        [ObserversRpc]
        public void Open()
        {
            var sequence = DOTween.Sequence();
            sequence.AppendInterval(0.2f);
            foreach (var teamPoint in _teamPoints)
            {
                sequence.AppendCallback(() => teamPoint.Open());
                sequence.AppendInterval(0.2f);
            }
            sequence.OnComplete(UpdateUI);
            sequence.Play();
        }
        
        public void Close()
        {
            var sequence = DOTween.Sequence();
            // iterate in reverse order
            for (int i = _teamPoints.Count - 1; i >= 0; i--)
            {
                var x = i;
                sequence.AppendCallback(() => _teamPoints[x].Close());
                sequence.AppendInterval(0.2f);
            }
            sequence.Play();
        }
    }
}