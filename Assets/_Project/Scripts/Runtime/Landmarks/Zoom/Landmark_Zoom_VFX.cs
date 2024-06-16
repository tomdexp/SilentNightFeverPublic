using System;
using _Project.Scripts.Runtime.Audio;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using _Project.Scripts.Runtime.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Landmarks.Zoom
{
    [RequireComponent(typeof(Landmark_Zoom))]
    public class Landmark_Zoom_VFX : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private RotationTracker _sliderRotationTracker;
        [SerializeField] private ParticleSystem _teamAParticles;
        [SerializeField] private ParticleSystem _teamBParticles;
        [Tooltip("The threshold at which the particles will start playing")]
        [SerializeField] private float _threshold = 5f;

        private float _absSignedAngle;
        private float _t;
        private bool _isTeamVFXPlayingA;
        private bool _isTeamVFXPlayingB;

        private void Awake()
        {
            _sliderRotationTracker.OnSignedAngleChanged += OnSignedAngleChanged;
        }

        private void OnDestroy()
        {
            _sliderRotationTracker.OnSignedAngleChanged -= OnSignedAngleChanged;
        }

        private void OnSignedAngleChanged(float newSignedAngle)
        {
            // If its negative, then we need to calculate the new FOV for team A
            // If its positive, then we need to calculate the new FOV for team B
            _absSignedAngle = Mathf.Abs(newSignedAngle);
            if (_absSignedAngle > _threshold)
            {
                if (newSignedAngle < 0)
                {
                    if (!_isTeamVFXPlayingA)
                    {
                        _isTeamVFXPlayingA = true;
                        _teamAParticles.Play();
                        AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventOnStartZoomElectricity, transform.gameObject);
                    }
                }
                else
                {
                    if (!_isTeamVFXPlayingB)
                    {
                        _isTeamVFXPlayingB = true;
                        _teamBParticles.Play();
                        AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventOnStartZoomElectricity, transform.gameObject);
                    }
                }
            }
            else
            {
                if (_isTeamVFXPlayingA || _isTeamVFXPlayingB)
                {
                    _isTeamVFXPlayingA = false;
                    _isTeamVFXPlayingB = false;
                    _teamAParticles.Stop();
                    _teamBParticles.Stop();
                    AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventOnStopZoomElectricity, transform.gameObject);
                }
            }
        }
    }
}