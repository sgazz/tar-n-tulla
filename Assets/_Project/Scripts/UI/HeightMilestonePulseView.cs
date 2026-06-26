using System.Collections;
using TarTulla.Core;
using TarTulla.Game;
using UnityEngine;
using UnityEngine.UI;

namespace TarTulla.UI
{
    public class HeightMilestonePulseView : MonoBehaviour
    {
        const float PulseDuration = 0.55f;
        const float DefaultMilestoneInterval = 10f;

        [SerializeField] Text heightText;
        [SerializeField] Text bestText;
        [SerializeField] Text pulseText;
        [SerializeField] CanvasGroup pulseGroup;

        Coroutine pulseRoutine;
        Vector3 heightBaseScale = Vector3.one;
        Vector3 bestBaseScale = Vector3.one;

        void Awake()
        {
            if (heightText != null)
                heightBaseScale = heightText.transform.localScale;
            if (bestText != null)
                bestBaseScale = bestText.transform.localScale;

            if (pulseText != null)
                pulseText.gameObject.SetActive(false);

            if (pulseGroup != null)
                pulseGroup.alpha = 0f;
        }

        void OnEnable()
        {
            GameplayFeedbackEvents.HeightMilestone += HandleHeightMilestone;
            GameplayFeedbackEvents.NewBestHeight += HandleNewBestHeight;
        }

        void OnDisable()
        {
            GameplayFeedbackEvents.HeightMilestone -= HandleHeightMilestone;
            GameplayFeedbackEvents.NewBestHeight -= HandleNewBestHeight;

            if (pulseRoutine != null)
            {
                StopCoroutine(pulseRoutine);
                pulseRoutine = null;
            }
        }

        public void Bind(Text height, Text best, Text pulse = null, CanvasGroup group = null)
        {
            heightText = height;
            bestText = best;
            pulseText = pulse;
            pulseGroup = group;
        }

        void HandleHeightMilestone(float height)
        {
            if (!IsMilestonePulseEnabled())
                return;

            float interval = GetMilestoneInterval();
            ShowPulse($"+{Mathf.RoundToInt(interval)}m", heightText);
        }

        void HandleNewBestHeight(float height)
        {
            if (!IsMilestonePulseEnabled())
                return;

            ShowPulse("New Best!", bestText);
        }

        void ShowPulse(string message, Text targetText)
        {
            if (pulseRoutine != null)
                StopCoroutine(pulseRoutine);

            pulseRoutine = StartCoroutine(PulseRoutine(message, targetText));
        }

        IEnumerator PulseRoutine(string message, Text targetText)
        {
            if (pulseText != null)
            {
                pulseText.text = message;
                pulseText.gameObject.SetActive(true);
            }

            if (pulseGroup != null)
                pulseGroup.alpha = 1f;

            float elapsed = 0f;
            while (elapsed < PulseDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / PulseDuration;
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.12f;

                if (targetText != null)
                    targetText.transform.localScale = (targetText == heightText ? heightBaseScale : bestBaseScale) * scale;

                if (pulseGroup != null)
                    pulseGroup.alpha = 1f - t;

                yield return null;
            }

            if (heightText != null)
                heightText.transform.localScale = heightBaseScale;
            if (bestText != null)
                bestText.transform.localScale = bestBaseScale;

            if (pulseText != null)
                pulseText.gameObject.SetActive(false);

            if (pulseGroup != null)
                pulseGroup.alpha = 0f;

            pulseRoutine = null;
        }

        static bool IsMilestonePulseEnabled()
        {
            var feedback = GetFeedback();
            return feedback == null || (feedback.enableFeedback && feedback.enableHeightMilestonePulse);
        }

        static float GetMilestoneInterval()
        {
            var feedback = GetFeedback();
            return feedback != null ? feedback.heightMilestoneInterval : DefaultMilestoneInterval;
        }

        static TarTullaGameplayProfile.FeedbackTuning GetFeedback() =>
            TarTullaTuningAccess.GetActiveProfile()?.Feedback;
    }
}
