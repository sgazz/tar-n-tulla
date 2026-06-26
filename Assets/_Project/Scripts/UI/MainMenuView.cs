using System;
using UnityEngine;
using UnityEngine.UI;

namespace TarTulla.UI
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] Text titleText;
        [SerializeField] Text subtitleText;
        [SerializeField] Button startButton;

        public event Action StartPressed;

        void Awake()
        {
            if (startButton != null)
                startButton.onClick.AddListener(HandleStartPressed);
        }

        public void Bind(Text title, Text subtitle, Button start)
        {
            titleText = title;
            subtitleText = subtitle;
            startButton = start;

            if (startButton != null)
            {
                startButton.onClick.RemoveListener(HandleStartPressed);
                startButton.onClick.AddListener(HandleStartPressed);
            }
        }

        public void SetVisible(bool visible, bool animateShow = false) =>
            UIViewVisibility.SetVisible(gameObject, visible, animateShow);

        void HandleStartPressed() => StartPressed?.Invoke();
    }
}
