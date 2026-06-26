using UnityEngine;

namespace TarTulla.UI
{
    public class SafeAreaFitter : MonoBehaviour
    {
        [SerializeField] RectTransform target;
        [SerializeField] bool enableDebugLogs;

        Rect lastSafeArea;
        Vector2Int lastScreenSize;

        void Awake()
        {
            if (target == null)
                target = GetComponent<RectTransform>();
        }

        void OnEnable() => ApplySafeArea();

        void Update()
        {
            if (Screen.safeArea == lastSafeArea
                && Screen.width == lastScreenSize.x
                && Screen.height == lastScreenSize.y)
                return;

            ApplySafeArea();
        }

        public void ApplySafeArea()
        {
            if (target == null)
                return;

            Rect safeArea = Screen.safeArea;
            lastSafeArea = safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            if (Screen.width <= 0 || Screen.height <= 0)
                return;

            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            target.anchorMin = anchorMin;
            target.anchorMax = anchorMax;
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[Tar&Tulla][SafeArea] screen={Screen.width}x{Screen.height}, " +
                    $"safe=({safeArea.x:F0},{safeArea.y:F0},{safeArea.width:F0},{safeArea.height:F0}), " +
                    $"anchors=({anchorMin.x:F3},{anchorMin.y:F3})-({anchorMax.x:F3},{anchorMax.y:F3})",
                    this);
            }
        }
    }
}
