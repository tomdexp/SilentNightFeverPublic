using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI
{
    public class UI_SelectorResolution : UI_SelectorBase
    {
        [SerializeField, Required] private TMP_Text _resolutionText;
        private List<Resolution> _supportedResolutions;
        private Resolution _currentResolution;
        
        private void Start()
        {
            SetupResolutions();
            UpdateResolutionText();
        }

        private void Update()
        {
            if (Time.frameCount % 60 != 0) return;

            if (!_supportedResolutions.Exists(r => r.width == _currentResolution.width && r.height == _currentResolution.height))
            {
                SetupResolutions();
                UpdateResolutionText();
            }
        }

        protected override void OnPreviousButtonClicked()
        {
            // get the current index of the current resolution
            int currentIndex = _supportedResolutions.FindIndex(r => r.width == _currentResolution.width && r.height == _currentResolution.height);
            // get the previous resolution with loop
            Resolution previousResolution = currentIndex == 0 ? _supportedResolutions[^1] : _supportedResolutions[currentIndex - 1];
            SetResolution(previousResolution);
        }

        protected override void OnNextButtonClicked()
        {
            // get the current index of the current resolution
            int currentIndex = _supportedResolutions.FindIndex(r => r.width == _currentResolution.width && r.height == _currentResolution.height);
            // get the next resolution with loop
            Resolution nextResolution = currentIndex == _supportedResolutions.Count - 1 ? _supportedResolutions[0] : _supportedResolutions[currentIndex + 1];
            SetResolution(nextResolution);
        }

        private void SetupResolutions()
        {
            _supportedResolutions = new List<Resolution>(Screen.resolutions);
            _currentResolution = Screen.currentResolution;
            
            Logger.LogInfo($"Resolutions Setup, current screen support {_supportedResolutions.Count} resolutions and current resolution is {_currentResolution.width}x{_currentResolution.height}", Logger.LogType.Local, this);
        }
        
        private void UpdateResolutionText()
        {
            _resolutionText.text = $"{_currentResolution.width}x{_currentResolution.height}";
        }
        
        private void SetResolution(Resolution resolution)
        {
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRateRatio);
            _currentResolution = resolution;
            UpdateResolutionText();
            
            Logger.LogInfo($"Resolution changed to {resolution.width}x{resolution.height}", Logger.LogType.Local, this);
        }
    }
}