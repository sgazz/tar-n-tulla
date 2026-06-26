using TarTulla.Game;
using UnityEngine;

namespace TarTulla.Core
{
    public class RunStatsTracker : MonoBehaviour
    {
        [SerializeField] ClimbProgressTracker progressTracker;

        int landingCount;
        int ropeSaveCount;
        float bestHeightAtRunStart;

        public int LandingCount => landingCount;
        public int RopeSaveCount => ropeSaveCount;

        void Awake()
        {
            progressTracker ??= FindAnyObjectByType<ClimbProgressTracker>();
        }

        void OnEnable()
        {
            GameplayFeedbackEvents.JumperLanded += HandleJumperLanded;
            GameplayFeedbackEvents.PullAssistTriggered += HandlePullAssistTriggered;
        }

        void OnDisable()
        {
            GameplayFeedbackEvents.JumperLanded -= HandleJumperLanded;
            GameplayFeedbackEvents.PullAssistTriggered -= HandlePullAssistTriggered;
        }

        public void ResetForRun()
        {
            landingCount = 0;
            ropeSaveCount = 0;
            bestHeightAtRunStart = progressTracker != null ? progressTracker.BestHeight : 0f;
        }

        public RunSummary BuildSummary()
        {
            float height = progressTracker != null ? progressTracker.CurrentHeight : 0f;
            float best = progressTracker != null ? progressTracker.BestHeight : 0f;

            return new RunSummary
            {
                Height = height,
                BestHeight = best,
                IsNewBest = height > bestHeightAtRunStart + 0.001f && height >= best - 0.001f,
                Landings = landingCount,
                RopeSaves = ropeSaveCount
            };
        }

        void HandleJumperLanded(string _) => landingCount++;

        void HandlePullAssistTriggered(float _) => ropeSaveCount++;
    }
}
