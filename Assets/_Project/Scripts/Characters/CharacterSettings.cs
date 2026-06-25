using UnityEngine;
using TarTulla.Platforms;

namespace TarTulla.Characters
{
    [CreateAssetMenu(fileName = "CharacterSettings", menuName = "Tar&Tulla/Character Settings")]
    public class CharacterSettings : ScriptableObject
    {
        [Header("Jump")]
        [Tooltip("Upward impulse applied on landing from above.")]
        public float jumpForce = 11f;

        [Header("Movement")]
        public float maxFallSpeed = 25f;
        public float horizontalDamping = 2f;

        [Header("Landing")]
        [Tooltip("Maximum upward vertical speed still treated as a landing approach.")]
        public float landingVelocityThreshold = 0.5f;
        public float groundedGraceTime = 0.05f;
        public float jumpCooldown = 0.12f;
    }
}
