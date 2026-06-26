using UnityEngine;
using UnityEngine.UI;

namespace TarTulla.UI
{
    [CreateAssetMenu(fileName = "TarTulla_UITheme", menuName = "Tar&Tulla/UI Theme")]
    public class TarTullaUITheme : ScriptableObject
    {
        [Header("Pozadina i paneli")]
            [Tooltip("Boja pozadine kamere (gameplay svet). Ne koristi se kao UI overlay preko jumpera.")]
            public Color backgroundColor = new(0.07f, 0.08f, 0.11f, 1f);

        [Tooltip("Boja panela (meni, pauza, game over).")]
        public Color panelColor = new(0.1f, 0.12f, 0.16f, 0.94f);

        [Header("Main Menu pozadina")]
        [Tooltip("Key art samo za Main Menu ekran. Ne koristi se kao gameplay pozadina.")]
        public Sprite mainMenuKeyArt;

        [Tooltip("Boja ivice panela. Zaobljeni uglovi panela — buduća podrška.")]
        public Color panelBorderColor = new(0.28f, 0.32f, 0.38f, 0.5f);

        [Tooltip("Zaobljeni uglovi panela u pikselima. Još nije vizuelno primenjeno — rezerva za kasnije.")]
        public float panelCornerRadius;

        [Header("Tekst")]
        [Tooltip("Primarna boja teksta (naslovi, HUD visina).")]
        public Color primaryTextColor = new(0.95f, 0.96f, 0.98f, 1f);

        [Tooltip("Sekundarna boja teksta (podnaslovi, Best, pomoćni tekst).")]
        public Color secondaryTextColor = new(0.72f, 0.76f, 0.82f, 1f);

        [Header("Tar i Tulla akcenti")]
        [Tooltip("Akcentna boja za Tar (hladna plava/cijan).")]
        public Color accentColorTar = new(0.35f, 0.75f, 0.95f, 1f);

        [Tooltip("Akcentna boja za Tulla (topla narandžasta).")]
        public Color accentColorTulla = new(0.95f, 0.55f, 0.35f, 1f);

        [Tooltip("Boja konopca u UI motivima (krem / blago žuta).")]
        public Color ropeColor = new(0.88f, 0.8f, 0.55f, 1f);

        [Tooltip("Boja opasnosti / pada (prigušena crvena).")]
        public Color dangerColor = new(0.82f, 0.38f, 0.35f, 1f);

        [Header("Dugmad")]
        [Tooltip("Normalna boja primarnog dugmeta.")]
        public Color buttonColor = new(0.22f, 0.48f, 0.72f, 1f);

        [Tooltip("Boja dugmeta kada je pritisnuto.")]
        public Color buttonPressedColor = new(0.14f, 0.32f, 0.52f, 1f);

        [Tooltip("Sekundarno dugme (npr. Main Menu na game over).")]
        public Color buttonSecondaryColor = new(0.16f, 0.18f, 0.22f, 0.92f);

        [Tooltip("Boja teksta na dugmetu.")]
        public Color buttonTextColor = new(0.96f, 0.97f, 0.99f, 1f);

        [Header("HUD")]
        [Tooltip("Pozadina HUD trake (niska providnost).")]
        public Color hudStripColor = new(0f, 0f, 0f, 0.38f);

        [Tooltip("Boja sitne vertikalne niti / strelice pored visine.")]
        public Color hudClimbHintColor = new(0.88f, 0.8f, 0.55f, 0.85f);

        [Tooltip("Pozadina fullscreen dim sloja.")]
        public Color overlayDimColor = new(0f, 0f, 0f, 0.62f);

        [Header("Developer HUD")]
        [Tooltip("Pozadina dev HUD-a (odvojena od igračkog UI-ja).")]
        public Color developerHudBackground = new(0.05f, 0.06f, 0.08f, 0.55f);

        [Tooltip("Tekst dev HUD-a.")]
        public Color developerHudTextColor = new(0.55f, 0.9f, 0.65f, 1f);

        [Header("Veličine fonta")]
        [Tooltip("Veličina naslova (Tar&Tulla).")]
        public int defaultTitleSize = 96;

        [Tooltip("Veličina sekcijskog naslova (Paused, Run Over).")]
        public int defaultHeaderSize = 72;

        [Tooltip("Veličina osnovnog teksta.")]
        public int defaultBodySize = 36;

        [Tooltip("Veličina teksta na dugmetu.")]
        public int defaultButtonTextSize = 40;

        public void ApplyButtonColors(Button button, bool primary = true)
        {
            if (button == null)
                return;

            var image = button.GetComponent<Image>();
            Color normal = primary ? buttonColor : buttonSecondaryColor;
            if (image != null)
                image.color = normal;

            var colors = button.colors;
            colors.normalColor = normal;
            colors.highlightedColor = Color.Lerp(normal, Color.white, 0.12f);
            colors.pressedColor = primary ? buttonPressedColor : Color.Lerp(buttonSecondaryColor, Color.black, 0.15f);
            colors.disabledColor = new Color(0.35f, 0.35f, 0.38f, 0.7f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            var label = button.GetComponentInChildren<Text>();
            if (label != null)
                label.color = buttonTextColor;
        }
    }
}
