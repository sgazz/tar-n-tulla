using System.Collections;
using TarTulla.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TarTulla.UI
{
    public class SavedFeedbackView : MonoBehaviour
    {
        const float DisplayDuration = 0.85f;
        const float Cooldown = 0.35f;

        [SerializeField] Text savedText;
        [SerializeField] CanvasGroup canvasGroup;

        Coroutine displayRoutine;
        float lastShownTime = -999f;

        void Awake()
        {
            canvasGroup ??= GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            if (savedText != null)
                savedText.gameObject.SetActive(false);
        }

        public void Bind(Text text, CanvasGroup group = null)
        {
            savedText = text;
            canvasGroup = group ?? canvasGroup;
        }

        void OnEnable() =>
            GameplayFeedbackEvents.PullAssistTriggered += HandlePullAssistTriggered;

        void OnDisable()
        {
            GameplayFeedbackEvents.PullAssistTriggered -= HandlePullAssistTriggered;
            StopDisplay();
        }

        void HandlePullAssistTriggered(float _)
        {
            if (Time.unscaledTime - lastShownTime < Cooldown)
                return;

            lastShownTime = Time.unscaledTime;
            if (displayRoutine != null)
                StopCoroutine(displayRoutine);

            displayRoutine = StartCoroutine(ShowSavedRoutine());
        }

        IEnumerator ShowSavedRoutine()
        {
            if (savedText != null)
            {
                savedText.text = "Saved!";
                savedText.gameObject.SetActive(true);
            }

            float elapsed = 0f;
            while (elapsed < DisplayDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                if (canvasGroup != null)
                {
                    float t = elapsed / DisplayDuration;
                    canvasGroup.alpha = t < 0.15f ? t / 0.15f : t > 0.75f ? (1f - t) / 0.25f : 1f;
                }

                yield return null;
            }

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            if (savedText != null)
                savedText.gameObject.SetActive(false);

            displayRoutine = null;
        }

        void StopDisplay()
        {
            if (displayRoutine != null)
            {
                StopCoroutine(displayRoutine);
                displayRoutine = null;
            }
        }
    }
}
