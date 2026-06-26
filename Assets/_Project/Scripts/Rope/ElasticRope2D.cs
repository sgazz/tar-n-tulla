using UnityEngine;
using TarTulla.Characters;
using TarTulla.Core;
using TarTulla.Game;

namespace TarTulla.Rope
{
    public class ElasticRope2D : MonoBehaviour
    {
        static readonly Color RelaxedColor = new(0.85f, 0.75f, 0.45f);
        static readonly Color StretchedColor = new(0.95f, 0.35f, 0.25f);

        const float ClimbHeightThreshold = 0.15f;
        const float ClimbStretchThreshold = 0.1f;

        [SerializeField] RopeSettings settings;
        [SerializeField] JumperController2D jumperA;
        [SerializeField] JumperController2D jumperB;
        [SerializeField] LineRenderer lineRenderer;
        [SerializeField] bool logResolvedProfileValuesOnStart = true;

        Rigidbody2D bodyA;
        Rigidbody2D bodyB;

        bool HasTuningSource => TarTullaTuningAccess.HasActiveProfile || settings != null;

        float RestLength => GetRopeValue(r => r.restLength, s => s.restLength, 3f);
        float MaxLength => GetRopeValue(r => r.maxLength, s => s.maxLength, 4.5f);
        float SpringStrength => GetRopeValue(r => r.springStrength, s => s.springStrength, 50f);
        float Damping => GetRopeValue(r => r.damping, s => s.damping, 8f);
        float PullAssistStrength => GetRopeValue(r => r.pullAssistStrength, s => s.pullAssistStrength, 25f);
        float LineWidth => GetRopeValue(r => r.lineWidth, s => s.lineWidth, 0.08f);
        float OverstretchColorThreshold => GetRopeValue(r => r.overstretchColorThreshold, s => s.overstretchColorThreshold, 0.85f);
        float ClimbLeadPullFactor => GetRopeValue(r => r.climbLeadPullFactor, _ => 0.3f, 0.3f);
        float ClimbAnchorBoost => GetRopeValue(r => r.climbAnchorBoost, _ => 2f, 2f);
        float ClimbSlingJumpMultiplier => GetRopeValue(r => r.climbSlingJumpMultiplier, _ => 1.35f, 1.35f);

        public float StretchRatio { get; private set; } = 1f;

        const float FeedbackCooldown = 0.2f;
        float lastOverstretchFeedbackTime = -999f;
        float lastPullAssistFeedbackTime = -999f;

        void Awake()
        {
            CacheBodies();
            SetupLineRenderer();
        }

        void Start()
        {
            if (logResolvedProfileValuesOnStart)
            {
                Debug.Log(
                    $"[Tar&Tulla][Rope] Rope tuning: restLength={RestLength}, maxLength={MaxLength}, " +
                    $"springStrength={SpringStrength}, damping={Damping}, pullAssistStrength={PullAssistStrength}, " +
                    $"source={(TarTullaTuningAccess.HasActiveProfile ? TarTullaTuningAccess.GetActiveProfile().name : settings?.name ?? "fallback")}",
                    this);
            }
        }

        void FixedUpdate()
        {
            if (!HasTuningSource || bodyA == null || bodyB == null)
                return;

            ApplyRopeForces();
        }

        void LateUpdate()
        {
            UpdateLineRenderer();
        }

        public void Configure(RopeSettings ropeSettings, JumperController2D a, JumperController2D b)
        {
            settings = ropeSettings;
            jumperA = a;
            jumperB = b;
            CacheBodies();
            SetupLineRenderer();

            if (logResolvedProfileValuesOnStart)
            {
                Debug.Log(
                    $"[Tar&Tulla][Rope] Rope tuning after configure: restLength={RestLength}, springStrength={SpringStrength}",
                    this);
            }
        }

        public float GetClimbJumpMultiplier(JumperController2D jumper)
        {
            if (!IsClimbTensionState(out JumperController2D anchor, out _))
                return 1f;

            if (jumper != anchor || !anchor.IsGrounded)
                return 1f;

            return ClimbSlingJumpMultiplier;
        }

        void CacheBodies()
        {
            bodyA = jumperA != null ? jumperA.Rigidbody : null;
            bodyB = jumperB != null ? jumperB.Rigidbody : null;
        }

