using GameplayFramework.Diagnostics;
using GameplayFramework.Runtime;
using GameplayFramework.Tuning;
using UnityEngine;

namespace TarTulla.Game
{
    [DefaultExecutionOrder(-99)]
    public class TarTullaRuntime : GameRuntime
    {
        public static TarTullaRuntime Instance { get; private set; }

        [SerializeField] bool logProfileSnapshotOnAwake = true;

        public TarTullaGameplayProfile Profile => ActiveProfile as TarTullaGameplayProfile;
        public TarTullaGameplayProfile.CharacterTuning Character => Profile?.Character;
        public TarTullaGameplayProfile.RopeTuning Rope => Profile?.Rope;
        public TarTullaGameplayProfile.TiltTuning Tilt => Profile?.Tilt;
        public TarTullaGameplayProfile.CameraTuning Camera => Profile?.Camera;
        public TarTullaGameplayProfile.PlatformTuning Platforms => Profile?.Platforms;
        public TarTullaGameplayProfile.RunRulesTuning RunRules => Profile?.RunRules;

        protected override void Awake()
        {
            if (Instance != null && Instance != this)
                Debug.LogWarning("[Tar&Tulla][Runtime] Duplicate TarTullaRuntime detected. Using latest instance.", this);

            Instance = this;
            base.Awake();

            if (!IsReady && StartupProfile is TarTullaGameplayProfile startup)
                ApplyProfile(startup);

            if (logProfileSnapshotOnAwake)
                LogProfileSnapshot();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public override void ApplyProfile(GameplayProfileBase profile)
        {
            if (profile != null && profile is not TarTullaGameplayProfile)
            {
                DebugTuningLogger.LogValidationWarning(profile, "Expected TarTullaGameplayProfile.");
                return;
            }

            base.ApplyProfile(profile);

            if (logProfileSnapshotOnAwake)
                LogProfileSnapshot();
        }

        [ContextMenu("Reapply Active Profile")]
        void ReapplyActiveProfile()
        {
            if (StartupProfile != null)
                ApplyProfile(StartupProfile);
            else if (Profile != null)
                ApplyProfile(Profile);
        }

        public void LogProfileSnapshot()
        {
            if (Profile == null)
            {
                Debug.LogWarning("[Tar&Tulla][Runtime] No active TarTullaGameplayProfile assigned.", this);
                return;
            }

            var c = Profile.Character;
            var r = Profile.Rope;
            var t = Profile.Tilt;
            var cam = Profile.Camera;
            var p = Profile.Platforms;
            var rules = Profile.RunRules;

            Debug.Log(
                $"[Tar&Tulla][Runtime] Active profile: {Profile.name} ({Profile.DisplayName})",
                this);
            Debug.Log(
                $"[Tar&Tulla][Runtime] jumpForce={c.jumpForce}, gravityScale={c.gravityScale}, " +
                $"ropeRestLength={r.restLength}, springStrength={r.springStrength}, pullAssistStrength={r.pullAssistStrength}, " +
                $"tiltSensitivity={t.tiltSensitivity}, cameraSmoothTime={cam.smoothTime}, " +
                $"platformCount={p.platformCount}, fallDistanceLimit={rules.fallDistanceLimit}",
                this);
        }
    }
}
