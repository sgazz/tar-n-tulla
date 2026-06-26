using TarTulla.Core;
using TarTulla.Game;
using TarTulla.Input;
using TarTulla.Rope;
using UnityEngine;
using UnityEngine.UI;

namespace TarTulla.UI
{
    public class DeveloperHUDView : MonoBehaviour
    {
        [SerializeField] bool showDeveloperHUD;
        [SerializeField] Text debugText;
        [SerializeField] ClimbProgressTracker progressTracker;
        [SerializeField] PrototypeLevelBuilder levelBuilder;
        [SerializeField] MobileTiltInput2D tiltInput;
        [SerializeField] ElasticRope2D rope;

        UIManager uiManager;
        float nextRefreshTime;

        public bool ShowDeveloperHUD
        {
            get => showDeveloperHUD;
            set
            {
                showDeveloperHUD = value;
                ApplyVisibility();
            }
        }

        void Awake()
        {
            ApplyVisibility();
        }

        public void BindUiManager(UIManager manager) => uiManager = manager;

        public void Bind(
            Text text,
            ClimbProgressTracker tracker,
            PrototypeLevelBuilder builder,
            MobileTiltInput2D tilt,
            ElasticRope2D elasticRope)
        {
            debugText = text;
            progressTracker = tracker;
            levelBuilder = builder;
            tiltInput = tilt;
            rope = elasticRope;
            ApplyVisibility();
        }

        public void SetVisible(bool visible)
        {
            if (!ShouldShow())
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(visible);
        }

        void Update()
        {
            if (!ShouldShow() || !gameObject.activeInHierarchy || debugText == null)
                return;

            if (Time.unscaledTime < nextRefreshTime)
                return;

            nextRefreshTime = Time.unscaledTime + 0.15f;
            RefreshText();
        }

        void RefreshText()
        {
            var profile = TarTullaTuningAccess.GetActiveProfile();
            string profileLabel = profile != null ? profile.DisplayName : "none";
            float height = progressTracker != null ? progressTracker.CurrentHeight : 0f;
            int platforms = levelBuilder != null ? levelBuilder.ActivePlatformCount : 0;

            float jumpForce = profile?.Character.jumpForce ?? 0f;
            float gravityScale = profile?.Character.gravityScale ?? 0f;
            float restLength = profile?.Rope.restLength ?? 0f;
            float springStrength = profile?.Rope.springStrength ?? 0f;
            float pullAssist = profile?.Rope.pullAssistStrength ?? 0f;
            float tiltSensitivity = profile?.Tilt.tiltSensitivity ?? 0f;
            float tiltInputValue = tiltInput != null ? tiltInput.HorizontalInput : 0f;

            debugText.text =
                $"DEV HUD\n" +
                $"Profile: {profileLabel}\n" +
                $"Height: {Mathf.RoundToInt(height)}m\n" +
                $"Platforms: {platforms}\n" +
                $"tilt: {tiltInputValue:F2}\n" +
                $"jumpForce: {jumpForce:F1}\n" +
                $"gravityScale: {gravityScale:F1}\n" +
                $"restLength: {restLength:F1}\n" +
                $"springStrength: {springStrength:F0}\n" +
                $"pullAssist: {pullAssist:F0}\n" +
                $"tiltSensitivity: {tiltSensitivity:F1}";
        }

        void ApplyVisibility()
        {
            gameObject.SetActive(ShouldShow());
        }

        bool ShouldShow()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return showDeveloperHUD;
#else
            return false;
#endif
        }
    }
}
