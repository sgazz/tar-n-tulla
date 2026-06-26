using System.Collections;
using System.Collections.Generic;
using TarTulla.Game;
using UnityEngine;
using UnityEngine.UI;

namespace TarTulla.UI
{
    public class TutorialHintView : MonoBehaviour
    {
        public const string TutorialSeenPlayerPrefsKey = "TarTulla_TutorialSeen";

        static readonly string[] DefaultHints =
        {
            "Tilt your phone",
            "Land on platforms",
            "Use the rope to save your partner",
            "Keep climbing"
        };

        [SerializeField] Text hintText;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] bool showHintsOnFirstRun = true;

        Coroutine hintRoutine;

        void Awake()
        {
            canvasGroup ??= GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        public void Bind(Text text, CanvasGroup group = null)
        {
            hintText = text;
            canvasGroup = group ?? canvasGroup;
        }

        public void StopHints()
        {
            if (hintRoutine != null)
            {
                StopCoroutine(hintRoutine);
                hintRoutine = null;
            }

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            if (hintText != null)
                hintText.gameObject.SetActive(false);
        }

        public void TryShowFirstRunHints()
        {
            if (!ShouldShowHints())
                return;

            StopHints();
            hintRoutine = StartCoroutine(ShowHintsRoutine());
        }

        bool ShouldShowHints()
        {
            if (!showHintsOnFirstRun)
                return false;

            var onboarding = GetOnboarding();
            if (onboarding != null && !onboarding.showTutorialHints)
                return false;

            if (onboarding != null && onboarding.rememberTutorialSeen
                && PlayerPrefs.GetInt(TutorialSeenPlayerPrefsKey, 0) == 1)
                return false;

            return true;
        }

        IEnumerator ShowHintsRoutine()
        {
            float hintDuration = GetHintDuration();
            IReadOnlyList<string> hints = DefaultHints;

            foreach (string hint in hints)
            {
                if (hintText != null)
                {
                    hintText.text = hint;
                    hintText.gameObject.SetActive(true);
                }

                yield return FadeHint(hintDuration);
            }

            MarkTutorialSeen();
            StopHints();
            hintRoutine = null;
        }

        IEnumerator FadeHint(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                if (canvasGroup != null)
                {
                    float t = elapsed / duration;
                    canvasGroup.alpha = t < 0.2f ? t / 0.2f : t > 0.8f ? (1f - t) / 0.2f : 1f;
                }

                yield return null;
            }
        }

        void MarkTutorialSeen()
        {
            var onboarding = GetOnboarding();
            if (onboarding != null && onboarding.rememberTutorialSeen)
                PlayerPrefs.SetInt(TutorialSeenPlayerPrefsKey, 1);
        }

        [ContextMenu("Reset Tutorial Seen")]
        void ResetTutorialSeenMenu() => ResetTutorialSeen();

        public static void ResetTutorialSeen()
        {
            PlayerPrefs.DeleteKey(TutorialSeenPlayerPrefsKey);
            Debug.Log("[Tar&Tulla] Tutorial seen flag reset.");
        }

        static TarTullaGameplayProfile.OnboardingTuning GetOnboarding() =>
            TarTullaTuningAccess.GetActiveProfile()?.Onboarding;

        static float GetHintDuration()
        {
            var onboarding = GetOnboarding();
            return onboarding != null ? onboarding.hintDuration : 2.5f;
        }
    }
}
