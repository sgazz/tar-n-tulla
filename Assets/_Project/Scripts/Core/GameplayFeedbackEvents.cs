using System;

namespace TarTulla.Core
{
    /// <summary>
    /// Lightweight feedback-only event hub. Not a general gameplay event bus.
    /// </summary>
    public static class GameplayFeedbackEvents
    {
        public static event Action<string> JumperLanded;
        public static event Action<string> JumpImpulse;
        public static event Action<float> RopeOverstretched;
        public static event Action<float> PullAssistTriggered;
        public static event Action<float> DangerRatioChanged;
        public static event Action<float> NewBestHeight;
        public static event Action<float> HeightMilestone;

        public static void InvokeJumperLanded(string jumperName) =>
            JumperLanded?.Invoke(jumperName);

        public static void InvokeJumpImpulse(string jumperName) =>
            JumpImpulse?.Invoke(jumperName);

        public static void InvokeRopeOverstretched(float stretchRatio) =>
            RopeOverstretched?.Invoke(stretchRatio);

        public static void InvokePullAssistTriggered(float strength) =>
            PullAssistTriggered?.Invoke(strength);

        public static void InvokeDangerRatioChanged(float ratio) =>
            DangerRatioChanged?.Invoke(ratio);

        public static void InvokeNewBestHeight(float height) =>
            NewBestHeight?.Invoke(height);

        public static void InvokeHeightMilestone(float height) =>
            HeightMilestone?.Invoke(height);
    }
}
