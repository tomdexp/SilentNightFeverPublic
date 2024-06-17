using System;
using System.Collections;
using System.Globalization;
using _Project.Scripts.Runtime.Audio;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI
{
    public class UI_BindSliderToRTPC : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private AK.Wwise.RTPC _rtpc;
        [SerializeField, Required] private TMP_Text _valueText;
        [SerializeField, Required] private TMP_Text _minValueText;
        [SerializeField, Required] private TMP_Text _maxValueText;
        
        
        [Title("Settings")]
        [SerializeField] private SliderType _sliderType;
        [SerializeField] private int _minValue = 0;
        [SerializeField] private int _maxValue = 100;
        
        [Title("Debug (Read-Only)")]
        [SerializeField, ReadOnly] private float _currentValue;

        private enum SliderType
        {
            Music,
            Ambiance,
            SFX,
            Landmarks,
            HighPass,
            LowPass,
            Notch
        }
        private Slider _slider;

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => AudioManager.HasInstance);
            _slider = GetComponentInChildren<Slider>();
            if (!_slider)
            {
                Logger.LogError("No slider found in children of " + name, context:this);
                yield break;
            }
            _minValueText.text = _minValue.ToString();
            _maxValueText.text = _maxValue.ToString();
            _slider.minValue = _minValue;
            _slider.maxValue = _maxValue;
            _currentValue = _sliderType switch
            {
                SliderType.Music => AudioManager.Instance.AudioManagerData.SettingsMusicSliderDefaultValue,
                SliderType.Ambiance => AudioManager.Instance.AudioManagerData.SettingsAmbianceSliderDefaultValue,
                SliderType.SFX => AudioManager.Instance.AudioManagerData.SettingsSFXSliderDefaultValue,
                SliderType.Landmarks => AudioManager.Instance.AudioManagerData.SettingsLandmarksSliderDefaultValue,
                SliderType.HighPass => AudioManager.Instance.AudioManagerData.SettingsHighPassSliderDefaultValue,
                SliderType.LowPass => AudioManager.Instance.AudioManagerData.SettingsLowPassSliderDefaultValue,
                SliderType.Notch => AudioManager.Instance.AudioManagerData.SettingsNotchSliderDefaultValue,
                _ => throw new ArgumentOutOfRangeException()
            };
            _slider.value = _currentValue;
            _valueText.text = _currentValue.ToString(CultureInfo.InvariantCulture);
            if(AudioManager.HasInstance) AudioManager.Instance.SetLocalRTPC(_rtpc, _currentValue);
            _slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnDestroy()
        {
            _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float newValue)
        {
            _currentValue = newValue;
            _valueText.text = newValue.ToString(CultureInfo.InvariantCulture);
            if(AudioManager.HasInstance) AudioManager.Instance.SetLocalRTPC(_rtpc, newValue);
        }
    }
}