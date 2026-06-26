using System;
using TarTulla.Core;
using TarTulla.Game;
using UnityEngine;
using UnityEngine.Events;

namespace TarTulla.UI
{
    [DefaultExecutionOrder(100)]
    public class UIManager : MonoBehaviour
    {
        [SerializeField] PrototypeRunController runController;
        [SerializeField] RunStatsTracker runStatsTracker;
        [SerializeField] MainMenuView mainMenuView;
        [SerializeField] HUDView hudView;
        [SerializeField] RunCountdownView countdownView;
        [SerializeField] TutorialHintView tutorialHintView;
        [SerializeField] PauseView pauseView;
        [SerializeField] GameOverView gameOverView;
        [SerializeField] DeveloperHUDView developerHudView;
        [SerializeField] GameObject fullscreenBlocker;
        [SerializeField] bool showDeveloperHudInEditor;
        [SerializeField] bool enableDebugLogs;

        [Header("Events")]
        [SerializeField] UnityEvent<UIState> onStateChanged;

        UIState currentState = UIState.Boot;
        bool countdownInProgress;

        public UIState CurrentState => currentState;
        public UnityEvent<UIState> OnStateChanged => onStateChanged;

        void Awake()
        {
            ResolveReferences();
            WireViewEvents();
        }

        void Start()
        {
            Time.timeScale = 1f;
            TransitionTo(UIState.MainMenu, ShowMainMenuInternal);
        }

        void OnDestroy()
        {
            if (runController != null)
                runController.OnRunFailed -= HandleRunFailed;
        }

        public void ShowMainMenu()
        {
            if (currentState == UIState.MainMenu)
                return;

            TransitionTo(UIState.MainMenu, ShowMainMenuInternal);
        }

        public void StartGame()
        {
            if (currentState == UIState.Playing || currentState == UIState.Ready || countdownInProgress)
                return;

            BeginRunStart();
        }

        public void PauseGame()
        {
            if (currentState != UIState.Playing)
                return;

            TransitionTo(UIState.Paused, PauseGameInternal);
        }

        public void ResumeGame()
        {
            if (currentState != UIState.Paused)
                return;

            TransitionTo(UIState.Playing, ResumeGameInternal);
        }

        public void ShowGameOver(float height, float bestHeight)
        {
            if (currentState == UIState.GameOver)
                return;

            var summary = runStatsTracker != null
                ? runStatsTracker.BuildSummary()
                : new RunSummary
                {
                    Height = height,
                    BestHeight = bestHeight,
                    IsNewBest = height >= bestHeight - 0.001f && height > 0.01f
                };

            TransitionTo(UIState.GameOver, () => ShowGameOverInternal(summary));
        }

        public void RestartRun()
        {
            if (currentState != UIState.Paused && currentState != UIState.GameOver)
                return;

            BeginRunStart();
        }

        public void RegisterViews(
            MainMenuView mainMenu,
            HUDView hud,
            PauseView pause,
            GameOverView gameOver,
            DeveloperHUDView developerHud,
            GameObject blocker,
            RunCountdownView countdown = null,
            TutorialHintView tutorial = null)
        {
            mainMenuView = mainMenu;
            hudView = hud;
            pauseView = pause;
            gameOverView = gameOver;
            developerHudView = developerHud;
            fullscreenBlocker = blocker;
            countdownView = countdown;
            tutorialHintView = tutorial;

            if (developerHudView != null)
            {
                developerHudView.ShowDeveloperHUD = showDeveloperHudInEditor;
                developerHudView.BindUiManager(this);
            }

            WireViewEvents();
        }

        void HandleRunFailed(float height, float bestHeight) => ShowGameOver(height, bestHeight);

        void BeginRunStart()
        {
            countdownInProgress = false;
            tutorialHintView?.StopHints();
            countdownView?.StopCountdown();

            runStatsTracker?.ResetForRun();

            if (ShouldShowCountdown())
            {
                TransitionTo(UIState.Ready, BeginCountdownInternal);
                return;
            }

            TransitionTo(UIState.Playing, StartGameImmediateInternal);
        }

        void BeginCountdownInternal()
        {
            Time.timeScale = 0f;
            runController?.PrepareRun();
            hudView?.SetVisible(true);
            countdownView?.SetVisible(true);
            UpdateDeveloperHudVisibility();
            SetFullscreenBlocker(false);

            if (countdownView == null)
            {
                FinishCountdown();
                return;
            }

            countdownInProgress = true;
            countdownView.PlayCountdown(FinishCountdown);
        }

