using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    public class MetroLizardsNPC : MonoBehaviour
    {
        // marker class for the parent game object of the NPC in the metro scene

        public void Open()
        {
            transform.DOKill();
            transform.DOScale(1,1f);
        }
        
        public void Close()
        {
            transform.DOKill();
            var sequence = DOTween.Sequence(transform);
            sequence.AppendInterval(.5f);
            sequence.Append(transform.DOScale(0,1f));
            sequence.Play();
        }
    }
}