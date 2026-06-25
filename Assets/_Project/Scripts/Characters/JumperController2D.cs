using UnityEngine;
using TarTulla.Platforms;

namespace TarTulla.Characters
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class JumperController2D : MonoBehaviour
    {
        const float LandingNormalMinY = 0.35f;

        [SerializeField] CharacterSettings settings;
        [SerializeField] string jumperName = "Jumper";
        [SerializeField] bool enableDebugLogs = true;

        Rigidbody2D rb;
        float groundedUntil;
        float jumpCooldownUntil;

        public bool IsGrounded => Time.time < groundedUntil;
        public Rigidbody2D Rigidbody => rb;
        public string JumperName => jumperName;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        void FixedUpdate()
        {
            if (settings == null)
                return;

            var velocity = rb.linearVelocity;

            if (velocity.y < -settings.maxFallSpeed)
                velocity.y = -settings.maxFallSpeed;

            if (Mathf.Abs(velocity.x) > 0.01f)
            {
                float damp = 1f - settings.horizontalDamping * Time.fixedDeltaTime;
                velocity.x *= Mathf.Clamp(damp, 0f, 1f);
            }

            rb.linearVelocity = velocity;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            Log($"Collision with {collision.gameObject.name}");

            if (settings == null)
            {
                Log("Landing skipped: CharacterSettings is missing.");
                return;
            }

            if (HasBasicPlatform(collision))
                Log("Platform detected");
            else
                Log("No BasicPlatform on collider (static fallback may still apply).");

            if (!TryGetLandingContact(collision, out var contact))
            {
                Log("Landing rejected: no upward-facing contact.");
                return;
            }

            groundedUntil = Time.time + settings.groundedGraceTime;

            float verticalVelocity = rb.linearVelocity.y;
            if (!IsLandingApproach(verticalVelocity))
            {
                Log($"Landing rejected: vertical velocity too high ({verticalVelocity:F2}).");
                return;
            }

            if (Time.time < jumpCooldownUntil)
            {
                Log($"Landing rejected: jump cooldown active ({jumpCooldownUntil - Time.time:F2}s left).");
                return;
            }

            Log($"Landing accepted, applying jump impulse: {settings.jumpForce}");
            Log($"Vertical velocity before jump: {verticalVelocity:F2}");
            ApplyJump();
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            if (settings == null || !TryGetLandingContact(collision, out _))
                return;

            groundedUntil = Time.time + settings.groundedGraceTime;
        }

        public void Configure(CharacterSettings characterSettings, string name)
        {
            settings = characterSettings;
            jumperName = name;
        }

        void ApplyJump()
        {
            var velocity = rb.linearVelocity;
            if (velocity.y < 0f)
                velocity.y = 0f;

            rb.linearVelocity = velocity;
            rb.AddForce(Vector2.up * settings.jumpForce, ForceMode2D.Impulse);
            jumpCooldownUntil = Time.time + settings.jumpCooldown;
        }

        bool TryGetLandingContact(Collision2D collision, out ContactPoint2D contact)
        {
            contact = default;

            if (!IsPlatformCollider(collision.collider))
                return false;

            for (int i = 0; i < collision.contactCount; i++)
            {
                var candidate = collision.GetContact(i);
                if (candidate.normal.y > LandingNormalMinY)
                {
                    contact = candidate;
                    Log($"Landing from above detected (normal.y={candidate.normal.y:F2}).");
                    return true;
                }
            }

            return false;
        }

        static bool IsPlatformCollider(Collider2D collider)
        {
            if (collider.GetComponent<BasicPlatform>() != null)
                return true;

            var body = collider.attachedRigidbody;
            return body == null || body.bodyType == RigidbodyType2D.Static;
        }

        static bool HasBasicPlatform(Collision2D collision)
        {
            return collision.collider.GetComponent<BasicPlatform>() != null;
        }

        bool IsLandingApproach(float verticalVelocity)
        {
            return verticalVelocity <= settings.landingVelocityThreshold;
        }

        void Log(string message)
        {
            if (!enableDebugLogs)
                return;

            Debug.Log($"[Tar&Tulla][{jumperName}] {message}", this);
        }

        void OnDrawGizmosSelected()
        {
            if (!IsGrounded)
                return;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.4f);
        }
    }
}
