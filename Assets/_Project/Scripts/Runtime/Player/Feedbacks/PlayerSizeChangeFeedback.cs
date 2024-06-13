using System;
using System.Collections;
using _Project.Scripts.Runtime.Networking;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Player.Feedbacks
{
    [RequireComponent(typeof(NetworkPlayer))]
    public class PlayerSizeChangeFeedback : MonoBehaviour
    {
        [Title("Reference")]
        [SerializeField] private ParticleSystem _sizeChangeParticles;
        private NetworkPlayer _networkPlayer;
        
        private void Awake()
        {
            _networkPlayer = GetComponent<NetworkPlayer>();
        }

        private void Start()
        {
            _networkPlayer.OnSizeChanged += OnSizeChanged;
        }

        private void OnDestroy()
        {
            _networkPlayer.OnSizeChanged -= OnSizeChanged;
        }

        private void OnSizeChanged(bool playAudio)
        {
            StartCoroutine(OnSizeChangedCoroutine(playAudio));
        }
        
        private IEnumerator OnSizeChangedCoroutine(bool playAudio)
        {
            if (!playAudio) yield break; // this mean that the feedback is not wanted and it is just a reset size change
            yield return new WaitForSeconds(0.5f); // TODO : Hardcoded feedback delay
            _sizeChangeParticles.Play();
        }
    }
}