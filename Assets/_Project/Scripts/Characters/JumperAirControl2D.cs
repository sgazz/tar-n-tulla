using UnityEngine;
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

        void Awake()
        {
            jumper = GetComponent<JumperController2D>();
            rb = GetComponent<Rigidbody2D>();
        }

        void FixedUpdate()
        {
            if (settings == null || tiltInput == null)
                return;

            float input = tiltInput.HorizontalInput;
            if (Mathf.Abs(input) < settings.inputDeadZone)
                return;

            float controlMultiplier = jumper.IsGrounded
                ? settings.groundedControlMultiplier
                : settings.airborneControlMultiplier;

            float force = input * settings.airAcceleration * controlMultiplier;
            rb.AddForce(Vector2.right * force, ForceMode2D.Force);

            var velocity = rb.linearVelocity;
            velocity.x = Mathf.Clamp(
                velocity.x,
                -settings.maxHorizontalAirSpeed,
                settings.maxHorizontalAirSpeed);
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
    }
}
