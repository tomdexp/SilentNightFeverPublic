using System;
using System.Collections;
using _Project.Scripts.Runtime.Landmarks.Zoom;
using FishNet.Object;
using UnityEngine;

namespace _Project.Scripts.Runtime.Audio.Bindings
{
    [RequireComponent(typeof(Landmark_Zoom), typeof(AkGameObj))]
    public class Landmark_Zoom_Audio : NetworkBehaviour
    {
        private Landmark_Zoom _landmarkZoom;
        private AkGameObj _akGameObj;
        private float _lastRtpc;
        private float _minTurnSpeed = 0f;
        private float _maxTurnSpeed = 0.37f;

        public override void OnStartServer()
        {
            _landmarkZoom = GetComponent<Landmark_Zoom>();
            _akGameObj = GetComponent<AkGameObj>();
            
            _landmarkZoom.OnStartTurning += OnStartTurning;
            _landmarkZoom.OnStopTurning += OnStopTurning;

            StartCoroutine(TryRegisterEmitter());
        }
        
        private IEnumerator TryRegisterEmitter()
        {
            while (!AudioManager.HasInstance)
            {
                yield return null;
            }
            AudioManager.Instance.RegisterEmitter(_akGameObj);
        }

        public override void OnStopServer()
        {
            _landmarkZoom.OnStartTurning -= OnStartTurning;
            _landmarkZoom.OnStopTurning -= OnStopTurning;
        }

        private void FixedUpdate()
        {
            if (!AudioManager.HasInstance) return;
            var t = Mathf.InverseLerp(_minTurnSpeed, _maxTurnSpeed, _landmarkZoom.Speed);
            var rtpc = Mathf.Lerp(AudioManager.Instance.AudioManagerData.RTPC_GP_LM_SatelliteSpeed_MinValue, AudioManager.Instance.AudioManagerData.RTPC_GP_LM_SatelliteSpeed_MaxValue, t);
            if (Mathf.Approximately(rtpc, _lastRtpc)) return;
            _lastRtpc = rtpc;
            AudioManager.Instance.SetLocalRTPC(AudioManager.Instance.AudioManagerData.RTPC_GP_LM_SatelliteSpeed, rtpc, transform.gameObject);
        }

        private void OnStartTurning()
        {
            AudioManager.Instance.PlayAudioNetworked(AudioManager.Instance.AudioManagerData.EventLandmarkZoomStartTurning, transform.gameObject);
        }
        
        private void OnStopTurning()
        {
            AudioManager.Instance.PlayAudioNetworked(AudioManager.Instance.AudioManagerData.EventLandmarkZoomStopTurning, transform.gameObject);
        }
    }
}
