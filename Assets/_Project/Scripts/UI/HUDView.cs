using TarTulla.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TarTulla.UI
{
    public class HUDView : MonoBehaviour
    {
        [SerializeField] ClimbProgressTracker progressTracker;
        [SerializeField] Text heightText;
        [SerializeField] Text bestText;
        [SerializeField] Button pauseButton;

        public event System.Action PausePressed;

        void Awake()
        {
            if (pauseButton != null)
                pauseButton.onClick.AddListener(() => PausePressed?.Invoke());
        }

        public void Bind(ClimbProgressTracker tracker, Text height, Text best, Button pause)
        {
            progressTracker = tracker;
            heightText = height;
            bestText = best;
            pauseButton = pause;

            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveAllListeners();
                pauseButton.onClick.AddListener(() => PausePressed?.Invoke());
            }
        }

        public void SetVisible(bool visible) =>
            UIViewVisibility.SetVisible(gameObject, visible, false);

        void Update()
        {
            if (!gameObject.activeInHierarchy || progressTracker == null)
                return;

            if (heightText != null)
                heightText.text = $"Height {Mathf.RoundToInt(progressTracker.CurrentHeight)}m";

            if (bestText != null)
                bestText.text = $"Best {Mathf.RoundToInt(progressTracker.BestHeight)}m";
        }
    }
}
