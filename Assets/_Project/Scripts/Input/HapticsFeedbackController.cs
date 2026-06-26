using TarTulla.Core;
using TarTulla.Game;
using UnityEngine;

namespace TarTulla.Input
{
    public class HapticsFeedbackController : MonoBehaviour
    {
        const float DefaultLandingStrength = 0.35f;
        const float DefaultRopeStretchStrength = 0.25f;
        const float DefaultDangerStrength = 0.5f;
        const float EventCooldown = 0.2f;
        const float DangerHapticThreshold = 0.75f;

        [SerializeField] bool logHapticsInEditor;

        float lastLandingTime = -999f;
        float lastRopeStretchTime = -999f;
        float lastPullAssistTime = -999f;
        float lastDangerTime = -999f;

        void OnEnable()
        {
            GameplayFeedbackEvents.JumperLanded += HandleJumperLanded;
            GameplayFeedbackEvents.RopeOverstretched += HandleRopeOverstretched;
            GameplayFeedbackEvents.PullAssistTriggered += HandlePullAssistTriggered;
            GameplayFeedbackEvents.DangerRatioChanged += HandleDangerRatioChanged;
        }

        void OnDisable()
        {
            GameplayFeedbackEvents.JumperLanded -= HandleJumperLanded;
            GameplayFeedbackEvents.RopeOverstretched -= HandleRopeOverstretched;
            GameplayFeedbackEvents.PullAssistTriggered -= HandlePullAssistTriggered;
            GameplayFeedbackEvents.DangerRatioChanged -= HandleDangerRatioChanged;
        }

        void HandleJumperLanded(string jumperName)
        {
            if (!CanFire(ref lastLandingTime))
                return;

            TriggerHaptic(GetLandingStrength(), $"landing ({jumperName})");
        }

        void HandleRopeOverstretched(float stretchRatio)
        {
            if (!CanFire(ref lastRopeStretchTime))
                return;

            TriggerHaptic(GetRopeStretchStrength(), $"rope overstretch ({stretchRatio:F2})");
        }

        void HandlePullAssistTriggered(float strength)
        {
            if (!CanFire(ref lastPullAssistTime))
                return;

            float normalized = Mathf.Clamp01(strength / 50f);
            TriggerHaptic(GetLandingStrength() * 0.6f * Mathf.Max(0.35f, normalized), $"pull assist ({strength:F1})");
        }

        void HandleDangerRatioChanged(float ratio)
        {
            if (ratio < DangerHapticThreshold || !CanFire(ref lastDangerTime))
                return;

            TriggerHaptic(GetDangerStrength() * ratio, $"danger ({ratio:F2})");
        }

        bool CanFire(ref float lastTime)
        {
            if (!IsHapticsEnabled())
                return false;

            if (Time.time - lastTime < EventCooldown)
                return false;

            lastTime = Time.time;
            return true;
        }

        void TriggerHaptic(float strength, string debugLabel)
        {
            if (strength <= 0.05f)
                return;

#if UNITY_EDITOR
            if (logHapticsInEditor)
                Debug.Log($"[Tar&Tulla][Haptics] {debugLabel} strength={strength:F2}");
#elif UNITY_IOS || UNITY_ANDROID
            if (strength >= 0.45f)
                Handheld.Vibrate();
#endif
        }

        static bool IsHapticsEnabled()
        {
            var feedback = GetFeedback();
            return feedback == null || (feedback.enableFeedback && feedback.enableHaptics);
        }

        static float GetLandingStrength()
        {
            var feedback = GetFeedback();
            return feedback != null ? feedback.landingHapticStrength : DefaultLandingStrength;
        }

        static float GetRopeStretchStrength()
        {
            var feedback = GetFeedback();
            return feedback != null ? feedback.ropeStretchHapticStrength : DefaultRopeStretchStrength;
        }

        static float GetDangerStrength()
        {
            var feedback = GetFeedback();
            return feedback != null ? feedback.dangerHapticStrength : DefaultDangerStrength;
        }

        static TarTullaGameplayProfile.FeedbackTuning GetFeedback() =>
            TarTullaTuningAccess.GetActiveProfile()?.Feedback;
    }
}
