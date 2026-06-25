using UnityEngine;
using TarTulla.Game;
using TarTulla.Platforms;
using TarTulla.Rope;

namespace TarTulla.Characters
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class JumperController2D : MonoBehaviour
    {
        const float LandingNormalMinY = 0.45f;
        const float UpwardVelocityRejectThreshold = 0.05f;

        [SerializeField] CharacterSettings settings;
        [SerializeField] string jumperName = "Jumper";
        [SerializeField] bool enableDebugLogs = true;
        [SerializeField] bool logResolvedProfileValuesOnStart = true;

        Rigidbody2D rb;
        ElasticRope2D rope;
        float groundedUntil;
        float jumpCooldownUntil;

        public bool IsGrounded => Time.time < groundedUntil;
        public Rigidbody2D Rigidbody => rb;
        public string JumperName => jumperName;

        bool HasTuningSource => TarTullaTuningAccess.HasActiveProfile || settings != null;

        float JumpForce => GetCharacterValue(s => s.jumpForce, s => settings.jumpForce, 11f);
        float GravityScale => GetCharacterValue(s => s.gravityScale, _ => 3f, 3f);
        float MaxFallSpeed => GetCharacterValue(s => s.maxFallSpeed, s => settings.maxFallSpeed, 25f);
        float HorizontalDamping => GetCharacterValue(s => s.horizontalDamping, s => settings.horizontalDamping, 2f);
        float LandingVelocityThreshold => GetCharacterValue(s => s.landingVelocityThreshold, s => settings.landingVelocityThreshold, 0.5f);
        float GroundedGraceTime => GetCharacterValue(s => s.groundedGraceTime, s => settings.groundedGraceTime, 0.05f);
        float JumpCooldown => GetCharacterValue(s => s.jumpCooldown, s => settings.jumpCooldown, 0.12f);

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        void Start()
        {
            ApplyResolvedGravityScale();

            if (logResolvedProfileValuesOnStart)
            {
                Debug.Log(
                    $"[Tar&Tulla][{jumperName}] Character tuning: jumpForce={JumpForce}, gravityScale={GravityScale}, " +
                    $"maxFallSpeed={MaxFallSpeed}, jumpCooldown={JumpCooldown}, source={(TarTullaTuningAccess.HasActiveProfile ? Profile.name : settings?.name ?? "fallback")}",
                    this);
            }
        }

        void FixedUpdate()
        {
            if (!HasTuningSource)
                return;

            ApplyResolvedGravityScale();

            var velocity = rb.linearVelocity;

            if (velocity.y < -MaxFallSpeed)
                velocity.y = -MaxFallSpeed;

            if (Mathf.Abs(velocity.x) > 0.01f)
            {
                float damp = 1f - HorizontalDamping * Time.fixedDeltaTime;
                velocity.x *= Mathf.Clamp(damp, 0f, 1f);
            }

            rb.linearVelocity = velocity;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (!HasTuningSource)
                return;

            if (!HasBasicPlatform(collision))
                return;

            Log("One-way platform contact");

            float verticalVelocity = rb.linearVelocity.y;
            if (verticalVelocity > UpwardVelocityRejectThreshold)
            {
                Log("Landing rejected: moving upward");
                return;
            }

            if (!TryGetLandingContact(collision, out _))
            {
                Log("Landing rejected: bad normal");
                return;
            }

            groundedUntil = Time.time + GroundedGraceTime;

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

            Log("Landing accepted");
            Log($"Applying jump impulse: {JumpForce}");
            ApplyJump();
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            if (!HasTuningSource || !HasBasicPlatform(collision))
                return;

            if (rb.linearVelocity.y > UpwardVelocityRejectThreshold)
                return;

            if (!TryGetLandingContact(collision, out _))
                return;

            groundedUntil = Time.time + GroundedGraceTime;
        }

        public void Configure(CharacterSettings characterSettings, string name)
        {
            settings = characterSettings;
            jumperName = name;
            ApplyResolvedGravityScale();
        }

        void ApplyJump()
        {
            var velocity = rb.linearVelocity;
            if (velocity.y < 0f)
                velocity.y = 0f;

            rb.linearVelocity = velocity;
            float jumpForce = JumpForce;
            rope ??= FindFirstObjectByType<ElasticRope2D>();
            if (rope != null)
                jumpForce *= rope.GetClimbJumpMultiplier(this);

            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpCooldownUntil = Time.time + JumpCooldown;
        }

        void ApplyResolvedGravityScale()
        {
            if (rb == null)
                return;

            rb.gravityScale = GravityScale;
        }

        float GetCharacterValue(
            System.Func<TarTullaGameplayProfile.CharacterTuning, float> fromProfile,
            System.Func<CharacterSettings, float> fromSettings,
            float fallback)
        {
            var profile = TarTullaTuningAccess.GetActiveProfile();
            if (profile != null)
                return fromProfile(profile.Character);

            if (settings != null)
                return fromSettings(settings);

            return fallback;
        }

        TarTullaGameplayProfile Profile => TarTullaTuningAccess.GetActiveProfile();

        bool TryGetLandingContact(Collision2D collision, out ContactPoint2D contact)
        {
            contact = default;

            if (!HasBasicPlatform(collision))
                return false;

            for (int i = 0; i < collision.contactCount; i++)
            {
                var candidate = collision.GetContact(i);
                if (candidate.normal.y > LandingNormalMinY)
                {
                    contact = candidate;
                    return true;
                }
            }

            return false;
        }

        static bool HasBasicPlatform(Collision2D collision)
        {
            return collision.collider.GetComponent<BasicPlatform>() != null;
        }

        bool IsLandingApproach(float verticalVelocity)
        {
            return verticalVelocity <= LandingVelocityThreshold;
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
