using System.Collections;
using _Project.Scripts.Runtime.UI.Transitions;
using _Project.Scripts.Runtime.Utils;
using _Project.Scripts.Runtime.Utils.Singletons;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI
{
    public class TransitionManager : NetworkPersistentSingleton<TransitionManager>
    {
        [Title("References")]
        [SerializeField, Required] private TransitionCanvasGroup _transitionSceneChange;
        [SerializeField, Required] private TransitionCanvasGroup _transitionLoadingGame;
        [SerializeField, Required] private TransitionCanvasGroup _transitionLoadingRound;
        [SerializeField, Required] private UI_TeamScore _uiTeamAScore;
        [SerializeField, Required] private UI_TeamScore _uiTeamBScore;

        protected override void Awake()
        {
            base.Awake();
            if (FindAnyObjectByType<LocalBlackFade>())
            {
                var canvasGroup = FindAnyObjectByType<LocalBlackFade>().CanvasGroup;
                canvasGroup.DOFade(0, 2f).SetEase(Ease.Linear);
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        public IEnumerator BeginSceneChangeTransition()
        {
            Logger.LogDebug("BeginSceneChangeTransition", Logger.LogType.Server, this);
            yield return _transitionSceneChange.BeginTransition();
        }
        
        public IEnumerator EndSceneChangeTransition()
        {
            Logger.LogDebug("EndSceneChangeTransition", Logger.LogType.Server, this);
            yield return _transitionSceneChange.EndTransition();
        }
        
        public IEnumerator BeginLoadingGameTransition()
        {
            Logger.LogDebug("BeginLoadingGameTransition", Logger.LogType.Server, this);
            yield return _transitionLoadingGame.BeginTransition();
        }
        
        public IEnumerator EndLoadingGameTransition()
        {
            Logger.LogDebug("EndLoadingGameTransition", Logger.LogType.Server, this);
            yield return _transitionLoadingGame.EndTransition();
        }
        
        public IEnumerator BeginLoadingRoundTransition()
        {
            Logger.LogDebug("BeginLoadingRoundTransition", Logger.LogType.Server, this);
            yield return _transitionLoadingRound.BeginTransition();
            _uiTeamAScore.Open();
            yield return new WaitForSeconds(.25f);
            _uiTeamBScore.Open();
        }
        
        public IEnumerator EndLoadingRoundTransition()
        {
            Logger.LogDebug("EndLoadingRoundTransition", Logger.LogType.Server, this);
            _uiTeamBScore.Close();
            yield return new WaitForSeconds(.25f);
            _uiTeamAScore.Close();
            //yield return new WaitForSeconds(1f);
            yield return _transitionLoadingRound.EndTransition();
        }
    }
}
