using _Project.Scripts.Runtime.Player;

namespace _Project.Scripts.Runtime.Landmarks.Components
{
    public interface ILandmarkComponent
    {
        public Landmark Landmark { get; set; }

        public void SetValue(float value, PlayerIndexType sourcePlayer) {}
    }
}