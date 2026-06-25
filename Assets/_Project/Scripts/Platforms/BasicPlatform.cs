using UnityEngine;

namespace TarTulla.Platforms
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public class BasicPlatform : MonoBehaviour
    {
        [SerializeField] float bounceMultiplier;
        [SerializeField] bool useOneWayBehavior = true;
        [SerializeField] float surfaceArc = 140f;

        public float BounceMultiplier => bounceMultiplier;
        public bool UseOneWayBehavior => useOneWayBehavior;
        public float SurfaceArc => surfaceArc;

        void Awake()
        {
            ApplyOneWaySetup();
        }

        void Reset()
        {
            ApplyOneWaySetup();
        }

        void OnValidate()
        {
            ApplyOneWaySetup();
        }

        public void Configure(bool oneWayBehavior, float arc)
        {
            useOneWayBehavior = oneWayBehavior;
            surfaceArc = arc;
            ApplyOneWaySetup();
        }

        public void ApplyOneWaySetup()
        {
            var box = GetComponent<BoxCollider2D>();
            if (box == null)
                box = gameObject.AddComponent<BoxCollider2D>();

            if (!useOneWayBehavior)
            {
                box.usedByEffector = false;
                return;
            }

            var effector = GetComponent<PlatformEffector2D>();
            if (effector == null)
                effector = gameObject.AddComponent<PlatformEffector2D>();

            box.usedByEffector = true;
            effector.useOneWay = true;
            effector.surfaceArc = surfaceArc;
            effector.useSideFriction = false;
            effector.useSideBounce = false;
        }
    }
}
