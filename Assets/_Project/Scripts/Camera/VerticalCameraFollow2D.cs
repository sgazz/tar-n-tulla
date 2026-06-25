using UnityEngine;
using TarTulla.Game;

namespace TarTulla.CameraSystems
{
    public class VerticalCameraFollow2D : MonoBehaviour
    {
        const string TarFallbackName = "Tar";
        const string TullaFallbackName = "Tulla";

        [SerializeField] Transform targetA;
        [SerializeField] Transform targetB;
        [SerializeField] float smoothTime = 0.25f;
        [SerializeField] float verticalOffset = 1.5f;
        [SerializeField] bool allowSmallDownwardCorrection = true;
        [SerializeField] float maxDownwardCorrection = 1.5f;
        [SerializeField] float horizontalFollow = 0.4f;
        [SerializeField] float maxHorizontalOffset = 2.5f;
        [SerializeField] bool findTargetsOnStart = true;

        Vector3 velocity;
        float highestY;

        float SmoothTime => GetCameraValue(c => c.smoothTime, smoothTime);
        float VerticalOffset => GetCameraValue(c => c.verticalOffset, verticalOffset);
        bool AllowSmallDownwardCorrection => GetCameraBool(c => c.allowSmallDownwardCorrection, allowSmallDownwardCorrection);
        float MaxDownwardCorrection => GetCameraValue(c => c.maxDownwardCorrection, maxDownwardCorrection);

        void Start()
        {
            if (findTargetsOnStart && (targetA == null || targetB == null))
                TryFindTargets();

            highestY = transform.position.y;
        }

        void LateUpdate()
        {
            if (targetA == null || targetB == null)
            {
                TryFindTargets();
                if (targetA == null || targetB == null)
                    return;
            }

            Vector2 midpoint = (targetA.position + targetB.position) * 0.5f;
            float targetY = midpoint.y + VerticalOffset;

            if (targetY > highestY)
                highestY = targetY;

            float minAllowedY = AllowSmallDownwardCorrection
                ? highestY - MaxDownwardCorrection
                : highestY;
            float desiredY = Mathf.Max(targetY, minAllowedY);

            float targetX = Mathf.Clamp(midpoint.x * horizontalFollow, -maxHorizontalOffset, maxHorizontalOffset);
            var desiredPosition = new Vector3(targetX, desiredY, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, SmoothTime);
        }

        public void SetTargets(Transform a, Transform b)
        {
            targetA = a;
            targetB = b;
        }

        public void ResetToTarget(Vector3 worldPosition)
        {
            transform.position = worldPosition;
            highestY = worldPosition.y;
            velocity = Vector3.zero;
        }

        public void ResetToTargets() => ResetCamera();

        public void ResetCamera()
        {
            if (targetA == null || targetB == null)
            {
                TryFindTargets();
                if (targetA == null || targetB == null)
                    return;
            }

            Vector2 midpoint = (targetA.position + targetB.position) * 0.5f;
            float targetX = Mathf.Clamp(midpoint.x * horizontalFollow, -maxHorizontalOffset, maxHorizontalOffset);
            var position = new Vector3(targetX, midpoint.y + VerticalOffset, transform.position.z);
            ResetToTarget(position);
        }

        void TryFindTargets()
        {
            if (targetA == null)
            {
                var tar = GameObject.Find(TarFallbackName);
                if (tar != null)
                    targetA = tar.transform;
            }

            if (targetB == null)
            {
                var tulla = GameObject.Find(TullaFallbackName);
                if (tulla != null)
                    targetB = tulla.transform;
            }
        }

        float GetCameraValue(System.Func<TarTullaGameplayProfile.CameraTuning, float> fromProfile, float fallback)
        {
            var profile = TarTullaTuningAccess.GetActiveProfile();
            return profile != null ? fromProfile(profile.Camera) : fallback;
        }

        bool GetCameraBool(System.Func<TarTullaGameplayProfile.CameraTuning, bool> fromProfile, bool fallback)
        {
            var profile = TarTullaTuningAccess.GetActiveProfile();
            return profile != null ? fromProfile(profile.Camera) : fallback;
        }
    }
}