        void ApplyRopeForces()
        {
            Vector2 posA = bodyA.position;
            Vector2 posB = bodyB.position;
            Vector2 delta = posB - posA;
            float distance = delta.magnitude;
            StretchRatio = RestLength > 0.001f ? distance / RestLength : 1f;

            if (distance < 0.001f)
                return;

            Vector2 direction = delta / distance;
            float stretch = distance - RestLength;

            Vector2 springForce = direction * (stretch * SpringStrength);

            Vector2 relativeVelocity = bodyB.linearVelocity - bodyA.linearVelocity;
            float velocityAlongRope = Vector2.Dot(relativeVelocity, direction);
            Vector2 dampingForce = direction * (velocityAlongRope * Damping);

            Vector2 totalForce = springForce + dampingForce;

            if (IsClimbTensionState(out JumperController2D anchor, out JumperController2D lead)
                && stretch > ClimbStretchThreshold)
            {
                var anchorBody = anchor == jumperA ? bodyA : bodyB;
                var leadBody = lead == jumperA ? bodyA : bodyB;
                Vector2 anchorToLead = (leadBody.position - anchorBody.position).normalized;
                Vector2 climbForce = anchorToLead * (stretch * SpringStrength)
                    + anchorToLead * (Vector2.Dot(leadBody.linearVelocity - anchorBody.linearVelocity, anchorToLead) * Damping);

                anchorBody.AddForce(climbForce * ClimbAnchorBoost);
                leadBody.AddForce(-climbForce * ClimbLeadPullFactor);
            }
            else
            {
                bodyA.AddForce(totalForce);
                bodyB.AddForce(-totalForce);
            }

            if (distance > MaxLength)
            {
                float excess = distance - MaxLength;
                Vector2 limitForce = direction * (excess * SpringStrength * 3f);

                if (IsClimbTensionState(out JumperController2D limitAnchor, out JumperController2D limitLead))
                {
                    var limitAnchorBody = limitAnchor == jumperA ? bodyA : bodyB;
                    var limitLeadBody = limitLead == jumperA ? bodyA : bodyB;
                    limitAnchorBody.AddForce(limitForce * ClimbAnchorBoost);
                    limitLeadBody.AddForce(-limitForce * ClimbLeadPullFactor);
                }
                else
                {
                    bodyA.AddForce(limitForce);
                    bodyB.AddForce(-limitForce);
                }
            }

            ApplyPullAssist(posA, posB, direction, stretch);

            float visualStretchRatio = MaxLength > 0.001f ? distance / MaxLength : 0f;
            if (visualStretchRatio >= OverstretchColorThreshold
                && Time.time - lastOverstretchFeedbackTime >= FeedbackCooldown)
            {
                lastOverstretchFeedbackTime = Time.time;
                GameplayFeedbackEvents.InvokeRopeOverstretched(visualStretchRatio);
            }
        }

        bool IsClimbTensionState(out JumperController2D anchor, out JumperController2D lead)
        {
            anchor = null;
            lead = null;

            if (jumperA == null || jumperB == null)
                return false;

            float heightDelta = jumperB.transform.position.y - jumperA.transform.position.y;

            if (jumperA.IsGrounded && !jumperB.IsGrounded && heightDelta > ClimbHeightThreshold)
            {
                anchor = jumperA;
                lead = jumperB;
                return true;
            }

            if (jumperB.IsGrounded && !jumperA.IsGrounded && heightDelta < -ClimbHeightThreshold)
            {
                anchor = jumperB;
                lead = jumperA;
                return true;
            }

            return false;
        }

        void ApplyPullAssist(Vector2 posA, Vector2 posB, Vector2 direction, float stretch)
        {
            if (PullAssistStrength <= 0f)
                return;

            bool aGrounded = jumperA.IsGrounded;
            bool bGrounded = jumperB.IsGrounded;

            if (aGrounded && !bGrounded && posB.y < posA.y - ClimbHeightThreshold)
            {
                bodyB.AddForce(direction * PullAssistStrength);
                TryInvokePullAssistFeedback();
                return;
            }

            if (bGrounded && !aGrounded && posA.y < posB.y - ClimbHeightThreshold)
            {
                bodyA.AddForce(-direction * PullAssistStrength);
                TryInvokePullAssistFeedback();
                return;
            }

            if (!IsClimbTensionState(out JumperController2D anchor, out JumperController2D lead) || stretch <= ClimbStretchThreshold)
                return;

            var anchorBody = anchor == jumperA ? bodyA : bodyB;
            var leadBody = lead == jumperA ? bodyA : bodyB;
            Vector2 hoistDirection = (leadBody.position - anchorBody.position).normalized;
            anchorBody.AddForce(hoistDirection * PullAssistStrength);
            TryInvokePullAssistFeedback();
        }

        void TryInvokePullAssistFeedback()
        {
            if (Time.time - lastPullAssistFeedbackTime < FeedbackCooldown)
                return;

            lastPullAssistFeedbackTime = Time.time;
            GameplayFeedbackEvents.InvokePullAssistTriggered(PullAssistStrength);
        }

        void SetupLineRenderer()
        {
            if (lineRenderer == null)
                lineRenderer = GetComponent<LineRenderer>();

            if (lineRenderer == null || !HasTuningSource)
                return;

            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = LineWidth;
            lineRenderer.endWidth = LineWidth;

            if (lineRenderer.material == null)
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

            lineRenderer.sortingLayerName = TarTulla.CameraSystems.VisualSortingLayers.Gameplay;
            lineRenderer.sortingOrder = 10;
        }

        void UpdateLineRenderer()
        {
            if (lineRenderer == null || bodyA == null || bodyB == null || !HasTuningSource)
                return;

            lineRenderer.SetPosition(0, bodyA.position);
            lineRenderer.SetPosition(1, bodyB.position);

            float distance = Vector2.Distance(bodyA.position, bodyB.position);
            float stretchRatio = distance / MaxLength;
            float widthScale = Mathf.Lerp(1f, 1.4f, Mathf.InverseLerp(RestLength, MaxLength, distance));

            lineRenderer.startWidth = LineWidth * widthScale;
            lineRenderer.endWidth = LineWidth * widthScale;

            Color color = stretchRatio >= OverstretchColorThreshold ? StretchedColor : RelaxedColor;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }

        float GetRopeValue(
            System.Func<TarTullaGameplayProfile.RopeTuning, float> fromProfile,
            System.Func<RopeSettings, float> fromSettings,
            float fallback)
        {
            var profile = TarTullaTuningAccess.GetActiveProfile();
            if (profile != null)
                return fromProfile(profile.Rope);

            if (settings != null)
                return fromSettings(settings);

            return fallback;
        }
    }
}
