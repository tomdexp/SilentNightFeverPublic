using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Audio
{
    public class AkEventSNF : MonoBehaviour
    {
        [Title("Event References")]
        [SerializeField] private AK.Wwise.Event _akEvent;
    }
}