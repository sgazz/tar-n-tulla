using UnityEngine;
using TarTulla.Game;
using TarTulla.Input;

namespace TarTulla.Characters
{
    [RequireComponent(typeof(JumperController2D), typeof(Rigidbody2D))]
    [DefaultExecutionOrder(10)]
    public class JumperAirControl2D : MonoBehaviour
    {
        [SerializeField] MobileTiltInput2D tiltInput;
        [SerializeField] AirControlSettings settings;
        [SerializeField] bool enableDebugLogs;

        JumperController2D jumper;
        Rigidbody2D rb;

        bool HasTuningSource => TarTullaTuningAccess.HasActiveProfile || settings != null;

        float MaxHorizontalAirSpeed => GetTiltValue(t => t.maxHorizontalAirSpeed, s => s.maxHorizontalAirSpeed, 5f);
        float AirAcceleration => GetTiltValue(t => t.airAcceleration, s => s.airAcceleration, 20f);
        float GroundedControlMultiplier => GetTiltValue(t => t.groundedControlMultiplier, s => s.groundedControlMultiplier, 0.15f);
        float AirborneControlMultiplier => GetTiltValue(t => t.airborneControlMultiplier, s => s.airborneControlMultiplier, 1f);
        float InputDeadZone => GetTiltValue(t => t.inputDeadZone, s => s.inputDeadZone, 0.08f);

        void Awake()
        {
            jumper = GetComponent<JumperController2D>();
            rb = GetComponent<Rigidbody2D>();
        }

        void FixedUpdate()
        {
            if (!HasTuningSource || tiltInput == null)
                return;

            float input = tiltInput.HorizontalInput;
            if (Mathf.Abs(input) < InputDeadZone)
                return;

            float controlMultiplier = jumper.IsGrounded
                ? GroundedControlMultiplier
                : AirborneControlMultiplier;

            float force = input * AirAcceleration * controlMultiplier;
            rb.AddForce(Vector2.right * force, ForceMode2D.Force);

            var velocity = rb.linearVelocity;
            velocity.x = Mathf.Clamp(velocity.x, -MaxHorizontalAirSpeed, MaxHorizontalAirSpeed);
            rb.linearVelocity = new Vector2(velocity.x, velocity.y);

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[Tar&Tulla][{jumper.JumperName}][AirControl] input={input:F2} force={force:F1} vx={velocity.x:F2} grounded={jumper.IsGrounded}",
                    this);
            }
        }

        public void Configure(MobileTiltInput2D input, AirControlSettings airControlSettings)
        {
            tiltInput = input;
            settings = airControlSettings;
        }

        float GetTiltValue(
            System.Func<TarTullaGameplayProfile.TiltTuning, float> fromProfile,
            System.Func<AirControlSettings, float> fromSettings,
            float fallback)
        {
            var profile = TarTullaTuningAccess.GetActiveProfile();
            if (profile != null)
                return fromProfile(profile.Tilt);

            if (settings != null)
                return fromSettings(settings);

            return fallback;
        }
    }
}
