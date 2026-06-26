using UnityEngine;
using UnityEngine.UI;

namespace TarTulla.UI
{
    public static class UIStyleDefaults
    {
        public static readonly Color PanelBackground = new(0.08f, 0.1f, 0.14f, 0.92f);
        public static readonly Color OverlayDim = new(0f, 0f, 0f, 0.65f);
        public static readonly Color HudStripBackground = new(0f, 0f, 0f, 0.35f);
        public static readonly Color ButtonNormal = new(0.22f, 0.45f, 0.72f, 1f);
        public static readonly Color ButtonHighlighted = new(0.32f, 0.55f, 0.82f, 1f);
        public static readonly Color ButtonPressed = new(0.16f, 0.34f, 0.58f, 1f);
        public static readonly Color ButtonDisabled = new(0.35f, 0.35f, 0.38f, 0.7f);
        public static readonly Color SubtitleText = new(0.82f, 0.86f, 0.9f, 1f);

        public const float ButtonMinHeight = 80f;
        public const float ButtonSpacing = 0.1f;
        public const int TitleFontSize = 96;
        public const int SubtitleFontSize = 34;
        public const int HudHeightFontSize = 44;
        public const int HudBestFontSize = 34;
        public const int ButtonFontSize = 40;
        public const int SectionLabelFontSize = 72;
        public const int GameOverStatFontSize = 48;

        static TarTullaUITheme fallbackTheme;

        public static TarTullaUITheme FallbackTheme
        {
            get
            {
                if (fallbackTheme != null)
                    return fallbackTheme;

                fallbackTheme = ScriptableObject.CreateInstance<TarTullaUITheme>();
                return fallbackTheme;
            }
        }

        public static void ApplyButtonStyle(Button button)
        {
            if (button == null)
                return;

            var image = button.GetComponent<Image>();
            if (image != null)
                image.color = ButtonNormal;

            var colors = button.colors;
            colors.normalColor = ButtonNormal;
            colors.highlightedColor = ButtonHighlighted;
            colors.pressedColor = ButtonPressed;
            colors.disabledColor = ButtonDisabled;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
        }
    }
}
