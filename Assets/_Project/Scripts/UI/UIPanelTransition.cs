using System.Collections;
using UnityEngine;

namespace TarTulla.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanelTransition : MonoBehaviour
    {
        [SerializeField] float fadeDuration = 0.12f;
        [SerializeField] bool useScale;
        [SerializeField] float scaleFrom = 0.96f;

        CanvasGroup canvasGroup;
        Coroutine routine;
        Vector3 defaultScale = Vector3.one;

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            defaultScale = transform.localScale;
        }

        public void ShowInstant()
        {
            StopRoutine();
            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            transform.localScale = defaultScale;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        public void HideInstant()
        {
            StopRoutine();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            gameObject.SetActive(false);
            transform.localScale = defaultScale;
        }

        public void ShowAnimated()
        {
            if (!gameObject.activeSelf)
                ShowInstant();

            if (fadeDuration <= 0f)
                return;

            StopRoutine();
            routine = StartCoroutine(AnimateShow());
        }

        public void HideAnimated()
        {
            if (!gameObject.activeSelf)
                return;

            if (fadeDuration <= 0f)
            {
                HideInstant();
                return;
            }

            StopRoutine();
            routine = StartCoroutine(AnimateHide());
        }

        IEnumerator AnimateShow()
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            canvasGroup.alpha = 0f;
            if (useScale)
                transform.localScale = defaultScale * scaleFrom;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                canvasGroup.alpha = t;
                if (useScale)
                    transform.localScale = Vector3.Lerp(defaultScale * scaleFrom, defaultScale, t);
                yield return null;
            }

            ShowInstant();
        }

        IEnumerator AnimateHide()
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            float startAlpha = canvasGroup.alpha;
            Vector3 startScale = transform.localScale;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                if (useScale)
                    transform.localScale = Vector3.Lerp(startScale, defaultScale * scaleFrom, t);
                yield return null;
            }

            HideInstant();
        }

        void StopRoutine()
        {
            if (routine == null)
                return;

            StopCoroutine(routine);
            routine = null;
        }
    }
}
