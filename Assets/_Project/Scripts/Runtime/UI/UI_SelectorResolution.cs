using System;
using System.Collections;
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
                Logger.LogInfo("Screen changed ! Updating resolutions list and current resolution");
                SetupResolutions();
                UpdateResolutionText();
            }
        }

        protected override void OnPreviousButtonClicked()
        {
            int currentIndex = _supportedResolutions.FindIndex(r => 
                r.width == _currentResolution.width 
                && r.height == _currentResolution.height
                && r.refreshRateRatio.Equals(_currentResolution.refreshRateRatio));
            if (currentIndex == -1)
            {
                Logger.LogWarning("Current resolution not found in supported resolutions list", Logger.LogType.Local, this);
                SetupResolutions();
                return;
            }
            if (currentIndex == 0)
            {
                Logger.LogWarning("Current resolution is the first one in the list", Logger.LogType.Local, this);
                return;
            }
            Resolution previousResolution = _supportedResolutions[currentIndex - 1];
            SetResolution(previousResolution);
        }

        protected override void OnNextButtonClicked()
        {
            int currentIndex = _supportedResolutions.FindIndex(r => 
                r.width == _currentResolution.width 
                && r.height == _currentResolution.height
                && r.refreshRateRatio.Equals(_currentResolution.refreshRateRatio));
            if (currentIndex == -1)
            {
                Logger.LogWarning("Current resolution not found in supported resolutions list", Logger.LogType.Local, this);
                SetupResolutions();
                return;
            }
            if (currentIndex == _supportedResolutions.Count - 1)
            {
                Logger.LogWarning("Current resolution is the last one in the list", Logger.LogType.Local, this);
                return;
            }

            Logger.LogTrace("Index of the current resolution in the list is " + currentIndex +  $" and correspond to {_supportedResolutions[currentIndex]}", Logger.LogType.Local, this);
            Logger.LogTrace("Index of the next resolution in the list is " + (currentIndex + 1) +  $" and correspond to {_supportedResolutions[currentIndex + 1]}", Logger.LogType.Local, this);
            
            Resolution nextResolution = _supportedResolutions[currentIndex + 1];
            SetResolution(nextResolution);
        }

        private void SetupResolutions()
        {
            _supportedResolutions = new List<Resolution>(Screen.resolutions);
            _currentResolution = Screen.currentResolution;
            
            Logger.LogInfo($"Resolutions Setup, current screen support {_supportedResolutions.Count} resolutions and current resolution is {_currentResolution.width}x{_currentResolution.height}", Logger.LogType.Local, this);
            foreach (var resolution in _supportedResolutions)
            {
                Logger.LogInfo($"Available resolution on this monitor: {resolution.width}x{resolution.height}", Logger.LogType.Local, this);
            }
        }
        
        private void UpdateResolutionText()
        {
            _resolutionText.text = _currentResolution.ToString();
        }
        
        private void SetResolution(Resolution resolution)
        {
            Logger.LogInfo($"Attempting to set resolution to {resolution.width}x{resolution.height}", Logger.LogType.Local, this);
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRateRatio);
            _currentResolution = resolution;
            UpdateResolutionText();
        }
    }
}