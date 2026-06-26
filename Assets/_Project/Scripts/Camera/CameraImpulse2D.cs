using TarTulla.Core;
using TarTulla.Game;
using UnityEngine;

namespace TarTulla.CameraSystems
{
    [DefaultExecutionOrder(40)]
    public class CameraImpulse2D : MonoBehaviour
    {
        const float DefaultLandingImpulse = 0.08f;
        const float DefaultRopeStretchImpulse = 0.05f;
        const float DefaultDangerImpulse = 0.12f;
        const float DecaySpeed = 14f;
        const float DangerImpulseCooldown = 0.35f;

        static Vector3 s_worldOffset;
        Vector3 currentOffset;
        float lastDangerImpulseTime = -999f;

        public static Vector3 WorldOffset => s_worldOffset;

        void OnEnable()
        {
            GameplayFeedbackEvents.JumperLanded += HandleJumperLanded;
            GameplayFeedbackEvents.RopeOverstretched += HandleRopeOverstretched;
            GameplayFeedbackEvents.DangerRatioChanged += HandleDangerRatioChanged;
        }

        void OnDisable()
        {
            GameplayFeedbackEvents.JumperLanded -= HandleJumperLanded;
            GameplayFeedbackEvents.RopeOverstretched -= HandleRopeOverstretched;
            GameplayFeedbackEvents.DangerRatioChanged -= HandleDangerRatioChanged;
            currentOffset = Vector3.zero;
            s_worldOffset = Vector3.zero;
        }

        void LateUpdate()
        {
            if (!IsCameraImpulseEnabled())
            {
                currentOffset = Vector3.zero;
                s_worldOffset = Vector3.zero;
                return;
            }

            currentOffset = Vector3.Lerp(currentOffset, Vector3.zero, Time.deltaTime * DecaySpeed);
            s_worldOffset = currentOffset;
        }

        void HandleJumperLanded(string _) =>
            ApplyImpulse(GetLandingImpulse(), 0.35f, 0.15f);

        void HandleRopeOverstretched(float _) =>
            ApplyImpulse(GetRopeStretchImpulse(), 0.25f, 0.2f);

        void HandleDangerRatioChanged(float ratio)
        {
            if (ratio < 0.85f || Time.time - lastDangerImpulseTime < DangerImpulseCooldown)
                return;

            lastDangerImpulseTime = Time.time;
            ApplyImpulse(GetDangerImpulse(), 0.4f, 0.35f);
        }

        void ApplyImpulse(float strength, float verticalWeight, float horizontalWeight)
        {
            if (!IsCameraImpulseEnabled() || strength <= 0f)
                return;

            var random = new Vector2(
                Random.Range(-horizontalWeight, horizontalWeight),
                Random.Range(-verticalWeight, verticalWeight));

            currentOffset += new Vector3(random.x, random.y, 0f) * strength;
        }

        static bool IsCameraImpulseEnabled()
        {
            var feedback = GetFeedback();
            return feedback == null || (feedback.enableFeedback && feedback.enableCameraImpulse);
        }

        static float GetLandingImpulse()
        {
            var feedback = GetFeedback();
            return feedback != null ? feedback.landingCameraImpulse : DefaultLandingImpulse;
        }

        static float GetRopeStretchImpulse()
        {
            var feedback = GetFeedback();
            return feedback != null ? feedback.ropeStretchCameraImpulse : DefaultRopeStretchImpulse;
        }

        static float GetDangerImpulse()
        {
            var feedback = GetFeedback();
            return feedback != null ? feedback.dangerCameraImpulse : DefaultDangerImpulse;
        }

        static TarTullaGameplayProfile.FeedbackTuning GetFeedback() =>
            TarTullaTuningAccess.GetActiveProfile()?.Feedback;
    }
}
