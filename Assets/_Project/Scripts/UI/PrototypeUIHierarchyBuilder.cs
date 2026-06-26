using System;
using TarTulla.Core;
using TarTulla.Input;
using TarTulla.Rope;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace TarTulla.UI
{
    [DefaultExecutionOrder(-200)]
    [RequireComponent(typeof(UIManager))]
    public class PrototypeUIHierarchyBuilder : MonoBehaviour
    {
        const float ReferenceWidth = 1080f;
        const float ReferenceHeight = 1920f;
        const float MotifDotSize = 44f;

        [SerializeField] TarTullaUITheme theme;
        [SerializeField] Font uiFont;
        [SerializeField] bool buildOnAwake = true;

        TarTullaUITheme ActiveTheme => theme != null ? theme : UIStyleDefaults.FallbackTheme;

        void Awake()
        {
            if (!buildOnAwake)
                return;

            var canvas = transform.Find("Canvas");
            if (canvas != null)
            {
                bool hasMainMenuKeyArt = canvas.Find("SafeArea/MainMenuPanel/BackgroundImage") != null;
                bool hasThemedMotif = canvas.Find("SafeArea/MainMenuPanel/PairMotif") != null;
                if (hasMainMenuKeyArt || hasThemedMotif)
                {
                    ApplyThemeToExisting(canvas);
                    return;
                }

                Destroy(canvas.gameObject);
            }

            BuildHierarchy();
        }

        [ContextMenu("Build UI Hierarchy")]
        public void BuildHierarchy()
        {
            EnsureEventSystem();
            var activeTheme = ActiveTheme;

            var canvasGo = CreateUIObject("Canvas", transform, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var fullscreenBlocker = BuildFullscreenBlocker(canvasGo.transform, activeTheme);
            var safeAreaGo = CreateUIObject("SafeArea", canvasGo.transform, typeof(RectTransform), typeof(SafeAreaFitter));
            StretchFull(safeAreaGo.GetComponent<RectTransform>());

            var mainMenu = BuildMainMenuPanel(safeAreaGo.transform, activeTheme);
            var hud = BuildHudPanel(safeAreaGo.transform, activeTheme);
            BuildDangerVignette(safeAreaGo.transform, activeTheme);
            BuildHeightMilestonePulse(hud.transform, activeTheme, hud);
            var countdown = BuildCountdownPanel(safeAreaGo.transform, activeTheme);
            var tutorial = BuildTutorialHintOverlay(safeAreaGo.transform, activeTheme);
            var pause = BuildPausePanel(safeAreaGo.transform, activeTheme);
            var gameOver = BuildGameOverPanel(safeAreaGo.transform, activeTheme);
            var devHud = BuildDeveloperHudPanel(safeAreaGo.transform, activeTheme);

            var uiManager = GetComponent<UIManager>();
            uiManager.RegisterViews(mainMenu, hud, pause, gameOver, devHud, fullscreenBlocker, countdown, tutorial);

            var progressTracker = FindAnyObjectByType<ClimbProgressTracker>();
            var levelBuilder = FindAnyObjectByType<PrototypeLevelBuilder>();
            var tiltInput = FindAnyObjectByType<MobileTiltInput2D>();
            var rope = FindAnyObjectByType<ElasticRope2D>();

            devHud.Bind(
                devHud.GetComponentInChildren<Text>(true),
                progressTracker,
                levelBuilder,
                tiltInput,
                rope);

            var applier = GetComponent<UIThemeApplier>() ?? gameObject.AddComponent<UIThemeApplier>();
            applier.BindFromHierarchy(safeAreaGo.transform, fullscreenBlocker, activeTheme);
        }

        void ApplyThemeToExisting(Transform canvas)
        {
            var applier = GetComponent<UIThemeApplier>() ?? gameObject.AddComponent<UIThemeApplier>();
            var blocker = canvas.Find("FullscreenBlocker")?.gameObject;
            var safeArea = canvas.Find("SafeArea");
            applier.BindFromHierarchy(safeArea, blocker, ActiveTheme);
            EnsureFeedbackViews(safeArea, ActiveTheme);
            EnsureOnboardingViews(safeArea, ActiveTheme);

            var mainMenuPanel = safeArea.Find("MainMenuPanel");
            if (mainMenuPanel != null)
                EnsureMainMenuKeyArt(mainMenuPanel, ActiveTheme);
        }

        void EnsureOnboardingViews(Transform safeArea, TarTullaUITheme activeTheme)
        {
            if (safeArea == null)
                return;

            RunCountdownView countdown = null;
            if (safeArea.Find("CountdownPanel") == null)
                countdown = BuildCountdownPanel(safeArea, activeTheme);
            else
                countdown = safeArea.Find("CountdownPanel")?.GetComponent<RunCountdownView>();

            TutorialHintView tutorial = null;
            if (safeArea.Find("TutorialHintOverlay") == null)
                tutorial = BuildTutorialHintOverlay(safeArea, activeTheme);
            else
                tutorial = safeArea.Find("TutorialHintOverlay")?.GetComponent<TutorialHintView>();

            var hudPanel = safeArea.Find("HUDPanel");
            if (hudPanel != null && hudPanel.Find("SavedFeedback") == null)
                BuildSavedFeedback(hudPanel, activeTheme);

            var gameOverPanel = safeArea.Find("GameOverPanel");
            if (gameOverPanel != null)
                EnsureGameOverSummaryFields(gameOverPanel, activeTheme);

            var uiManager = GetComponent<UIManager>();
            if (uiManager != null && (countdown != null || tutorial != null))
            {
                var mainMenu = safeArea.Find("MainMenuPanel")?.GetComponent<MainMenuView>();
                var hud = safeArea.Find("HUDPanel")?.GetComponent<HUDView>();
                var pause = safeArea.Find("PausePanel")?.GetComponent<PauseView>();
                var gameOver = gameOverPanel?.GetComponent<GameOverView>();
                var devHud = safeArea.Find("DeveloperHUDPanel")?.GetComponent<DeveloperHUDView>();
                var blocker = safeArea.parent?.Find("FullscreenBlocker")?.gameObject;
                uiManager.RegisterViews(mainMenu, hud, pause, gameOver, devHud, blocker, countdown, tutorial);
            }
        }

        void EnsureFeedbackViews(Transform safeArea, TarTullaUITheme activeTheme)
        {
            if (safeArea == null)
                return;

            if (safeArea.Find("DangerVignette") == null)
                BuildDangerVignette(safeArea, activeTheme);

            var hudPanel = safeArea.Find("HUDPanel");
            if (hudPanel != null && hudPanel.Find("HeightMilestonePulse") == null)
            {
                var hudView = hudPanel.GetComponent<HUDView>();
                BuildHeightMilestonePulse(hudPanel, activeTheme, hudView);
            }
        }

        void BuildDangerVignette(Transform parent, TarTullaUITheme activeTheme)
        {
            var vignette = CreatePanel("DangerVignette", parent, new Color(0.45f, 0.02f, 0.02f, 0f));
            StretchFull(vignette.GetComponent<RectTransform>());
            vignette.GetComponent<Image>().raycastTarget = false;
            vignette.AddComponent<DangerVignetteView>();
            vignette.transform.SetAsFirstSibling();
        }

        void BuildHeightMilestonePulse(Transform hudPanel, TarTullaUITheme activeTheme, HUDView hudView)
        {
            var pulseRoot = CreateUIObject("HeightMilestonePulse", hudPanel, typeof(RectTransform), typeof(CanvasGroup));
            SetAnchors(pulseRoot.GetComponent<RectTransform>(), new Vector2(0.45f, 0.88f), new Vector2(0.78f, 0.98f));
            pulseRoot.GetComponent<CanvasGroup>().blocksRaycasts = false;
            pulseRoot.GetComponent<CanvasGroup>().interactable = false;

            var pulseText = CreateText(pulseRoot.transform, "PulseText", "+10m", activeTheme.defaultBodySize + 10,
                TextAnchor.MiddleCenter, activeTheme.primaryTextColor, Vector2.zero, Vector2.one);
            pulseText.gameObject.SetActive(false);

            Text heightText = hudPanel.Find("HudStrip/HeightText")?.GetComponent<Text>();
            Text bestText = hudPanel.Find("HudStrip/BestText")?.GetComponent<Text>();

            var pulseView = pulseRoot.AddComponent<HeightMilestonePulseView>();
            pulseView.Bind(heightText, bestText, pulseText, pulseRoot.GetComponent<CanvasGroup>());
        }

        GameObject BuildFullscreenBlocker(Transform parent, TarTullaUITheme activeTheme)
        {
            var blocker = CreatePanel("FullscreenBlocker", parent, activeTheme.overlayDimColor);
            blocker.transform.SetSiblingIndex(1);
            StretchFull(blocker.GetComponent<RectTransform>());
            blocker.SetActive(false);
            return blocker;
        }

        MainMenuView BuildMainMenuPanel(Transform parent, TarTullaUITheme activeTheme)
        {
            var panel = CreatePanel("MainMenuPanel", parent, new Color(0f, 0f, 0f, 0f));
            StretchFull(panel.GetComponent<RectTransform>());
            panel.GetComponent<Image>().raycastTarget = false;
            if (panel.GetComponent<RectMask2D>() == null)
                panel.AddComponent<RectMask2D>();
            AddPanelTransition(panel);

            BuildMainMenuBackground(panel.transform, activeTheme);
            BuildMainMenuGradientOverlay(panel.transform);

            var title = CreateText(panel.transform, "LogoTitle", "Tar&Tulla", activeTheme.defaultTitleSize, TextAnchor.MiddleCenter,
                activeTheme.primaryTextColor, new Vector2(0.06f, 0.78f), new Vector2(0.94f, 0.92f));
            title.fontStyle = FontStyle.Bold;

            var subtitle = CreateText(panel.transform, "Subtitle", "Two jumpers. One rope. Keep climbing.",
                activeTheme.defaultBodySize, TextAnchor.MiddleCenter, activeTheme.secondaryTextColor,
                new Vector2(0.08f, 0.71f), new Vector2(0.92f, 0.78f));

            var startButton = CreateButton(panel.transform, "TapToStartButton", "Tap to Start", activeTheme,
                new Vector2(0.12f, 0.07f), new Vector2(0.88f, 0.15f), true);

            var view = panel.AddComponent<MainMenuView>();
            view.Bind(title, subtitle, startButton);
            return view;
        }

        void EnsureMainMenuKeyArt(Transform mainMenuPanel, TarTullaUITheme activeTheme)
        {
            if (mainMenuPanel.Find("BackgroundImage") != null)
                return;

            var motif = mainMenuPanel.Find("PairMotif");
            if (motif != null)
                DestroyImmediateSafe(motif.gameObject);

            var panelImage = mainMenuPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(0f, 0f, 0f, 0f);
                panelImage.raycastTarget = false;
            }

            if (mainMenuPanel.GetComponent<RectMask2D>() == null)
                mainMenuPanel.gameObject.AddComponent<RectMask2D>();

            BuildMainMenuBackground(mainMenuPanel, activeTheme);
            BuildMainMenuGradientOverlay(mainMenuPanel);

            RenameOrCreateMainMenuText(mainMenuPanel, "LogoTitle", "TitleText", "Tar&Tulla",
                activeTheme.defaultTitleSize, activeTheme.primaryTextColor,
                new Vector2(0.06f, 0.78f), new Vector2(0.94f, 0.92f), FontStyle.Bold);
            RenameOrCreateMainMenuText(mainMenuPanel, "Subtitle", "SubtitleText",
                "Two jumpers. One rope. Keep climbing.",
                activeTheme.defaultBodySize, activeTheme.secondaryTextColor,
                new Vector2(0.08f, 0.71f), new Vector2(0.92f, 0.78f), FontStyle.Normal);

            var startButton = mainMenuPanel.Find("TapToStartButton")?.GetComponent<Button>()
                ?? mainMenuPanel.Find("StartButton")?.GetComponent<Button>();
            if (startButton != null)
            {
                startButton.gameObject.name = "TapToStartButton";
                SetAnchors(startButton.GetComponent<RectTransform>(), new Vector2(0.12f, 0.07f), new Vector2(0.88f, 0.15f));
            }

            var view = mainMenuPanel.GetComponent<MainMenuView>();
            if (view != null)
            {
                view.Bind(
                    mainMenuPanel.Find("LogoTitle")?.GetComponent<Text>(),
                    mainMenuPanel.Find("Subtitle")?.GetComponent<Text>(),
                    startButton);
            }
        }

        void BuildMainMenuBackground(Transform parent, TarTullaUITheme activeTheme)
        {
            var sprite = activeTheme.mainMenuKeyArt;
            if (sprite == null)
                return;

            var background = CreateUIObject("BackgroundImage", parent, typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(AspectRatioFitter));
            background.transform.SetAsFirstSibling();
            StretchFull(background.GetComponent<RectTransform>());

            var image = background.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.type = Image.Type.Simple;
            image.color = Color.white;

            var fitter = background.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
        }

        void BuildMainMenuGradientOverlay(Transform parent)
        {
            var overlay = CreateUIObject("DarkGradientOverlay", parent, typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(MainMenuGradientOverlay));
            overlay.transform.SetSiblingIndex(1);
            StretchFull(overlay.GetComponent<RectTransform>());
        }

        void RenameOrCreateMainMenuText(Transform parent, string newName, string legacyName, string content,
            int fontSize, Color color, Vector2 anchorMin, Vector2 anchorMax, FontStyle fontStyle)
        {
            var existing = parent.Find(newName) ?? parent.Find(legacyName);
            Text text;
            if (existing != null)
            {
                existing.name = newName;
                text = existing.GetComponent<Text>();
            }
            else
            {
                text = CreateText(parent, newName, content, fontSize, TextAnchor.MiddleCenter, color, anchorMin, anchorMax);
            }

            if (text == null)
                return;

            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.fontStyle = fontStyle;
            SetAnchors(text.rectTransform, anchorMin, anchorMax);
        }

        HUDView BuildHudPanel(Transform parent, TarTullaUITheme activeTheme)
        {
            var panel = CreatePanel("HUDPanel", parent, new Color(0f, 0f, 0f, 0f));
            StretchFull(panel.GetComponent<RectTransform>());
            panel.GetComponent<Image>().raycastTarget = false;

            var climbHint = CreatePanel("ClimbHint", panel.transform, activeTheme.hudClimbHintColor);
            SetAnchors(climbHint.GetComponent<RectTransform>(), new Vector2(0.035f, 0.905f), new Vector2(0.042f, 0.975f));
            climbHint.GetComponent<Image>().raycastTarget = false;

            var strip = CreatePanel("HudStrip", panel.transform, activeTheme.hudStripColor);
            SetAnchors(strip.GetComponent<RectTransform>(), new Vector2(0.045f, 0.9f), new Vector2(0.6f, 0.98f));

            var heightText = CreateText(strip.transform, "HeightText", "Height 0m", activeTheme.defaultBodySize + 8,
                TextAnchor.MiddleLeft, activeTheme.primaryTextColor, new Vector2(0.06f, 0.52f), new Vector2(0.96f, 0.96f));
            var bestText = CreateText(strip.transform, "BestText", "Best 0m", activeTheme.defaultBodySize,
                TextAnchor.MiddleLeft, activeTheme.secondaryTextColor, new Vector2(0.06f, 0.04f), new Vector2(0.96f, 0.48f));

            var pauseButton = CreateButton(panel.transform, "PauseButton", "II", activeTheme,
                new Vector2(0.8f, 0.91f), new Vector2(0.96f, 0.98f), true);

            BuildSavedFeedback(panel.transform, activeTheme);

            var view = panel.AddComponent<HUDView>();
            view.Bind(FindAnyObjectByType<ClimbProgressTracker>(), heightText, bestText, pauseButton);
            panel.SetActive(false);
            return view;
        }

        PauseView BuildPausePanel(Transform parent, TarTullaUITheme activeTheme)
        {
            var panel = CreatePanel("PausePanel", parent, activeTheme.panelColor);
            StretchFull(panel.GetComponent<RectTransform>());
            AddPanelTransition(panel);

            CreateText(panel.transform, "PausedLabel", "Paused", activeTheme.defaultHeaderSize, TextAnchor.MiddleCenter,
                activeTheme.primaryTextColor, new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.7f));

            var resume = CreateButton(panel.transform, "ResumeButton", "Resume", activeTheme,
                new Vector2(0.14f, 0.44f), new Vector2(0.86f, 0.52f), true);
            var restart = CreateButton(panel.transform, "RestartButton", "Restart", activeTheme,
                new Vector2(0.14f, 0.32f), new Vector2(0.86f, 0.4f), true);
            var mainMenu = CreateButton(panel.transform, "MainMenuButton", "Main Menu", activeTheme,
                new Vector2(0.14f, 0.2f), new Vector2(0.86f, 0.28f), false);

            var view = panel.AddComponent<PauseView>();
            view.Bind(resume, restart, mainMenu);
            panel.SetActive(false);
            return view;
        }

        GameOverView BuildGameOverPanel(Transform parent, TarTullaUITheme activeTheme)
        {
            var panel = CreatePanel("GameOverPanel", parent, activeTheme.panelColor);
            StretchFull(panel.GetComponent<RectTransform>());
            AddPanelTransition(panel);

            BuildPairMotif(panel.transform, "PairMotif", new Vector2(0.38f, 0.72f), new Vector2(0.62f, 0.78f), activeTheme);

            CreateText(panel.transform, "GameOverLabel", "Run Over", activeTheme.defaultHeaderSize, TextAnchor.MiddleCenter,
                activeTheme.primaryTextColor, new Vector2(0.1f, 0.58f), new Vector2(0.9f, 0.68f));

            var heightText = CreateText(panel.transform, "HeightText", "Height 0m", activeTheme.defaultBodySize + 12,
                TextAnchor.MiddleCenter, activeTheme.primaryTextColor, new Vector2(0.1f, 0.46f), new Vector2(0.9f, 0.54f));
            var bestText = CreateText(panel.transform, "BestText", "Best 0m", activeTheme.defaultBodySize,
                TextAnchor.MiddleCenter, activeTheme.secondaryTextColor, new Vector2(0.1f, 0.38f), new Vector2(0.9f, 0.46f));
            var statsText = CreateText(panel.transform, "StatsText", "Landings 0  ·  Saves 0", activeTheme.defaultBodySize - 2,
                TextAnchor.MiddleCenter, activeTheme.secondaryTextColor, new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.36f));
            statsText.gameObject.SetActive(false);
            var newBestBadge = CreateText(panel.transform, "NewBestBadge", "New Best!", activeTheme.defaultBodySize + 4,
                TextAnchor.MiddleCenter, activeTheme.accentColorTulla, new Vector2(0.1f, 0.54f), new Vector2(0.9f, 0.6f));
            newBestBadge.fontStyle = FontStyle.Bold;
            newBestBadge.gameObject.SetActive(false);

            var retry = CreateButton(panel.transform, "RetryButton", "Retry", activeTheme,
                new Vector2(0.14f, 0.22f), new Vector2(0.86f, 0.3f), true);
            var mainMenu = CreateButton(panel.transform, "MainMenuButton", "Main Menu", activeTheme,
                new Vector2(0.14f, 0.1f), new Vector2(0.86f, 0.18f), false);

            var view = panel.AddComponent<GameOverView>();
            view.Bind(heightText, bestText, retry, mainMenu, statsText, newBestBadge);
            panel.SetActive(false);
            return view;
        }

        RunCountdownView BuildCountdownPanel(Transform parent, TarTullaUITheme activeTheme)
        {
            var panel = CreateUIObject("CountdownPanel", parent, typeof(RectTransform), typeof(CanvasGroup));
            StretchFull(panel.GetComponent<RectTransform>());
            panel.GetComponent<CanvasGroup>().blocksRaycasts = false;
            panel.GetComponent<CanvasGroup>().interactable = false;

            var countdownText = CreateText(panel.transform, "CountdownText", "Ready", activeTheme.defaultTitleSize + 24,
                TextAnchor.MiddleCenter, activeTheme.primaryTextColor, new Vector2(0.15f, 0.42f), new Vector2(0.85f, 0.58f));
            countdownText.fontStyle = FontStyle.Bold;

            var view = panel.AddComponent<RunCountdownView>();
            view.Bind(countdownText, panel.GetComponent<CanvasGroup>());
            panel.SetActive(false);
            return view;
        }

        TutorialHintView BuildTutorialHintOverlay(Transform parent, TarTullaUITheme activeTheme)
        {
            var overlay = CreateUIObject("TutorialHintOverlay", parent, typeof(RectTransform), typeof(CanvasGroup));
            SetAnchors(overlay.GetComponent<RectTransform>(), new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.22f));
            overlay.GetComponent<CanvasGroup>().blocksRaycasts = false;
            overlay.GetComponent<CanvasGroup>().interactable = false;

            var hintText = CreateText(overlay.transform, "HintText", "Tilt your phone", activeTheme.defaultBodySize + 6,
                TextAnchor.MiddleCenter, activeTheme.primaryTextColor, Vector2.zero, Vector2.one);
            hintText.fontStyle = FontStyle.Italic;

            var view = overlay.AddComponent<TutorialHintView>();
            view.Bind(hintText, overlay.GetComponent<CanvasGroup>());
            overlay.SetActive(true);
            return view;
        }

        void BuildSavedFeedback(Transform hudPanel, TarTullaUITheme activeTheme)
        {
            var root = CreateUIObject("SavedFeedback", hudPanel, typeof(RectTransform), typeof(CanvasGroup));
            SetAnchors(root.GetComponent<RectTransform>(), new Vector2(0.3f, 0.42f), new Vector2(0.7f, 0.5f));
            root.GetComponent<CanvasGroup>().blocksRaycasts = false;
            root.GetComponent<CanvasGroup>().interactable = false;

            var savedText = CreateText(root.transform, "SavedText", "Saved!", activeTheme.defaultBodySize + 8,
                TextAnchor.MiddleCenter, activeTheme.accentColorTulla, Vector2.zero, Vector2.one);
            savedText.fontStyle = FontStyle.Bold;

            var view = root.AddComponent<SavedFeedbackView>();
            view.Bind(savedText, root.GetComponent<CanvasGroup>());
        }

        void EnsureGameOverSummaryFields(Transform gameOverPanel, TarTullaUITheme activeTheme)
        {
            var view = gameOverPanel.GetComponent<GameOverView>();
            if (view == null)
                return;

            var statsText = gameOverPanel.Find("StatsText")?.GetComponent<Text>();
            if (statsText == null)
            {
                statsText = CreateText(gameOverPanel, "StatsText", "Landings 0  ·  Saves 0", activeTheme.defaultBodySize - 2,
                    TextAnchor.MiddleCenter, activeTheme.secondaryTextColor, new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.36f));
                statsText.gameObject.SetActive(false);
            }

            var newBestBadge = gameOverPanel.Find("NewBestBadge")?.GetComponent<Text>();
            if (newBestBadge == null)
            {
                newBestBadge = CreateText(gameOverPanel, "NewBestBadge", "New Best!", activeTheme.defaultBodySize + 4,
                    TextAnchor.MiddleCenter, activeTheme.accentColorTulla, new Vector2(0.1f, 0.54f), new Vector2(0.9f, 0.6f));
                newBestBadge.fontStyle = FontStyle.Bold;
                newBestBadge.gameObject.SetActive(false);
            }

            var heightText = gameOverPanel.Find("HeightText")?.GetComponent<Text>();
            var bestText = gameOverPanel.Find("BestText")?.GetComponent<Text>();
            var retry = gameOverPanel.Find("RetryButton")?.GetComponent<Button>();
            var mainMenu = gameOverPanel.Find("MainMenuButton")?.GetComponent<Button>();
            view.Bind(heightText, bestText, retry, mainMenu, statsText, newBestBadge);
        }

        DeveloperHUDView BuildDeveloperHudPanel(Transform parent, TarTullaUITheme activeTheme)
        {
            var panel = CreatePanel("DeveloperHUDPanel", parent, activeTheme.developerHudBackground);
            SetAnchors(panel.GetComponent<RectTransform>(), new Vector2(0.04f, 0.04f), new Vector2(0.58f, 0.22f));

            var text = CreateText(panel.transform, "DebugText", "DEV HUD", 26, TextAnchor.UpperLeft,
                activeTheme.developerHudTextColor);
            text.fontStyle = FontStyle.Bold;
            StretchFull(text.rectTransform, 12f);

            var view = panel.AddComponent<DeveloperHUDView>();
            panel.SetActive(false);
            return view;
        }

        void BuildPairMotif(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, TarTullaUITheme activeTheme)
        {
            var motifRoot = CreateUIObject(name, parent, typeof(RectTransform), typeof(RopeLineUI));
            SetAnchors(motifRoot.GetComponent<RectTransform>(), anchorMin, anchorMax);

            var tarDot = CreateDot(motifRoot.transform, "TarDot", activeTheme.accentColorTar, new Vector2(0f, 0.5f));
            var tullaDot = CreateDot(motifRoot.transform, "TullaDot", activeTheme.accentColorTulla, new Vector2(1f, 0.5f));

            var rope = motifRoot.GetComponent<RopeLineUI>();
            rope.Configure(tarDot.GetComponent<RectTransform>(), tullaDot.GetComponent<RectTransform>(),
                activeTheme.ropeColor, 5f);
        }

        GameObject CreateDot(Transform parent, string name, Color color, Vector2 anchor)
        {
            var dot = CreatePanel(name, parent, color);
            var rect = dot.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(MotifDotSize, MotifDotSize);
            dot.GetComponent<Image>().raycastTarget = false;
            return dot;
        }

        void AddPanelTransition(GameObject panel)
        {
            if (panel.GetComponent<CanvasGroup>() == null)
                panel.AddComponent<CanvasGroup>();

            if (panel.GetComponent<UIPanelTransition>() == null)
                panel.AddComponent<UIPanelTransition>();
        }

        void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
                return;

            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.transform.SetParent(transform, false);
            eventSystemGo.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            eventSystemGo.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemGo.AddComponent<StandaloneInputModule>();
