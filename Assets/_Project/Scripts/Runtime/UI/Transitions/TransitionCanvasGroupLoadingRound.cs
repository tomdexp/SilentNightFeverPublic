using System.Collections;
using System.Linq;
using _Project.Scripts.Runtime.Audio;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI.Transitions
{
    public class TransitionCanvasGroupLoadingRound : TransitionCanvasGroup
    {
        [SerializeField, Required] private UI_TeamScore _uiTeamAScore;
        [SerializeField, Required] private UI_TeamScore _uiTeamBScore;
        [SerializeField, Required] private MMFPlayerReplicated _feedbackTeamAWin;
        [SerializeField, Required] private MMFPlayerReplicated _feedbackTeamBWin;
        [SerializeField, Required] private MMFPlayerReplicated _feedbackOpen;
        [SerializeField, Required] private MMFPlayerReplicated _feedbackClose;
        [SerializeField] private float _delayBeforeTeamWinFeedback = 0.5f;
        [SerializeField] private float _delayAfterTeamWinFeedback = 0.5f;
        [SerializeField] private float _delayAfterCloseFeedback = 0.5f;
        
        public override IEnumerator BeginTransition()
        {
            yield return base.BeginTransition();
            var tween = DOTween.
                To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 1, Data.TransitionLoadingRoundFadeInDuration)
                .SetEase(Data.TransitionLoadingRoundFadeInEase);
            yield return tween.WaitForCompletion();
            if (IsServerStarted)
            {
                yield return new WaitForSeconds(_delayBeforeTeamWinFeedback);
                PlayerTeamType lastWinningTeam;
                if (GameManager.Instance.RoundsResults.Collection.Count == 0)
                {
                    Logger.LogWarning("No rounds results found, forcing a win for team A (should only appear when forcing a win via debug)", Logger.LogType.Local, this);
                    lastWinningTeam = PlayerTeamType.A;
                }
                else
                {
                    lastWinningTeam = GameManager.Instance.RoundsResults.Collection.Last().WinningTeam;
                }
                if (lastWinningTeam == PlayerTeamType.A)
                {
                    _feedbackTeamAWin.PlayFeedbacksForAll();
                    if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioNetworked(AudioManager.Instance.AudioManagerData.EventWinningTeamRoundTeamShiningStarFruit, AudioManager.Instance.gameObject);
                }
                else
                {
                    _feedbackTeamBWin.PlayFeedbacksForAll();
                    if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioNetworked(AudioManager.Instance.AudioManagerData.EventWinningTeamRoundTeamPlumsOfPassion, AudioManager.Instance.gameObject);
                }
                yield return new WaitForSeconds(_delayAfterTeamWinFeedback);
                _feedbackOpen.PlayFeedbacksForAll();
            }
            _canvasGroup.alpha = 1;
            _uiTeamAScore.Open();
            yield return new WaitForSeconds(.25f);
            _uiTeamBScore.Open();
        }

        public override IEnumerator EndTransition()
        {
            _uiTeamBScore.Close();
            yield return new WaitForSeconds(.25f);
            _uiTeamAScore.Close();
            yield return base.EndTransition();
            //yield return new WaitForSeconds(1f);
            _feedbackClose.PlayFeedbacksForAll();
            yield return new WaitForSeconds(_delayAfterCloseFeedback);
            var tween = DOTween
                .To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 0, Data.TransitionLoadingRoundFadeOutDuration)
                .SetEase(Data.TransitionLoadingRoundFadeOutEase);
            yield return tween.WaitForCompletion();
            yield return new WaitForSeconds(1f);
            _canvasGroup.alpha = 0;
        }
    }
}