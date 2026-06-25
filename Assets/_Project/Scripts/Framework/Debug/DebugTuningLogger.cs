using GameplayFramework.Tuning;
using UnityEngine;

namespace GameplayFramework.Diagnostics
{
    public static class DebugTuningLogger
    {
        const string Prefix = "[Tuning]";

        public static void LogActiveProfile(GameplayProfileBase profile)
        {
            Debug.Log($"{Prefix} Active profile: {profile.DisplayName} ({profile.ProfileId}) v{profile.Version}");
        }

        public static void LogProfileApplied(GameplayProfileBase profile)
        {
            Debug.Log($"{Prefix} Profile applied: {profile.DisplayName} ({profile.ProfileId})");
        }

        public static void LogProfileSwitchFailed(string runtimeName, string reason)
        {
            Debug.LogWarning($"{Prefix} {runtimeName}: profile switch failed — {reason}");
        }

        public static void LogMissingStartupProfile(string runtimeName)
        {
            Debug.LogWarning($"{Prefix} {runtimeName}: no startup profile assigned.");
        }

        public static void LogValidationFailed(string profileName, string reason)
        {
            Debug.LogWarning($"{Prefix} Validation failed for '{profileName}': {reason}");
        }

        public static void LogValidationWarning(GameplayProfileBase profile, string reason)
        {
            Debug.LogWarning($"{Prefix} {profile.DisplayName}: {reason}", profile);
        }
    }
}
