using System.Collections;
using _Project.Scripts.Runtime.UI.Transitions;
using _Project.Scripts.Runtime.Utils.Singletons;
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
        }
        
        public IEnumerator EndLoadingRoundTransition()
        {
            Logger.LogDebug("EndLoadingRoundTransition", Logger.LogType.Server, this);
            yield return _transitionLoadingRound.EndTransition();
        }
    }
}
