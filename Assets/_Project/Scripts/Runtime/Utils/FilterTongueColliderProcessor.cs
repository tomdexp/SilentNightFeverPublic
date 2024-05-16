using _Project.Scripts.Runtime.Player.PlayerTongue;
using Micosmo.SensorToolkit;

namespace _Project.Scripts.Runtime.Utils
{
    public class FilterTongueColliderProcessor : SignalProcessor
    {
        // If the signal object has a TongueCollider component, return true
        public override bool Process(ref Signal signal, Sensor sensor)
        {
            return signal.Object.TryGetComponent(out TongueCollider tongueCollider);
        }
    }
}