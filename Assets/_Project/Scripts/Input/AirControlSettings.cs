using UnityEngine;

namespace TarTulla.Input
{
    [CreateAssetMenu(fileName = "AirControlSettings", menuName = "Tar&Tulla/Air Control Settings")]
    public class AirControlSettings : ScriptableObject
    {
        [Header("Tilt")]
        public float tiltSensitivity = 8f;
        public float inputDeadZone = 0.08f;
        public float smoothing = 8f;

        [Header("Movement")]
        public float maxHorizontalAirSpeed = 5f;
        public float airAcceleration = 20f;
        public float groundedControlMultiplier = 0.15f;
        public float airborneControlMultiplier = 1f;

        [Header("Debug")]
        public bool debugUseKeyboardInEditor = true;
    }
}
