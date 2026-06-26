using System;
using UnityEngine;
using UnityEngine.UI;

namespace TarTulla.UI
{
    public class PauseView : MonoBehaviour
    {
        [SerializeField] Button resumeButton;
        [SerializeField] Button restartButton;
        [SerializeField] Button mainMenuButton;

        public event Action ResumePressed;
        public event Action RestartPressed;
        public event Action MainMenuPressed;

        void Awake()
        {
            WireButtons();
        }

        public void Bind(Button resume, Button restart, Button mainMenu)
        {
            resumeButton = resume;
            restartButton = restart;
            mainMenuButton = mainMenu;
            WireButtons();
        }

        public void SetVisible(bool visible, bool animateShow = false) =>
            UIViewVisibility.SetVisible(gameObject, visible, animateShow);

        void WireButtons()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveAllListeners();
                resumeButton.onClick.AddListener(() => ResumePressed?.Invoke());
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() => RestartPressed?.Invoke());
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(() => MainMenuPressed?.Invoke());
            }
        }
    }
}
