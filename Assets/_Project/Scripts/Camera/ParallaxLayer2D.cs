using UnityEngine;

namespace TarTulla.CameraSystems
{
  /// <summary>
  /// Moves a transform relative to the camera for simple 2D parallax.
  /// parallaxFactor 0 = world-fixed, 1 = fully follows camera delta.
  /// </summary>
  public class ParallaxLayer2D : MonoBehaviour
  {
    [SerializeField] Transform targetCamera;
    [SerializeField] Vector2 parallaxFactor = new(0.15f, 0.2f);
    [SerializeField] bool lockToCameraX;
    [SerializeField] bool lockToCameraY;
    [SerializeField] Vector2 offset;
    [SerializeField] bool useSmoothing;
    [SerializeField] float smoothTime = 0.12f;

    Vector3 cameraStartPosition;
    Vector3 layerStartPosition;
    Vector3 smoothVelocity;

    void Awake()
    {
      if (targetCamera == null)
      {
        var cam = Camera.main;
        if (cam != null)
          targetCamera = cam.transform;
      }

      layerStartPosition = transform.position;
      if (targetCamera != null)
        cameraStartPosition = targetCamera.position;
    }

    void LateUpdate()
    {
      if (targetCamera == null)
        return;

      Vector3 cameraDelta = targetCamera.position - cameraStartPosition;
      Vector3 desired = layerStartPosition;

      float x = lockToCameraX
        ? targetCamera.position.x + offset.x
        : layerStartPosition.x + cameraDelta.x * parallaxFactor.x + offset.x;

      float y = lockToCameraY
        ? targetCamera.position.y + offset.y
        : layerStartPosition.y + cameraDelta.y * parallaxFactor.y + offset.y;

      desired = new Vector3(x, y, layerStartPosition.z);

      if (useSmoothing)
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref smoothVelocity, smoothTime);
      else
        transform.position = desired;
    }

    public void SetTargetCamera(Transform cameraTransform)
    {
      targetCamera = cameraTransform;
      cameraStartPosition = targetCamera != null ? targetCamera.position : Vector3.zero;
      layerStartPosition = transform.position;
    }

    public void Configure(Transform cameraTransform, Vector2 factor, Vector2 layerOffset, bool smooth = false,
      bool lockX = false, bool lockY = false)
    {
      targetCamera = cameraTransform;
      parallaxFactor = factor;
      offset = layerOffset;
      useSmoothing = smooth;
      lockToCameraX = lockX;
      lockToCameraY = lockY;
      ResetAnchor();
    }

    public void ResetAnchor()
    {
      layerStartPosition = transform.position;
      if (targetCamera != null)
        cameraStartPosition = targetCamera.position;
    }
  }
}
