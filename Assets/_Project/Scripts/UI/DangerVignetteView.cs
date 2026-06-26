using TarTulla.Core;
using TarTulla.Game;
using UnityEngine;
using UnityEngine.UI;

namespace TarTulla.UI
{
    [RequireComponent(typeof(Image))]
    public class DangerVignetteView : MonoBehaviour
    {
        const float DefaultStartRatio = 0.65f;
        const float DefaultMaxAlpha = 0.35f;

        [SerializeField] Image overlayImage;

        float currentAlpha;

        void Awake()
        {
            overlayImage ??= GetComponent<Image>();
            if (overlayImage == null)
                return;

            overlayImage.raycastTarget = false;
            overlayImage.color = new Color(0.45f, 0.02f, 0.02f, 0f);
        }

        void OnEnable() =>
            GameplayFeedbackEvents.DangerRatioChanged += HandleDangerRatioChanged;

        void OnDisable()
        {
            GameplayFeedbackEvents.DangerRatioChanged -= HandleDangerRatioChanged;
            SetAlpha(0f);
        }

        void HandleDangerRatioChanged(float ratio)
        {
            if (!IsVignetteEnabled())
            {
                SetAlpha(0f);
                return;
            }

            float startRatio = GetStartRatio();
            float maxAlpha = GetMaxAlpha();
            float alpha = ratio <= startRatio
                ? 0f
                : Mathf.Lerp(0f, maxAlpha, Mathf.InverseLerp(startRatio, 1f, ratio));

            SetAlpha(alpha);
        }

        void SetAlpha(float alpha)
        {
            currentAlpha = alpha;
            if (overlayImage == null)
                return;

            var color = overlayImage.color;
            color.a = alpha;
            overlayImage.color = color;
        }

        static bool IsVignetteEnabled()
        {
            var feedback = GetFeedback();
            return feedback == null || (feedback.enableFeedback && feedback.enableScreenDangerVignette);
        }

        static float GetStartRatio()
        {
            var feedback = GetFeedback();
            return feedback != null ? feedback.dangerStartRatio : DefaultStartRatio;
        }

        static float GetMaxAlpha()
        {
            var feedback = GetFeedback();
            return feedback != null ? feedback.dangerMaxAlpha : DefaultMaxAlpha;
        }

        static TarTullaGameplayProfile.FeedbackTuning GetFeedback() =>
            TarTullaTuningAccess.GetActiveProfile()?.Feedback;
    }
}
