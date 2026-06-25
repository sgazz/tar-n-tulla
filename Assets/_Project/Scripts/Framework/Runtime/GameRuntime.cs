using System;
using GameplayFramework.Diagnostics;
using GameplayFramework.Tuning;
using UnityEngine;

namespace GameplayFramework.Runtime
{
    public abstract class GameRuntime : MonoBehaviour
    {
        [SerializeField] protected GameplayProfileBase startupProfile;

        public GameplayProfileBase StartupProfile => startupProfile;

        public event Action<GameplayProfileBase> ProfileChanged;

        public GameplayProfileBase ActiveProfile { get; private set; }
        public bool IsReady => ActiveProfile != null;

        protected virtual void Awake()
        {
            if (startupProfile != null)
                ApplyProfile(startupProfile);
            else
                DebugTuningLogger.LogMissingStartupProfile(name);

            if (IsReady)
                DebugTuningLogger.LogActiveProfile(ActiveProfile);
        }

        public virtual void ApplyProfile(GameplayProfileBase profile)
        {
            if (profile == null)
            {
                DebugTuningLogger.LogProfileSwitchFailed(name, "Profile reference is null.");
                return;
            }

            ActiveProfile = profile;
            ValidateActiveProfile();
            ProfileChanged?.Invoke(ActiveProfile);
            DebugTuningLogger.LogProfileApplied(ActiveProfile);
        }

        public bool ValidateActiveProfile()
        {
            if (ActiveProfile == null)
            {
                DebugTuningLogger.LogValidationFailed(name, "No active profile.");
                return false;
            }

            bool isValid = ActiveProfile.ValidateProfile();
            if (!isValid)
                DebugTuningLogger.LogValidationFailed(ActiveProfile.name, "Profile validation returned false.");

            return isValid;
        }
    }
}
