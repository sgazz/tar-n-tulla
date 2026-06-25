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

        Transform tar;
        Transform tulla;
        bool runActive;
        bool resetPending;
        Coroutine resetRoutine;

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
            StartRun();
        }

        void Update()
        {
            if (!runActive)
                return;

#if UNITY_EDITOR
            if (PrototypeKeyboardInput.WasResetRunPressed())
                StartRun();

            if (PrototypeKeyboardInput.WasRebuildLayoutPressed())
                RebuildLayout();
#endif

            if (tar == null || tulla == null || progressTracker == null)
                return;

            progressTracker.Tick();

            if (!resetPending && HasFailed())
                BeginResetRun();
        }

        public void StartRun()
        {
            if (resetRoutine != null)
            {
                StopCoroutine(resetRoutine);
                resetRoutine = null;
            }

            resetPending = false;
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

            runActive = true;
            Debug.Log("[Tar&Tulla] Run started");
        }

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

        void BeginResetRun()
        {
            resetPending = true;
            resetRoutine = StartCoroutine(ResetRunAfterDelay());
        }

        IEnumerator ResetRunAfterDelay()
        {
            if (ResetDelay > 0f)
                yield return new WaitForSeconds(ResetDelay);

            Debug.Log("[Tar&Tulla] Run reset: fell too far");
            StartRun();
        }

        bool HasFailed()
        {
            float failLine = progressTracker.HighestReachedY - FallDistanceLimit;
            return tar.position.y < failLine && tulla.position.y < failLine;
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
            levelBuilder ??= FindFirstObjectByType<PrototypeLevelBuilder>();
            progressTracker ??= FindFirstObjectByType<ClimbProgressTracker>();
            cameraFollow ??= FindFirstObjectByType<VerticalCameraFollow2D>();
        }
    }
}
