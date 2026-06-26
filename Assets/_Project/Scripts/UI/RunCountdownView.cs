using System;
using System.Collections;
using TarTulla.Game;
using UnityEngine;
using UnityEngine.UI;

namespace TarTulla.UI
{
    public class RunCountdownView : MonoBehaviour
    {
        static readonly string[] DefaultSteps = { "Ready", "3", "2", "1", "Climb!" };

        [SerializeField] Text countdownText;
        [SerializeField] CanvasGroup canvasGroup;

        Coroutine countdownRoutine;

        public event Action CountdownFinished;

        void Awake()
        {
            canvasGroup ??= GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            if (countdownText != null)
                countdownText.gameObject.SetActive(false);
        }

        public void Bind(Text text, CanvasGroup group = null)
        {
            countdownText = text;
            canvasGroup = group ?? canvasGroup;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
            if (!visible)
                StopCountdown();
        }

        public void PlayCountdown(Action onComplete = null)
        {
            StopCountdown();
            countdownRoutine = StartCoroutine(CountdownRoutine(onComplete));
        }

        public void StopCountdown()
        {
            if (countdownRoutine != null)
            {
                StopCoroutine(countdownRoutine);
                countdownRoutine = null;
            }

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            if (countdownText != null)
                countdownText.gameObject.SetActive(false);
        }

        IEnumerator CountdownRoutine(Action onComplete)
        {
            if (countdownText == null)
            {
                onComplete?.Invoke();
                CountdownFinished?.Invoke();
                yield break;
            }

            float stepDuration = GetStepDuration();
            countdownText.gameObject.SetActive(true);

            foreach (string step in DefaultSteps)
            {
                countdownText.text = step;
                if (canvasGroup != null)
                    canvasGroup.alpha = 1f;

                yield return FadeStep(stepDuration);
            }

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            countdownText.gameObject.SetActive(false);
            countdownRoutine = null;
            CountdownFinished?.Invoke();
            onComplete?.Invoke();
        }

        IEnumerator FadeStep(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                if (canvasGroup != null)
                {
                    float t = elapsed / duration;
                    canvasGroup.alpha = t < 0.15f ? t / 0.15f : t > 0.85f ? (1f - t) / 0.15f : 1f;
                }

                yield return null;
            }
        }

        static float GetStepDuration()
        {
            var onboarding = TarTullaTuningAccess.GetActiveProfile()?.Onboarding;
            return onboarding != null ? onboarding.countdownStepDuration : 0.65f;
        }
    }
}