        void FinishCountdown()
        {
            countdownInProgress = false;
            countdownView?.SetVisible(false);
            TransitionTo(UIState.Playing, StartGameImmediateInternal);
        }

        void StartGameImmediateInternal()
        {
            Time.timeScale = 1f;
            runController?.StartPreparedRun();
            hudView?.SetVisible(true);
            UpdateDeveloperHudVisibility();
            SetFullscreenBlocker(false);
            tutorialHintView?.TryShowFirstRunHints();
        }

        void TransitionTo(UIState nextState, Action applyState)
        {
            UIState previousState = currentState;
            HideAllPanels();
            applyState();
            SetState(nextState, previousState);
        }

        void ShowMainMenuInternal()
        {
            countdownInProgress = false;
            Time.timeScale = 1f;
            tutorialHintView?.StopHints();
            countdownView?.StopCountdown();
            runController?.StopRun();
            mainMenuView?.SetVisible(true, true);
            developerHudView?.SetVisible(false);
            SetFullscreenBlocker(false);
        }

        void PauseGameInternal()
        {
            Time.timeScale = 0f;
            runController?.SetRunPaused(true);
            hudView?.SetVisible(true);
            pauseView?.SetVisible(true, true);
            SetFullscreenBlocker(true);
        }

        void ResumeGameInternal()
        {
            Time.timeScale = 1f;
            runController?.SetRunPaused(false);
            hudView?.SetVisible(true);
            UpdateDeveloperHudVisibility();
            SetFullscreenBlocker(false);
        }

        void ShowGameOverInternal(RunSummary summary)
        {
            Time.timeScale = 0f;
            countdownInProgress = false;
            tutorialHintView?.StopHints();
            countdownView?.StopCountdown();
            runController?.SetRunPaused(true);
            gameOverView?.SetResults(summary);
            gameOverView?.SetVisible(true, true);
            developerHudView?.SetVisible(false);
            SetFullscreenBlocker(true);
        }

        void WireViewEvents()
        {
            if (mainMenuView != null)
            {
                mainMenuView.StartPressed -= StartGame;
                mainMenuView.StartPressed += StartGame;
            }

            if (hudView != null)
            {
                hudView.PausePressed -= PauseGame;
                hudView.PausePressed += PauseGame;
            }

            if (pauseView != null)
            {
                pauseView.ResumePressed -= ResumeGame;
                pauseView.RestartPressed -= RestartRun;
                pauseView.MainMenuPressed -= ShowMainMenu;
                pauseView.ResumePressed += ResumeGame;
                pauseView.RestartPressed += RestartRun;
                pauseView.MainMenuPressed += ShowMainMenu;
            }

            if (gameOverView != null)
            {
                gameOverView.RetryPressed -= RestartRun;
                gameOverView.MainMenuPressed -= ShowMainMenu;
                gameOverView.RetryPressed += RestartRun;
                gameOverView.MainMenuPressed += ShowMainMenu;
            }

            if (runController != null)
            {
                runController.OnRunFailed -= HandleRunFailed;
                runController.OnRunFailed += HandleRunFailed;
            }
        }

        void HideAllPanels()
        {
            mainMenuView?.SetVisible(false);
            hudView?.SetVisible(false);
            countdownView?.SetVisible(false);
            pauseView?.SetVisible(false);
            gameOverView?.SetVisible(false);
            developerHudView?.SetVisible(false);
            SetFullscreenBlocker(false);
        }

        void SetFullscreenBlocker(bool visible)
        {
            if (fullscreenBlocker != null)
                fullscreenBlocker.SetActive(visible);
        }

        void UpdateDeveloperHudVisibility()
        {
            if (developerHudView == null)
                return;

            developerHudView.BindUiManager(this);
            developerHudView.SetVisible(developerHudView.ShowDeveloperHUD);
        }

        void SetState(UIState state, UIState previousState)
        {
            currentState = state;
            onStateChanged?.Invoke(state);

            if (enableDebugLogs)
                Debug.Log($"[Tar&Tulla][UI] State changed: {previousState} -> {state}", this);
        }

        bool ShouldShowCountdown()
        {
            var onboarding = TarTullaTuningAccess.GetActiveProfile()?.Onboarding;
            return onboarding == null || onboarding.showCountdown;
        }

        void ResolveReferences()
        {
            runController ??= FindAnyObjectByType<PrototypeRunController>();
            runStatsTracker ??= FindAnyObjectByType<RunStatsTracker>();
        }
    }
}
