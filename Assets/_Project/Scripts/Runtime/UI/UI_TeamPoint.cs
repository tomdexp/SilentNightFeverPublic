using System;
using _Project.Scripts.Runtime.Player;
using _Project.Scripts.Runtime.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UI_TeamPoint : MonoBehaviour
    {
        [Title("References")]
        [SerializeField, Required] private UIData _uiData;
        [SerializeField, Required] private Image _emptyPoint;
        [SerializeField, Required] private Image _filledPoint;
        [SerializeField, Required] private AnimationClip _emptyPointAnimation;
        [SerializeField, Required] private AnimationClip _filledPointAnimation;
        [SerializeField, Required] private Animator _animator;
        
        [Title("Settings")]
        [SerializeField, ReadOnly] private PlayerTeamType _teamType = PlayerTeamType.Z;
        
        public bool IsFilled { get; private set; }
        
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
            if (!_emptyPoint)
            {
                Logger.LogError("Empty point is null, this should not happen", Logger.LogType.Local, this);
            }
            if (!_filledPoint)
            {
                Logger.LogError("Filled point is null, this should not happen", Logger.LogType.Local, this);
            }
            if (!_animator)
            {
                Logger.LogError("Animator is null, this should not happen", Logger.LogType.Local, this);
            }
            if (!_uiData)
            {
                Logger.LogError("UI data is null, this should not happen", Logger.LogType.Local, this);
            }
        }

        public void Open()
        {
            _canvasGroup.Open();
        }
        
        public void Close()
        {
            _canvasGroup.Close();
        }

        public void Initialize(PlayerTeamType teamType)
        {
            if (teamType == PlayerTeamType.Z)
            {
                Logger.LogError("Team type is Z, this should not happen", Logger.LogType.Local, this);
                return;
            }
            _teamType = teamType;
            _emptyPoint.color = _teamType == PlayerTeamType.A ? _uiData.TeamAColor : _uiData.TeamBColor;
            _filledPoint.color = _teamType == PlayerTeamType.A ? _uiData.TeamAColor : _uiData.TeamBColor;
        }
        
        public void SetFilled(bool filled)
        {
            IsFilled = filled;
            if (IsFilled)
            {
                _animator.Play(_filledPointAnimation.name);
            }
            else
            {
                _animator.Play(_emptyPointAnimation.name);
            }
        }
    }
}