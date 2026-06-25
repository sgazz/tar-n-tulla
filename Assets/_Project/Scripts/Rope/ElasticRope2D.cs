using UnityEngine;
using TarTulla.Characters;

namespace TarTulla.Rope
{
    public class ElasticRope2D : MonoBehaviour
    {
        static readonly Color RelaxedColor = new(0.85f, 0.75f, 0.45f);
        static readonly Color StretchedColor = new(0.95f, 0.35f, 0.25f);

        [SerializeField] RopeSettings settings;
        [SerializeField] JumperController2D jumperA;
        [SerializeField] JumperController2D jumperB;
        [SerializeField] LineRenderer lineRenderer;

        Rigidbody2D bodyA;
        Rigidbody2D bodyB;

        void Awake()
        {
            CacheBodies();
            SetupLineRenderer();
        }

        void FixedUpdate()
        {
            if (settings == null || bodyA == null || bodyB == null)
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

            if (distance < 0.001f)
                return;

            Vector2 direction = delta / distance;
            float stretch = distance - settings.restLength;

            Vector2 springForce = direction * (stretch * settings.springStrength);

            Vector2 relativeVelocity = bodyB.linearVelocity - bodyA.linearVelocity;
            float velocityAlongRope = Vector2.Dot(relativeVelocity, direction);
            Vector2 dampingForce = direction * (velocityAlongRope * settings.damping);

            Vector2 totalForce = springForce + dampingForce;
            bodyA.AddForce(totalForce);
            bodyB.AddForce(-totalForce);

            if (distance > settings.maxLength)
            {
                float excess = distance - settings.maxLength;
                Vector2 limitForce = direction * (excess * settings.springStrength * 3f);
                bodyA.AddForce(limitForce);
                bodyB.AddForce(-limitForce);
            }

            ApplyPullAssist(posA, posB, direction, distance);
        }

        void ApplyPullAssist(Vector2 posA, Vector2 posB, Vector2 direction, float distance)
        {
            if (settings.pullAssistStrength <= 0f)
                return;

            bool aGrounded = jumperA.IsGrounded;
            bool bGrounded = jumperB.IsGrounded;

            if (aGrounded && !bGrounded && posB.y < posA.y - 0.1f)
            {
                bodyB.AddForce(direction * settings.pullAssistStrength);
                return;
            }

            if (bGrounded && !aGrounded && posA.y < posB.y - 0.1f)
            {
                bodyA.AddForce(-direction * settings.pullAssistStrength);
            }
        }

        void SetupLineRenderer()
        {
            if (lineRenderer == null)
                lineRenderer = GetComponent<LineRenderer>();

            if (lineRenderer == null || settings == null)
                return;

            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = settings.lineWidth;
            lineRenderer.endWidth = settings.lineWidth;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.sortingOrder = 10;
        }

        void UpdateLineRenderer()
        {
            if (lineRenderer == null || bodyA == null || bodyB == null || settings == null)
                return;

            lineRenderer.SetPosition(0, bodyA.position);
            lineRenderer.SetPosition(1, bodyB.position);

            float distance = Vector2.Distance(bodyA.position, bodyB.position);
            float stretchRatio = distance / settings.maxLength;
            float widthScale = Mathf.Lerp(1f, 1.4f, Mathf.InverseLerp(settings.restLength, settings.maxLength, distance));

            lineRenderer.startWidth = settings.lineWidth * widthScale;
            lineRenderer.endWidth = settings.lineWidth * widthScale;

            Color color = stretchRatio >= settings.overstretchColorThreshold ? StretchedColor : RelaxedColor;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
    }
}
