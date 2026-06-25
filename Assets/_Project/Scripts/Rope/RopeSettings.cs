using UnityEngine;

namespace TarTulla.Rope
{
    [CreateAssetMenu(fileName = "RopeSettings", menuName = "Tar&Tulla/Rope Settings")]
    public class RopeSettings : ScriptableObject
    {
        [Header("Length")]
        public float restLength = 3f;
        public float maxLength = 4.5f;

        [Header("Forces")]
        public float springStrength = 50f;
        public float damping = 8f;
        public float pullAssistStrength = 25f;

        [Header("Visual")]
        public float lineWidth = 0.08f;
        [Range(0.5f, 1f)]
        public float overstretchColorThreshold = 0.85f;
    }
}
