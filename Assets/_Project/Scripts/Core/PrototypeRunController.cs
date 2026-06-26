using System;
using System.Collections;
using UnityEngine;
using TarTulla.CameraSystems;
using TarTulla.Game;
using TarTulla.Input;

namespace TarTulla.Core
{
    [DefaultExecutionOrder(-50)]
    public class PrototypeRunController : MonoBehaviour
    {
        const string TarObjectName = "Tar";
        const string TullaObjectName = "Tulla";

        [SerializeField] PrototypeLevelBuilder levelBuilder;
        [SerializeField] ClimbProgressTracker progressTracker;
        [SerializeField] VerticalCameraFollow2D cameraFollow;
        [SerializeField] float fallDistanceLimit = 14f;
        [SerializeField] bool autoStartOnPlay;

        Transform tar;
        Transform tulla;
        bool runActive;
        bool runPaused;
        bool resetPending;
        Coroutine resetRoutine;
        float lastDangerRatio = -1f;

        const float DangerRatioChangeThreshold = 0.04f;

        public event Action<float, float> OnRunFailed;
        public bool IsRunActive => runActive;

        float FallDistanceLimit => GetRunRules().fallDistanceLimit;
        float ResetDelay => GetRunRules().resetDelay;

        RunRuleSettings GetRunRules()
        {
            var profile = TarTullaTuningAccess.GetActiveProfile();
            if (profile != null)
                return new RunRuleSettings
                {
                    fallDistanceLimit = profile.RunRules.fallDistanceLimit,
                    resetDelay = profile.RunRules.resetDelay
                };

            return new RunRuleSettings
            {
                fallDistanceLimit = fallDistanceLimit,
                resetDelay = 0f
            };
        }

        struct RunRuleSettings
        {
            public float fallDistanceLimit;
            public float resetDelay;
        }

        void Start()
        {
            ResolveReferences();

            if (autoStartOnPlay)
                StartRun();
        }

        void Update()
        {
            if (!runActive || runPaused)
                return;

#if UNITY_EDITOR
            if (PrototypeKeyboardInput.WasResetRunPressed())
                ResetRun();

            if (PrototypeKeyboardInput.WasRebuildLayoutPressed())
                RebuildLayout();
#endif

            if (tar == null || tulla == null || progressTracker == null)
                return;

            progressTracker.Tick();

            if (levelBuilder != null && levelBuilder.UsesProceduralGeneration)
            {
                float midpointY = (tar.position.y + tulla.position.y) * 0.5f;
                levelBuilder.EnsurePlatformsAhead(midpointY);

                if (cameraFollow != null)
                    levelBuilder.CleanupOldPlatforms(cameraFollow.transform.position.y);
            }

            if (!resetPending && HasFailed())
                BeginRunFailed();

            UpdateDangerFeedback();
        }

        public void StartRun()
        {
            PrepareRun();
            StartPreparedRun();
        }

        public void PrepareRun()
        {
            if (resetRoutine != null)
            {
                StopCoroutine(resetRoutine);
                resetRoutine = null;
            }

            resetPending = false;
            runPaused = true;
            ResolveReferences();

            if (levelBuilder == null || progressTracker == null)
            {
                Debug.LogError("[Tar&Tulla] PrototypeRunController: Missing required references.");
                return;
            }

            levelBuilder.ClearGeneratedContent();
            levelBuilder.BuildPrototypeLayout();
            CacheJumperTransforms();

            float baselineY = levelBuilder.StartBaselineY;
            progressTracker.SetTargets(tar, tulla, baselineY);
            progressTracker.ResetProgress();
            cameraFollow?.ResetToTargets();
            lastDangerRatio = -1f;
            GameplayFeedbackEvents.InvokeDangerRatioChanged(0f);

            runActive = true;
            Debug.Log("[Tar&Tulla] Run prepared");
        }

        public void StartPreparedRun()
        {
            if (!runActive)
                PrepareRun();

            runPaused = false;
            Debug.Log("[Tar&Tulla] Run started");
        }

        public void ResetRun() => StartRun();

        public void StopRun()
        {
            if (resetRoutine != null)
            {
                StopCoroutine(resetRoutine);
                resetRoutine = null;
            }

            resetPending = false;
            runPaused = false;
            runActive = false;
            levelBuilder?.ClearGeneratedContent();
            tar = null;
            tulla = null;
        }

        public void SetRunPaused(bool paused) => runPaused = paused;

        public void RebuildLayout()
        {
            if (levelBuilder == null)
                return;

            levelBuilder.ClearGeneratedContent();
            levelBuilder.BuildPrototypeLayout();
            CacheJumperTransforms();

            float baselineY = levelBuilder.StartBaselineY;
            progressTracker.SetTargets(tar, tulla, baselineY);
            progressTracker.ResetProgress();
            cameraFollow?.ResetToTargets();
        }

        void BeginRunFailed()
        {
            resetPending = true;
            runActive = false;

            float height = progressTracker != null ? progressTracker.CurrentHeight : 0f;
            float best = progressTracker != null ? progressTracker.BestHeight : 0f;
            OnRunFailed?.Invoke(height, best);

            if (ResetDelay > 0f)
                resetRoutine = StartCoroutine(ClearResetPendingAfterDelay());
            else
                resetPending = false;
        }

        IEnumerator ClearResetPendingAfterDelay()
        {
            yield return new WaitForSecondsRealtime(ResetDelay);
            resetPending = false;
            resetRoutine = null;
        }

        bool HasFailed()
        {
            float failLine = progressTracker.HighestReachedY - FallDistanceLimit;
            return tar.position.y < failLine && tulla.position.y < failLine;
        }

        void UpdateDangerFeedback()
        {
            var feedback = TarTullaTuningAccess.GetActiveProfile()?.Feedback;
            if (feedback != null && !feedback.enableFeedback)
            {
                if (lastDangerRatio > 0.001f)
                {
                    lastDangerRatio = 0f;
                    GameplayFeedbackEvents.InvokeDangerRatioChanged(0f);
                }

                return;
            }

            float failLine = progressTracker.HighestReachedY - FallDistanceLimit;
            float margin = Mathf.Min(tar.position.y - failLine, tulla.position.y - failLine);
            float ratio = 1f - Mathf.Clamp01(margin / FallDistanceLimit);

            if (Mathf.Abs(ratio - lastDangerRatio) < DangerRatioChangeThreshold)
                return;

            lastDangerRatio = ratio;
            GameplayFeedbackEvents.InvokeDangerRatioChanged(ratio);
        }

        void CacheJumperTransforms()
        {
            if (levelBuilder != null && levelBuilder.TarTransform != null && levelBuilder.TullaTransform != null)
            {
                tar = levelBuilder.TarTransform;
                tulla = levelBuilder.TullaTransform;
                return;
            }

            tar = FindJumperTransform(TarObjectName);
            tulla = FindJumperTransform(TullaObjectName);
        }

        static Transform FindJumperTransform(string jumperName)
        {
            var jumper = GameObject.Find(jumperName);
            return jumper != null ? jumper.transform : null;
        }

        void ResolveReferences()
        {
            levelBuilder ??= GetComponent<PrototypeLevelBuilder>();
            progressTracker ??= GetComponent<ClimbProgressTracker>();
            levelBuilder ??= FindAnyObjectByType<PrototypeLevelBuilder>();
            progressTracker ??= FindAnyObjectByType<ClimbProgressTracker>();
            cameraFollow ??= FindAnyObjectByType<VerticalCameraFollow2D>();
        }
    }
}
