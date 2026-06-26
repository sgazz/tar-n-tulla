using UnityEngine;
using UnityEngine.UI;

namespace TarTulla.UI
{
    [ExecuteAlways]
    public class RopeLineUI : MonoBehaviour
    {
        [SerializeField] RectTransform startPoint;
        [SerializeField] RectTransform endPoint;
        [SerializeField] RectTransform lineRect;
        [SerializeField] Image lineImage;
        [SerializeField] float lineThickness = 4f;
        [SerializeField] Color color = new(0.88f, 0.8f, 0.55f, 1f);

        void Awake()
        {
            EnsureLineRect();
        }

        void LateUpdate() => RefreshLine();

        public void Configure(RectTransform start, RectTransform end, Color lineColor, float thickness = 4f)
        {
            startPoint = start;
            endPoint = end;
            color = lineColor;
            lineThickness = thickness;
            EnsureLineRect();
            RefreshLine();
        }

        public void SetColor(Color lineColor)
        {
            color = lineColor;
            if (lineImage != null)
                lineImage.color = color;
        }

        void EnsureLineRect()
        {
            if (lineRect != null)
                return;

            var lineGo = new GameObject("RopeLine", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            lineGo.transform.SetParent(transform, false);
            lineRect = lineGo.GetComponent<RectTransform>();
            lineImage = lineGo.GetComponent<Image>();
            lineImage.raycastTarget = false;
            lineImage.color = color;
        }

        void RefreshLine()
        {
            if (startPoint == null || endPoint == null || lineRect == null)
                return;

            Vector3 start = startPoint.position;
            Vector3 end = endPoint.position;
            Vector3 midpoint = (start + end) * 0.5f;
            Vector3 delta = end - start;
            float length = delta.magnitude;

            lineRect.position = midpoint;
            lineRect.sizeDelta = new Vector2(Mathf.Max(length, 1f), lineThickness);
            lineRect.rotation = length > 0.001f
                ? Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg)
                : Quaternion.identity;

            if (lineImage != null)
                lineImage.color = color;
        }
    }
}
