using UnityEngine;

namespace TarTulla.CameraSystems
{
  /// <summary>
  /// Keeps a SpriteRenderer aligned with the orthographic camera (portrait cover fit).
  /// Used for NearSideForeground — does not affect gameplay physics.
  /// </summary>
  [RequireComponent(typeof(SpriteRenderer))]
  public class CameraLockedSprite2D : MonoBehaviour
  {
    const float SpritePlaneZ = 0f;

    [SerializeField] Camera targetCamera;
    [SerializeField] Vector2 offset;
    [SerializeField] bool scaleToCoverViewport = true;
    [SerializeField] bool matchCameraRotation;
    [SerializeField] float coverBleed = 1.02f;
    [SerializeField] float scaleMultiplier = 1f;
    [SerializeField] [Range(0f, 1f)] float alpha = 1f;
    [SerializeField] bool logScaleOnce;

    SpriteRenderer spriteRenderer;
    float baseSpriteWidth = 1f;
    float baseSpriteHeight = 1f;
    bool scaleLogged;

    void Awake()
    {
      spriteRenderer = GetComponent<SpriteRenderer>();
      CacheSpriteSize();
      if (targetCamera == null)
        targetCamera = Camera.main;
    }

    void LateUpdate()
    {
      if (targetCamera == null || spriteRenderer == null || !spriteRenderer.enabled)
        return;

      if (spriteRenderer.sprite == null)
        return;

      CacheSpriteSize();

      Vector3 camPos = targetCamera.transform.position;
      transform.position = new Vector3(camPos.x + offset.x, camPos.y + offset.y, SpritePlaneZ);

      if (matchCameraRotation)
        transform.rotation = targetCamera.transform.rotation;

      if (scaleToCoverViewport)
        ApplyCoverScale();

      ApplySpriteAlpha();
    }

    void CacheSpriteSize()
    {
      if (spriteRenderer == null || spriteRenderer.sprite == null)
        return;

      var bounds = spriteRenderer.sprite.bounds.size;
      baseSpriteWidth = Mathf.Max(0.001f, bounds.x);
      baseSpriteHeight = Mathf.Max(0.001f, bounds.y);
    }

    float GetViewportAspect()
    {
      if (targetCamera.pixelWidth > 0 && targetCamera.pixelHeight > 0)
        return (float)targetCamera.pixelWidth / targetCamera.pixelHeight;

      return Mathf.Max(0.01f, targetCamera.aspect);
    }

    void ApplyCoverScale()
    {
      float aspect = GetViewportAspect();
      float worldHeight = targetCamera.orthographicSize * 2f;
      float worldWidth = worldHeight * aspect;
      float scaleX = worldWidth / baseSpriteWidth;
      float scaleY = worldHeight / baseSpriteHeight;
      float coverScale = Mathf.Max(scaleX, scaleY) * coverBleed * scaleMultiplier;

      if (!float.IsFinite(coverScale) || coverScale <= 0f)
        coverScale = 1f;

      transform.localScale = new Vector3(coverScale, coverScale, 1f);

      if (logScaleOnce && !scaleLogged)
      {
        scaleLogged = true;
        Debug.Log(
          $"[Tar&Tulla][Visual][CameraLockedSprite2D] {name}: camH={worldHeight:F2}, camW={worldWidth:F2}, " +
          $"spriteBounds=({baseSpriteWidth:F2},{baseSpriteHeight:F2}), scale={coverScale:F3}",
          this);
      }
    }

    void ApplySpriteAlpha()
    {
      var c = spriteRenderer.color;
      spriteRenderer.color = new Color(c.r, c.g, c.b, alpha);
    }

    public void SetTargetCamera(Camera camera)
    {
      targetCamera = camera;
    }

    public void SetEnabledVisual(bool enabled)
    {
      if (spriteRenderer != null)
        spriteRenderer.enabled = enabled;
    }

    public void ConfigureVisual(float visualAlpha, float visualScaleMultiplier, Vector2 visualOffset, bool logOnce = false)
    {
      alpha = Mathf.Clamp01(visualAlpha);
      scaleMultiplier = Mathf.Max(0.01f, visualScaleMultiplier);
      offset = visualOffset;
      logScaleOnce = logOnce;
      ApplySpriteAlpha();
    }

    public void SnapToCameraNow()
    {
      if (targetCamera == null || spriteRenderer == null || spriteRenderer.sprite == null)
        return;

      Vector3 camPos = targetCamera.transform.position;
      transform.position = new Vector3(camPos.x + offset.x, camPos.y + offset.y, SpritePlaneZ);
      if (scaleToCoverViewport)
        ApplyCoverScale();
    }
  }
}