#endif
        }

        GameObject CreatePanel(string name, Transform parent, Color color)
        {
            var panel = CreateUIObject(name, parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var image = panel.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = color.a > 0.01f;
            return panel;
        }

        Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor anchor, Color color,
            Vector2? anchorMin = null, Vector2? anchorMax = null)
        {
            var textGo = CreateUIObject(name, parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var text = textGo.GetComponent<Text>();
            text.font = GetFont();
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = color;
            text.text = content;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;

            if (anchorMin.HasValue && anchorMax.HasValue)
                SetAnchors(text.rectTransform, anchorMin.Value, anchorMax.Value);
            else
                StretchFull(text.rectTransform);

            return text;
        }

        Button CreateButton(Transform parent, string name, string label, TarTullaUITheme activeTheme,
            Vector2 anchorMin, Vector2 anchorMax, bool primary)
        {
            var buttonGo = CreateUIObject(name, parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var button = buttonGo.GetComponent<Button>();
            activeTheme.ApplyButtonColors(button, primary);

            SetAnchors(buttonGo.GetComponent<RectTransform>(), anchorMin, anchorMax);

            var text = CreateText(buttonGo.transform, "Label", label, activeTheme.defaultButtonTextSize,
                TextAnchor.MiddleCenter, activeTheme.buttonTextColor);
            StretchFull(text.rectTransform, 8f);

            var layout = buttonGo.AddComponent<LayoutElement>();
            layout.minHeight = UIStyleDefaults.ButtonMinHeight;

            return button;
        }

        static GameObject CreateUIObject(string name, Transform parent, params Type[] components)
        {
            var go = new GameObject(name, components);
            go.transform.SetParent(parent, false);
            return go;
        }

        static void StretchFull(RectTransform rect, float padding = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
        }

        static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        Font GetFont()
        {
            if (uiFont != null)
                return uiFont;

            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return uiFont;
        }

        static void DestroyImmediateSafe(GameObject target)
        {
            if (target == null)
                return;

            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
        }
    }
}
