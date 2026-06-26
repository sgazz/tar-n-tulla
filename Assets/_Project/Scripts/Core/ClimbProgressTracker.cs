using TarTulla.Game;
using UnityEngine;

namespace TarTulla.Core
{
    public class ClimbProgressTracker : MonoBehaviour
    {
        [SerializeField] Transform targetA;
        [SerializeField] Transform targetB;
        [SerializeField] float runBaselineY;

        float highestReachedY;
        float bestHeight;
        int lastMilestoneIndex;

        public float CurrentHeight => Mathf.Max(0f, highestReachedY - runBaselineY);
        public float BestHeight => bestHeight;
        public float HighestReachedY => highestReachedY;

        public void SetTargets(Transform a, Transform b, float baselineY)
        {
            targetA = a;
            targetB = b;
            runBaselineY = baselineY;
        }

        public void Tick()
        {
            if (targetA == null || targetB == null)
                return;

            float midpointY = (targetA.position.y + targetB.position.y) * 0.5f;

            if (midpointY > highestReachedY)
                highestReachedY = midpointY;

            float runHeight = CurrentHeight;
            float previousBest = bestHeight;
            if (runHeight > bestHeight)
                bestHeight = runHeight;

            if (runHeight > previousBest + 0.001f && runHeight > 0.01f)
                GameplayFeedbackEvents.InvokeNewBestHeight(runHeight);

            TryInvokeHeightMilestone(runHeight);
        }

        void TryInvokeHeightMilestone(float runHeight)
        {
            var feedback = TarTullaTuningAccess.GetActiveProfile()?.Feedback;
            if (feedback != null && (!feedback.enableFeedback || !feedback.enableHeightMilestonePulse))
                return;

            float interval = feedback != null ? feedback.heightMilestoneInterval : 10f;
            if (interval <= 0f || runHeight < interval)
                return;

            int milestoneIndex = Mathf.FloorToInt(runHeight / interval);
            if (milestoneIndex <= lastMilestoneIndex)
                return;

            lastMilestoneIndex = milestoneIndex;
            GameplayFeedbackEvents.InvokeHeightMilestone(milestoneIndex * interval);
        }

        public void ResetProgress()
        {
            highestReachedY = GetMidpointY();
            if (highestReachedY < runBaselineY)
                highestReachedY = runBaselineY;

            lastMilestoneIndex = 0;
        }

        float GetMidpointY()
        {
            if (targetA == null || targetB == null)
                return runBaselineY;

            return (targetA.position.y + targetB.position.y) * 0.5f;
        }
    }
}
