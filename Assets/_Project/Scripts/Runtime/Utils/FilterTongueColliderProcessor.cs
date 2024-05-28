using _Project.Scripts.Runtime.Player.PlayerTongue;
using Micosmo.SensorToolkit;


namespace _Project.Scripts.Runtime.Utils
{
    public class FilterTongueColliderProcessor : SignalProcessor
    {
        public override bool Process(ref Signal signal, Sensor sensor)
        {
            return signal.Object.TryGetComponent(out TongueCollider tongueCollider);
        }
    }
}