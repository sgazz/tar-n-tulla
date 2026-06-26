using UnityEngine;
using UnityEngine.UI;

namespace TarTulla.UI
{
    public class UIThemeApplier : MonoBehaviour
    {
        [SerializeField] TarTullaUITheme theme;

        [Header("World")]
        [SerializeField] Camera gameplayCamera;

        [Header("Canvas")]
        [SerializeField] Image fullscreenBlocker;

        [Header("Main Menu")]
        [SerializeField] Image mainMenuPanel;
        [SerializeField] Image mainMenuBackgroundImage;
        [SerializeField] Text mainMenuTitle;
        [SerializeField] Text mainMenuSubtitle;
        [SerializeField] Button mainMenuStartButton;
        [SerializeField] Image tarMotifDot;
        [SerializeField] Image tullaMotifDot;
        [SerializeField] RopeLineUI mainMenuRope;

        [Header("HUD")]
        [SerializeField] Image hudStrip;
        [SerializeField] Text hudHeightText;
        [SerializeField] Text hudBestText;
        [SerializeField] Image hudClimbHint;
        [SerializeField] Button hudPauseButton;

        [Header("Pause")]
        [SerializeField] Image pausePanel;
        [SerializeField] Text pauseTitle;
        [SerializeField] Button pauseResumeButton;
        [SerializeField] Button pauseRestartButton;
        [SerializeField] Button pauseMainMenuButton;

        [Header("Game Over")]
        [SerializeField] Image gameOverPanel;
        [SerializeField] Text gameOverTitle;
        [SerializeField] Text gameOverHeightText;
        [SerializeField] Text gameOverBestText;
        [SerializeField] Button gameOverRetryButton;
        [SerializeField] Button gameOverMainMenuButton;
        [SerializeField] Image gameOverTarDot;
        [SerializeField] Image gameOverTullaDot;
        [SerializeField] RopeLineUI gameOverRope;

        [Header("Developer HUD")]
        [SerializeField] Image developerHudPanel;
        [SerializeField] Text developerHudText;

        public TarTullaUITheme Theme => theme;

        public void SetTheme(TarTullaUITheme uiTheme) => theme = uiTheme;

        public void ApplyTheme()
        {
            if (theme == null)
                return;

            ApplyCameraBackground();
            ApplyImage(fullscreenBlocker, theme.overlayDimColor);

            bool hasMainMenuKeyArt = mainMenuBackgroundImage != null && mainMenuBackgroundImage.sprite != null;
            if (hasMainMenuKeyArt)
            {
                if (mainMenuPanel != null)
                    mainMenuPanel.color = Color.clear;
            }
            else
            {
                ApplyImage(mainMenuPanel, theme.panelColor);
            }

            ApplyText(mainMenuTitle, theme.primaryTextColor, theme.defaultTitleSize);
            ApplyText(mainMenuSubtitle, theme.secondaryTextColor, theme.defaultBodySize);
            theme.ApplyButtonColors(mainMenuStartButton, true);
            if (!hasMainMenuKeyArt)
                ApplyMotif(tarMotifDot, tullaMotifDot, mainMenuRope);

            ApplyImage(hudStrip, theme.hudStripColor);
            ApplyText(hudHeightText, theme.primaryTextColor, theme.defaultBodySize + 8);
            ApplyText(hudBestText, theme.secondaryTextColor, theme.defaultBodySize);
            ApplyImage(hudClimbHint, theme.hudClimbHintColor);
            theme.ApplyButtonColors(hudPauseButton, true);

            ApplyImage(pausePanel, theme.panelColor);
            ApplyText(pauseTitle, theme.primaryTextColor, theme.defaultHeaderSize);
            theme.ApplyButtonColors(pauseResumeButton, true);
            theme.ApplyButtonColors(pauseRestartButton, true);
            theme.ApplyButtonColors(pauseMainMenuButton, false);

            ApplyImage(gameOverPanel, theme.panelColor);
            ApplyText(gameOverTitle, theme.primaryTextColor, theme.defaultHeaderSize);
            ApplyText(gameOverHeightText, theme.primaryTextColor, theme.defaultBodySize + 12);
            ApplyText(gameOverBestText, theme.secondaryTextColor, theme.defaultBodySize);
            theme.ApplyButtonColors(gameOverRetryButton, true);
            theme.ApplyButtonColors(gameOverMainMenuButton, false);
            ApplyMotif(gameOverTarDot, gameOverTullaDot, gameOverRope);

            ApplyImage(developerHudPanel, theme.developerHudBackground);
            ApplyText(developerHudText, theme.developerHudTextColor, 26);
        }

