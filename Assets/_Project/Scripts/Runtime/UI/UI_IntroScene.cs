using System;
using System.Collections;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Utils;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI
{
    public class UI_IntroScene : MonoBehaviour
    {
        [Title("References")]
        [SerializeField, Required] private RawImage _gameLogo;
        [SerializeField, Required] private TMP_Text _gameTitle;
        [SerializeField, Required] private RawImage _cnamLogo;
        [SerializeField, Required] private RawImage _magelisLogo;
        [SerializeField, Required] private RawImage _poitiersLogo;
        [SerializeField, Required] private RawImage _p20Logo;
        [SerializeField, Required] private RawImage _p20LogoStressed;
        [SerializeField, Required] private RawImage _p20LogoStressedUltra;
        [SerializeField, Required] private RawImage _wwiseLogo;
        
        [Title("Settings")]
        [SerializeField] private float _secondsBeforeFirstLogo = 1.0f;
        [SerializeField] private float _secondsBetweenLogos = 1.0f;
        [SerializeField] private float _secondsBeforeSceneChange = 1.0f;
        
        [Title("Settings", "Game Logo")]
        [SerializeField] private float _secondsFadeInDurationGameLogo = 1.0f;
        [SerializeField] private float _secondsBeforeFadeOutGameLogo = 1.0f;
        [SerializeField] private float _secondsFadeOutDurationGameLogo = 1.0f;
        [SerializeField] private Ease _easeFadeInGameLogo = Ease.Linear;
        [SerializeField] private Ease _easeFadeOutGameLogo = Ease.Linear;
        [SerializeField] private float _secondsFadeInDurationGameTitle = 1.0f;
        [SerializeField] private float _secondsFadeOutDurationGameTitle = 1.0f;
        [SerializeField] private Ease _easeFadeInGameTitle = Ease.Linear;
        [SerializeField] private Ease _easeFadeOutGameTitle = Ease.Linear;
        
        [Title("Settings", "Cnam Logo")]
        [SerializeField] private float _secondsBeforeFadeOutCnamAndMagelisLogo = 1.0f;
        [SerializeField] private float _secondsFadeInDurationCnamLogo = 1.0f;
        [SerializeField] private float _secondsFadeOutDurationCnamLogo = 1.0f;
        [SerializeField] private Ease _easeFadeInCnamLogo = Ease.Linear;
        [SerializeField] private Ease _easeFadeOutCnamLogo = Ease.Linear;
        
        [Title("Settings", "Magelis Logo")]
        [SerializeField] private float _secondsFadeInDurationMagelisLogo = 1.0f;
        [SerializeField] private float _secondsFadeOutDurationMagelisLogo = 1.0f;
        [SerializeField] private Ease _easeFadeInMagelisLogo = Ease.Linear;
        [SerializeField] private Ease _easeFadeOutMagelisLogo = Ease.Linear;
        
        [Title("Settings", "P20 Logo")]
        [SerializeField] private float _secondsFadeInDurationP20Logo = 1.0f;
        [SerializeField] private float _secondsBeforeFadeOutP20Logo = 1.0f;
        [SerializeField] private float _secondsFadeOutDurationP20Logo = 1.0f;
        [SerializeField] private Ease _easeFadeInP20Logo = Ease.Linear;
        [SerializeField] private Ease _easeFadeOutP20Logo = Ease.Linear;
        
        [Title("Settings", "Wwise Logo")]
        [SerializeField] private float _secondsFadeInDurationWwiseLogo = 1.0f;
        [SerializeField] private float _secondsBeforeFadeOutWwiseLogo = 1.0f;
        [SerializeField] private float _secondsFadeOutDurationWwiseLogo = 1.0f;
        [SerializeField] private Ease _easeFadeInWwiseLogo = Ease.Linear;
        [SerializeField] private Ease _easeFadeOutWwiseLogo = Ease.Linear;

        private IEnumerator Start()
        {
            // 1st WWISE LOGO
            yield return new WaitForSeconds(_secondsBeforeFirstLogo);
            var tweenFadeInWwiseLogo = _wwiseLogo.DOFade(1.0f, _secondsFadeInDurationWwiseLogo).SetEase(_easeFadeInWwiseLogo);
            yield return tweenFadeInWwiseLogo.WaitForCompletion();
            yield return new WaitForSeconds(_secondsBeforeFadeOutWwiseLogo);
            var tweenFadeOutWwiseLogo = _wwiseLogo.DOFade(0.0f, _secondsFadeOutDurationWwiseLogo).SetEase(_easeFadeOutWwiseLogo);
            yield return tweenFadeOutWwiseLogo.WaitForCompletion();
            
            yield return new WaitForSeconds(_secondsBetweenLogos);
            
            // 2nd CNAM AND MAGELIS LOGOS AND POITIERS LOGO
            var tweenFadeInCnamLogo = _cnamLogo.DOFade(1.0f, _secondsFadeInDurationCnamLogo).SetEase(_easeFadeInCnamLogo);
            var tweenFadeInMagelisLogo = _magelisLogo.DOFade(1.0f, _secondsFadeInDurationMagelisLogo).SetEase(_easeFadeInMagelisLogo);
            var tweenFadeInPoitiersLogo = _poitiersLogo.DOFade(1.0f, _secondsFadeInDurationMagelisLogo).SetEase(_easeFadeInMagelisLogo);
            yield return tweenFadeInCnamLogo.WaitForCompletion();
            yield return new WaitForSeconds(_secondsBeforeFadeOutCnamAndMagelisLogo);
            var tweenFadeOutCnamLogo = _cnamLogo.DOFade(0.0f, _secondsFadeOutDurationCnamLogo).SetEase(_easeFadeOutCnamLogo);
            var tweenFadeOutMagelisLogo = _magelisLogo.DOFade(0.0f, _secondsFadeOutDurationMagelisLogo).SetEase(_easeFadeOutMagelisLogo);
            var tweenFadeOutPoitiersLogo = _poitiersLogo.DOFade(0.0f, _secondsFadeOutDurationMagelisLogo).SetEase(_easeFadeOutMagelisLogo);
            yield return tweenFadeOutCnamLogo.WaitForCompletion();
            
            
            yield return new WaitForSeconds(_secondsBetweenLogos);
            
            // 3rd P20 LOGO
            // Between 9h00 and 17h00 pm, the logo is normal
            // Between 17h00 and 23h00 pm, the logo is stressed
            // Between 23h00 and 5h00 am, the logo is ultra stressed
            // Between 5h00 and 9h00 am, the logo is stressed
            var now = DateTime.Now;
            bool isStressed = now.Hour is >= 17 and < 23 or >= 5 and < 9;
            bool isUltraStressed = now.Hour is >= 23 or < 5;
            if (!isStressed && !isUltraStressed)
            {
                var tweenFadeInP20Logo = _p20Logo.DOFade(1.0f, _secondsFadeInDurationP20Logo).SetEase(_easeFadeInP20Logo);
                yield return tweenFadeInP20Logo.WaitForCompletion();
                yield return new WaitForSeconds(_secondsBeforeFadeOutP20Logo);
                var tweenFadeOutP20Logo = _p20Logo.DOFade(0.0f, _secondsFadeOutDurationP20Logo).SetEase(_easeFadeOutP20Logo);
                yield return tweenFadeOutP20Logo.WaitForCompletion();
            }
            else if (isStressed)
            {
                var tweenFadeInP20LogoStressed = _p20LogoStressed.DOFade(1.0f, _secondsFadeInDurationP20Logo).SetEase(_easeFadeInP20Logo);
                yield return tweenFadeInP20LogoStressed.WaitForCompletion();
                yield return new WaitForSeconds(_secondsBeforeFadeOutP20Logo);
                var tweenFadeOutP20LogoStressed = _p20LogoStressed.DOFade(0.0f, _secondsFadeOutDurationP20Logo).SetEase(_easeFadeOutP20Logo);
                yield return tweenFadeOutP20LogoStressed.WaitForCompletion();
            }
            else
            {
                var tweenFadeInP20LogoStressedUltra = _p20LogoStressedUltra.DOFade(1.0f, _secondsFadeInDurationP20Logo).SetEase(_easeFadeInP20Logo);
                yield return tweenFadeInP20LogoStressedUltra.WaitForCompletion();
                yield return new WaitForSeconds(_secondsBeforeFadeOutP20Logo);
                var tweenFadeOutP20LogoStressedUltra = _p20LogoStressedUltra.DOFade(0.0f, _secondsFadeOutDurationP20Logo).SetEase(_easeFadeOutP20Logo);
                yield return tweenFadeOutP20LogoStressedUltra.WaitForCompletion();
            }

            yield return new WaitForSeconds(_secondsBetweenLogos);
            
            // 4th GAME LOGO AND TITLE
            var tweenFadeInGameLogo = _gameLogo.DOFade(1.0f, _secondsFadeInDurationGameLogo).SetEase(_easeFadeInGameLogo);
            var tweenFadeInGameTitle = _gameTitle.DOFade(1.0f, _secondsFadeInDurationGameTitle).SetEase(_easeFadeInGameTitle);
            //yield return tweenFadeInGameLogo.WaitForCompletion();
            yield return tweenFadeInGameTitle.WaitForCompletion();
            
            yield return new WaitForSeconds(_secondsBeforeFadeOutGameLogo);
            
            var tweenFadeOutGameLogo = _gameLogo.DOFade(0.0f, _secondsFadeOutDurationGameLogo).SetEase(_easeFadeOutGameLogo);
            var tweenFadeOutGameTitle = _gameTitle.DOFade(0.0f, _secondsFadeOutDurationGameTitle).SetEase(_easeFadeOutGameTitle);
            //yield return tweenFadeOutGameLogo.WaitForCompletion();
            yield return tweenFadeOutGameTitle.WaitForCompletion();

            yield return new WaitForSeconds(_secondsBeforeSceneChange);
            SceneManager.LoadScene(SceneType.MenuV2Scene.ToString());
        }
    }
}