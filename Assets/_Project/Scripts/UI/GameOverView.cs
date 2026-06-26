using System;
using TarTulla.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TarTulla.UI
{
    public class GameOverView : MonoBehaviour
    {
        [SerializeField] Text heightText;
        [SerializeField] Text bestText;
        [SerializeField] Text statsText;
        [SerializeField] Text newBestBadge;
        [SerializeField] Button retryButton;
        [SerializeField] Button mainMenuButton;

        public event Action RetryPressed;
        public event Action MainMenuPressed;

        void Awake()
        {
            WireButtons();
        }

        public void Bind(
            Text height,
            Text best,
            Button retry,
            Button mainMenu,
            Text stats = null,
            Text newBest = null)
        {
            heightText = height;
            bestText = best;
            retryButton = retry;
            mainMenuButton = mainMenu;
            statsText = stats;
            newBestBadge = newBest;
            WireButtons();
        }

        public void SetVisible(bool visible, bool animateShow = false) =>
            UIViewVisibility.SetVisible(gameObject, visible, animateShow);

        public void SetResults(float height, float bestHeight) =>
            SetResults(new RunSummary
            {
                Height = height,
                BestHeight = bestHeight,
                IsNewBest = height >= bestHeight - 0.001f && height > 0.01f
            });

        public void SetResults(RunSummary summary)
        {
            if (heightText != null)
                heightText.text = $"Height {Mathf.RoundToInt(summary.Height)}m";

            if (bestText != null)
                bestText.text = $"Best {Mathf.RoundToInt(summary.BestHeight)}m";

            if (statsText != null)
            {
                bool hasStats = summary.Landings > 0 || summary.RopeSaves > 0;
                statsText.gameObject.SetActive(hasStats);
                statsText.text = hasStats
                    ? $"Landings {summary.Landings}  ·  Saves {summary.RopeSaves}"
                    : string.Empty;
            }

            if (newBestBadge != null)
                newBestBadge.gameObject.SetActive(summary.IsNewBest);
        }

        void WireButtons()
        {
            if (retryButton != null)
            {
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(() => RetryPressed?.Invoke());
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(() => MainMenuPressed?.Invoke());
            }
        }
    }
}
