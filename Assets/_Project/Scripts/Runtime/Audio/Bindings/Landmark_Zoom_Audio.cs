using System;
using _Project.Scripts.Runtime.Landmarks.Zoom;
using FishNet.Object;
using UnityEngine;

namespace _Project.Scripts.Runtime.Audio.Bindings
{
    [RequireComponent(typeof(Landmark_Zoom))]
    public class Landmark_Zoom_Audio : NetworkBehaviour
    {
        private Landmark_Zoom _landmarkZoom;

        public override void OnStartServer()
        {
            _landmarkZoom = GetComponent<Landmark_Zoom>();
            _landmarkZoom.OnStartTurning += OnStartTurning;
            _landmarkZoom.OnStopTurning += OnStopTurning;
        }

        public override void OnStopServer()
        {
            _landmarkZoom.OnStartTurning -= OnStartTurning;
            _landmarkZoom.OnStopTurning -= OnStopTurning;
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