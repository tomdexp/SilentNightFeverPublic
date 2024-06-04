using _Project.Scripts.Runtime.Landmarks.Voodoo;
using DG.Tweening;
using Lofelt.NiceVibrations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Runtime.Player
{
    [CreateAssetMenu(fileName = nameof(PlayerData), menuName = "Scriptable Objects/" + nameof(PlayerData))]
    public class PlayerData : ScriptableObject
    {
        [Title("Player Color Settings")]
        public Color PlayerAColor = Color.red;
        public Color PlayerBColor = Color.blue;
        public Color PlayerCColor = Color.green;
        public Color PlayerDColor = Color.yellow;
        
        [Title("NPC Color Settings")]
        public Color[] NPCColors = new Color[4] {Color.red, Color.blue, Color.green, Color.yellow};
        
        [Title("Player Movement Settings")]
        [PropertyRange(0,100)] public float PlayerMovementSpeed = 5f;
        [InfoBox("Not Implemented Yet", InfoMessageType.Error)]
        [Tooltip("NOT IMPLEMENTED YET")]
        [PropertyRange(0,100)] public float PlayerMinMovementSpeed = 1f;
        [InfoBox("Not Implemented Yet", InfoMessageType.Error)]
        [Tooltip("NOT IMPLEMENTED YET")]
        [PropertyRange(0,100)] public float PlayerMaxMovementSpeed = 10f;
        [PropertyRange(0,100)] public float PlayerRotationSpeed = 5f;

        [Title("Player Animation Settings")] 
        public float PlayerMaxSpeedForAnimation = 4.2f;

        [Title("Player Camera Settings")] 
        public float CameraOffsetRadius = 3;
        public float CameraHeight = 6;
        [Tooltip("The default FOV of the camera")]
        public float CameraFov = 60;
        public float CameraFovChangeDuration = 1f;
        public Ease CameraFovChangeEase = Ease.Linear;
        
        [Title("Player Size Settings")]
        [PropertyRange(0,100)] public float PlayerDefaultSize = 1f;
        [PropertyRange(0,100)] public float PlayerMinSize = 0.5f;
        [PropertyRange(0,100)] public float PlayerMaxSize = 2f;
        [PropertyRange(0,100)] public float PlayerSizeUpChangeDuration = 1f;
        [PropertyRange(0,100)] public float PlayerSizeDownChangeDuration = 1f;
        public Ease PlayerSizeUpChangeEase = Ease.InOutBounce;
        public Ease PlayerSizeDownChangeEase = Ease.InOutBounce;
        
        [Title("Tongue Settings")]
        public float TongueThrowSpeed = 1f;
        public float TongueRetractSpeed = 2f;
        public Ease TongueThrowEase = Ease.Linear;
        public Ease TongueRetractEase = Ease.Linear;
        [Tooltip("When throwing the tongue, the player mass will change for X seconds, allowing the player to not be thrown away by the tongue")]
        public float SmoothPlayerMassChangeOnTongueMoveDuration = 0.5f;
        [Tooltip("This correspond to the amount of time the Tongue will be attached to an interactable object before it detaches itself")]
        public float TongueInteractDuration = 0.5f;
        [Tooltip("(FOR ONLINE INTERACTIONS ONLY) How much force an other player tongue attached to a player will move it")]
        public float OtherTongueAttachedForce = 10f;
        [Tooltip("(FOR ONLINE INTERACTIONS ONLY) If the other player is below this distance, the force will NOT be applied")]
        public float OtherTongueMinDistance = 1f;
        [Tooltip("If the tongue is not attached to anything, it will retract after this time in seconds, correspond to the duration the tongue is suspended in the air")]
        public float TongueMissDuration = 0.3f;
        [Tooltip("In percent, the distance the tongue will elongate for a miss, the normal distance is based on the FOV Sensor Length, this is mostly esthetic")]
        public float TongueMissPercentOfMaxDistance = 0.5f;
        [Tooltip("If the distance between the player and the tip of the tongue is above this value, the tongue will begin to gain tension")]
        public float TongueBreakDistance = 4.5f;
        [Tooltip("How much time after a break it will take for the player to be able to use the tongue again in seconds")]
        public float TongueBreakCooldown = 5f;
        [Tooltip("Where the tongue is at break distance, the tensions builds up in seconds, if the tongue is still at break distance after this time, it will break.")]
        public float TongueBreakTensionSeconds = 2f;
        [Tooltip("To avoid player doing back and forward to reset the tension timer, we decrease the tension by this factor on the update loop")]
        public float TongueBreakTensionLossFactor = 0.1f;
        [Tooltip("The time before the sound of the kitchen food being eaten is played (WARNING : It is cumulated with SecondsBeforeFruitIsConsumed)")]
        public float SecondsBeforeFruitSoundEaten = 0.5f;
        [Tooltip("The time before the fruit is consumed after the sound has been played (WARNING : It is cumulated with SecondsBeforeFruitSoundEaten)")]
        public float SecondsBeforeFruitIsConsumed = 1f;
        
        [Title("Tongue Feedbacks")]
        public AnimationCurve TongueTensionVibrationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public HapticClip TongueTensionVibrationClip;
        
        [Title("Landmark Datas")]
        public LandmarkData_Voodoo LandmarkData_Voodoo;
    }
}