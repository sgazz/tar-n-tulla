using UnityEngine;
using TarTulla.CameraSystems;
using TarTulla.Input;

namespace TarTulla.Core
{
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

            if (tar == null || tulla == null)
                return;

            progressTracker.Tick();

            if (HasFailed())
                ResetRun();
        }

        public void StartRun()
        {
            ResolveReferences();

            if (levelBuilder == null || progressTracker == null)
            {
                Debug.LogError("[Tar&Tulla] PrototypeRunController: Missing required references.");
                return;
            }

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

            levelBuilder.BuildPrototypeLayout();
            CacheJumperTransforms();

            float baselineY = levelBuilder.StartBaselineY;
            progressTracker.SetTargets(tar, tulla, baselineY);
            progressTracker.ResetProgress();
            cameraFollow?.ResetToTargets();
        }

        void ResetRun()
        {
            Debug.Log("[Tar&Tulla] Run reset: fell too far");
            StartRun();
        }

        bool HasFailed()
        {
            float failLine = progressTracker.HighestReachedY - fallDistanceLimit;
            return tar.position.y < failLine && tulla.position.y < failLine;
        }

        void CacheJumperTransforms()
        {
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
