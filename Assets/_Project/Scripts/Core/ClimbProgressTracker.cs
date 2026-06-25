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
            if (runHeight > bestHeight)
                bestHeight = runHeight;
        }

        public void ResetProgress()
        {
            highestReachedY = GetMidpointY();
            if (highestReachedY < runBaselineY)
                highestReachedY = runBaselineY;
        }

        float GetMidpointY()
        {
            if (targetA == null || targetB == null)
                return runBaselineY;

            return (targetA.position.y + targetB.position.y) * 0.5f;
        }
    }
}
