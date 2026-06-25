using UnityEngine;

namespace TarTulla.Platforms
{
    public class BasicPlatform : MonoBehaviour
    {
        [SerializeField] float bounceMultiplier = 0f;

        public float BounceMultiplier => bounceMultiplier;
    }
}
