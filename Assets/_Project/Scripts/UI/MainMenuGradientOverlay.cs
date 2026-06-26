using UnityEngine;
using UnityEngine.UI;

namespace TarTulla.UI
{
    [RequireComponent(typeof(Image))]
    public class MainMenuGradientOverlay : MonoBehaviour
    {
        const int TextureHeight = 256;

        [SerializeField] float topAlpha = 0.52f;
        [SerializeField] float centerAlpha = 0.06f;
        [SerializeField] float bottomAlpha = 0.74f;

        void Awake()
        {
            var image = GetComponent<Image>();
            image.raycastTarget = false;
            image.sprite = CreateGradientSprite(topAlpha, centerAlpha, bottomAlpha);
            image.type = Image.Type.Simple;
            image.color = Color.white;
        }

        static Sprite CreateGradientSprite(float topAlpha, float centerAlpha, float bottomAlpha)
        {
            var texture = new Texture2D(1, TextureHeight, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < TextureHeight; y++)
            {
                float t = y / (TextureHeight - 1f);
                float alpha = EvaluateAlpha(t, topAlpha, centerAlpha, bottomAlpha);
                texture.SetPixel(0, y, new Color(0f, 0f, 0f, alpha));
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, TextureHeight), new Vector2(0.5f, 0.5f), 100f);
        }

        static float EvaluateAlpha(float normalizedY, float topAlpha, float centerAlpha, float bottomAlpha)
        {
            if (normalizedY >= 0.58f)
            {
                float topBlend = Mathf.InverseLerp(0.58f, 1f, normalizedY);
                return Mathf.Lerp(centerAlpha, topAlpha, topBlend);
            }

            if (normalizedY <= 0.32f)
            {
                float bottomBlend = Mathf.InverseLerp(0.32f, 0f, normalizedY);
                return Mathf.Lerp(centerAlpha, bottomAlpha, bottomBlend);
            }

            return centerAlpha;
        }
    }
}