        public void BindFromHierarchy(Transform safeArea, GameObject blocker, TarTullaUITheme uiTheme)
        {
            theme = uiTheme;
            fullscreenBlocker = blocker != null ? blocker.GetComponent<Image>() : null;
            gameplayCamera ??= Camera.main;

            DisableLegacyCanvasBackground(safeArea != null ? safeArea.parent : null);

            var mainMenu = safeArea.Find("MainMenuPanel");
            var hud = safeArea.Find("HUDPanel");
            var pause = safeArea.Find("PausePanel");
            var gameOver = safeArea.Find("GameOverPanel");
            var devHud = safeArea.Find("DeveloperHUDPanel");

            if (mainMenu != null)
            {
                mainMenuPanel = mainMenu.GetComponent<Image>();
                mainMenuBackgroundImage = mainMenu.Find("BackgroundImage")?.GetComponent<Image>();
                mainMenuTitle = mainMenu.Find("LogoTitle")?.GetComponent<Text>()
                    ?? mainMenu.Find("TitleText")?.GetComponent<Text>();
                mainMenuSubtitle = mainMenu.Find("Subtitle")?.GetComponent<Text>()
                    ?? mainMenu.Find("SubtitleText")?.GetComponent<Text>();
                mainMenuStartButton = mainMenu.Find("TapToStartButton")?.GetComponent<Button>()
                    ?? mainMenu.Find("StartButton")?.GetComponent<Button>();
                var motif = mainMenu.Find("PairMotif");
                if (motif != null)
                {
                    tarMotifDot = motif.Find("TarDot")?.GetComponent<Image>();
                    tullaMotifDot = motif.Find("TullaDot")?.GetComponent<Image>();
                    mainMenuRope = motif.GetComponent<RopeLineUI>();
                }
            }

            if (hud != null)
            {
                var strip = hud.Find("HudStrip");
                hudStrip = strip?.GetComponent<Image>();
                hudHeightText = strip?.Find("HeightText")?.GetComponent<Text>();
                hudBestText = strip?.Find("BestText")?.GetComponent<Text>();
                hudClimbHint = strip?.Find("ClimbHint")?.GetComponent<Image>();
                hudPauseButton = hud.Find("PauseButton")?.GetComponent<Button>();
            }

            if (pause != null)
            {
                pausePanel = pause.GetComponent<Image>();
                pauseTitle = pause.Find("PausedLabel")?.GetComponent<Text>();
                pauseResumeButton = pause.Find("ResumeButton")?.GetComponent<Button>();
                pauseRestartButton = pause.Find("RestartButton")?.GetComponent<Button>();
                pauseMainMenuButton = pause.Find("MainMenuButton")?.GetComponent<Button>();
            }

            if (gameOver != null)
            {
                gameOverPanel = gameOver.GetComponent<Image>();
                gameOverTitle = gameOver.Find("GameOverLabel")?.GetComponent<Text>();
                gameOverHeightText = gameOver.Find("HeightText")?.GetComponent<Text>();
                gameOverBestText = gameOver.Find("BestText")?.GetComponent<Text>();
                gameOverRetryButton = gameOver.Find("RetryButton")?.GetComponent<Button>();
                gameOverMainMenuButton = gameOver.Find("MainMenuButton")?.GetComponent<Button>();
                var motif = gameOver.Find("PairMotif");
                if (motif != null)
                {
                    gameOverTarDot = motif.Find("TarDot")?.GetComponent<Image>();
                    gameOverTullaDot = motif.Find("TullaDot")?.GetComponent<Image>();
                    gameOverRope = motif.GetComponent<RopeLineUI>();
                }
            }

            if (devHud != null)
            {
                developerHudPanel = devHud.GetComponent<Image>();
                developerHudText = devHud.Find("DebugText")?.GetComponent<Text>();
            }

            ApplyTheme();
        }

        static void ApplyImage(Image image, Color color)
        {
            if (image == null)
                return;

            image.color = color;
        }

        static void ApplyText(Text text, Color color, int fontSize)
        {
            if (text == null)
                return;

            text.color = color;
            if (fontSize > 0)
                text.fontSize = fontSize;
        }

        void ApplyMotif(Image tarDot, Image tullaDot, RopeLineUI rope)
        {
            if (tarDot != null)
                tarDot.color = theme.accentColorTar;

            if (tullaDot != null)
                tullaDot.color = theme.accentColorTulla;

            if (rope != null)
                rope.SetColor(theme.ropeColor);
        }

        void ApplyCameraBackground()
        {
            if (theme == null)
                return;

            gameplayCamera ??= Camera.main;
            if (gameplayCamera == null)
                return;

            gameplayCamera.clearFlags = CameraClearFlags.SolidColor;
            gameplayCamera.backgroundColor = theme.backgroundColor;
        }

        static void DisableLegacyCanvasBackground(Transform canvas)
        {
            if (canvas == null)
                return;

            var legacyBackground = canvas.Find("CanvasBackground");
            if (legacyBackground != null)
                legacyBackground.gameObject.SetActive(false);
        }
    }
}
