using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TarTulla.CameraSystems
{
    /// <summary>
    /// Organizes visual layer hierarchy and creates placeholder background / near foreground sprites.
    ///
    /// Art paths:
    /// - Assets/_Project/Art/Backgrounds/Far_Background_1080x1920.png
    /// - Assets/_Project/Art/Backgrounds/Mid_Background_1080x1920.png
    /// - Assets/_Project/Art/Backgrounds/Near_Side_Foreground_1080x1920_transparent.png
    ///
    /// Import: Texture Type = Sprite (2D and UI), Max Size 2048+, Alpha Is Transparency ON for foreground PNG.
    /// </summary>
    [DefaultExecutionOrder(-110)]
    public class SceneVisualLayerSetup : MonoBehaviour
    {
        const string BackgroundRootName = "BackgroundRoot";
        const string GameplayRootName = "GameplayRoot";
        const string ForegroundRootName = "ForegroundRoot";
        const string PlatformsName = "Platforms";
        const string JumpersName = "Jumpers";
        const string RopeName = "Rope";
        const string NearForegroundName = "NearSideForeground";
        const string FarBackgroundName = "FarBackground";

        const string FarBackgroundPath = "Assets/_Project/Art/Backgrounds/Far_Background_1080x1920.png";
        const string MidBackgroundPath = "Assets/_Project/Art/Backgrounds/Mid_Background_1080x1920.png";
        const string NearForegroundPath = "Assets/_Project/Art/Backgrounds/Near_Side_Foreground_1080x1920_transparent.png";

        const float SpritePlaneZ = 0f;

        [Header("References")]
        [SerializeField] Transform backgroundRoot;
        [SerializeField] Transform gameplayRoot;
        [SerializeField] Transform foregroundRoot;
        [SerializeField] Camera targetCamera;

        [Header("Background art")]
        [SerializeField] Sprite farBackgroundSprite;
        [SerializeField] Sprite midBackgroundSprite;

        [Header("Near foreground")]
        [SerializeField] Sprite nearSideForegroundSprite;
        [SerializeField] bool enableNearForeground = true;
        [SerializeField] [Range(0f, 1f)] float nearForegroundAlpha = 0.9f;
        [SerializeField] float nearForegroundScaleMultiplier = 1f;
        [SerializeField] Vector2 nearForegroundOffset;

        [Header("Parallax tuning")]
        [SerializeField] Vector2 farParallaxFactor = new(0.05f, 0.10f);
        [SerializeField] Vector2 midParallaxFactor = new(0.12f, 0.20f);
        [SerializeField] Vector2 backRockParallaxFactor = new(0.18f, 0.28f);

        [Header("Gameplay corridor (editor guide)")]
        [SerializeField] [Range(0.3f, 1f)] float safeCorridorWidthPercent = 0.62f;
        [SerializeField] bool showSafeCorridorInEditorOnly;

        [Header("Debug")]
        [SerializeField] bool logVisualSetupOnStart = true;
        [SerializeField] bool enableVisualDebugFallback;

        static Sprite placeholderSprite;
        static Sprite debugFarSprite;
        static Sprite debugNearSprite;
        bool startupLogged;

        void Awake()
        {
            ResolveSpritesFromPaths();
            if (!ResolveTargetCamera())
                return;

            OrganizeHierarchy();
            EnsureBackgroundLayers();
            EnsureNearForeground();
        }

        void Start()
        {
            ReapplyAllSortingLayers();

            if (logVisualSetupOnStart && !startupLogged)
                LogVisualSetupReport("Start");

            VisualSortingLayerValidator.ValidateOnce(transform);
        }

        void ReapplyAllSortingLayers()
        {
            if (backgroundRoot == null)
                return;

            ReapplyLayerSorting(backgroundRoot, FarBackgroundName, VisualSortingLayers.BackgroundFar, 0,
                VisualLayerSortingUtility.FallbackFarOrder);
            ReapplyLayerSorting(backgroundRoot, "MidBackground", VisualSortingLayers.BackgroundMid, 0,
                VisualLayerSortingUtility.FallbackMidOrder);
            ReapplyLayerSorting(backgroundRoot, "BackRockDetails", VisualSortingLayers.BackgroundMid, 5,
                VisualLayerSortingUtility.FallbackMidOrder + 5);

            if (foregroundRoot != null)
            {
                var nearFg = foregroundRoot.Find(NearForegroundName);
                if (nearFg != null)
                {
                    var renderer = nearFg.GetComponent<SpriteRenderer>();
                    VisualLayerSortingUtility.Apply(renderer, VisualSortingLayers.Foreground, 20,
                        VisualLayerSortingUtility.FallbackForegroundOrder);
                }
            }
        }

        static void ReapplyLayerSorting(Transform parent, string childName, string layerName, int order, int fallbackOrder)
        {
            if (parent == null)
                return;

            var child = parent.Find(childName);
            if (child == null)
                return;

            var renderer = child.GetComponent<SpriteRenderer>();
            VisualLayerSortingUtility.Apply(renderer, layerName, order, fallbackOrder);
        }

        [ContextMenu("Validate Visual Layers Now")]
        public void ValidateVisualLayersNow()
        {
            ResolveSpritesFromPaths();
            ResolveTargetCamera();
            LogVisualSetupReport("ValidateVisualLayersNow");
        }

        bool ResolveTargetCamera()
        {
            if (targetCamera != null && targetCamera.enabled && targetCamera.orthographic)
                return true;

            if (targetCamera != null && !targetCamera.orthographic)
            {
                Debug.LogWarning(
                    $"[Tar&Tulla][Visual] Assigned camera '{targetCamera.name}' is not orthographic — searching for gameplay camera.",
                    this);
                targetCamera = null;
            }

            targetCamera = Camera.main;

            if (targetCamera == null || !targetCamera.orthographic)
            {
                var cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
                for (int i = 0; i < cameras.Length; i++)
                {
                    var cam = cameras[i];
                    if (cam != null && cam.enabled && cam.orthographic)
                    {
                        targetCamera = cam;
                        break;
                    }
                }
            }

            if (targetCamera == null)
            {
                Debug.LogError("[Tar&Tulla][Visual] No valid orthographic camera found for visual layers.", this);
                return false;
            }

            return true;
        }

        void ResolveSpritesFromPaths()
        {
#if UNITY_EDITOR
            farBackgroundSprite ??= LoadSpriteAtPath(FarBackgroundPath);
            midBackgroundSprite ??= LoadSpriteAtPath(MidBackgroundPath);
            nearSideForegroundSprite ??= LoadSpriteAtPath(NearForegroundPath);
#endif
        }

#if UNITY_EDITOR
        static Sprite LoadSpriteAtPath(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }
#endif

        void OrganizeHierarchy()
        {
            var gameRoot = transform;
            backgroundRoot = EnsureChildRoot(gameRoot, backgroundRoot, BackgroundRootName, 0);
            gameplayRoot = EnsureChildRoot(gameRoot, gameplayRoot, GameplayRootName, 1);
            foregroundRoot = EnsureChildRoot(gameRoot, foregroundRoot, ForegroundRootName, 3);

            ReparentLegacyRoot(gameRoot, "LevelRoot", gameplayRoot, PlatformsName);
            ReparentLegacyRoot(gameRoot, "CharactersRoot", gameplayRoot, JumpersName);
            ReparentLegacyRoot(gameRoot, PlatformsName, gameplayRoot, PlatformsName);
            ReparentLegacyRoot(gameRoot, JumpersName, gameplayRoot, JumpersName);
            ReparentRope(gameRoot);
        }

        static Transform EnsureChildRoot(Transform gameRoot, Transform existing, string name, int siblingIndex)
        {
            if (existing == null)
                existing = gameRoot.Find(name);

            if (existing == null)
            {
                var go = new GameObject(name);
                existing = go.transform;
                existing.SetParent(gameRoot, false);
            }

            existing.SetSiblingIndex(siblingIndex);
            return existing;
        }

        static void ReparentLegacyRoot(Transform gameRoot, string legacyName, Transform gameplayRootTransform, string newName)
        {
            var legacy = gameRoot.Find(legacyName);
            if (legacy == null)
            {
                if (gameplayRootTransform.Find(newName) == null)
                {
                    var created = new GameObject(newName);
                    created.transform.SetParent(gameplayRootTransform, false);
                }

                return;
            }

            if (legacy.parent != gameplayRootTransform)
                legacy.SetParent(gameplayRootTransform, true);

            legacy.name = newName;
        }

        static void ReparentRope(Transform gameRoot)
        {
            var gameplay = gameRoot.Find(GameplayRootName);
            if (gameplay == null)
                return;

            Transform ropeRoot = gameplay.Find(RopeName);
            if (ropeRoot == null)
            {
                var ropeGo = new GameObject(RopeName);
                ropeRoot = ropeGo.transform;
                ropeRoot.SetParent(gameplay, false);
            }

            var systems = gameRoot.Find("Systems");
            if (systems == null)
                return;

            var elasticRope = systems.Find("ElasticRope");
            if (elasticRope != null && elasticRope.parent != ropeRoot)
                elasticRope.SetParent(ropeRoot, true);
        }

        void EnsureBackgroundLayers()
        {
            if (farBackgroundSprite == null)
                Debug.LogWarning("[Tar&Tulla][Visual] Far background sprite is not assigned on SceneVisualLayerSetup.", this);

            CreateBackgroundLayer(FarBackgroundName, backgroundRoot, VisualSortingLayers.BackgroundFar, 0,
                new Color(0.07f, 0.06f, 0.10f, 1f), farParallaxFactor, farBackgroundSprite,
                enableVisualDebugFallback ? new Color(0.85f, 0.1f, 0.85f, 1f) : (Color?)null);

            CreateBackgroundLayer("MidBackground", backgroundRoot, VisualSortingLayers.BackgroundMid, 0,
                new Color(0.09f, 0.07f, 0.12f, 0.45f), midParallaxFactor, midBackgroundSprite,
                enableVisualDebugFallback ? new Color(0.1f, 0.6f, 0.2f, 1f) : (Color?)null);

            CreateBackgroundLayer("BackRockDetails", backgroundRoot, VisualSortingLayers.BackgroundMid, 5,
                new Color(0.11f, 0.08f, 0.14f, 0.28f), backRockParallaxFactor, null,
                enableVisualDebugFallback ? new Color(0.9f, 0.6f, 0.1f, 1f) : (Color?)null);
        }

        void CreateBackgroundLayer(string name, Transform parent, string sortingLayer, int sortingOrder,
            Color placeholderColor, Vector2 parallaxFactor, Sprite artSprite, Color? debugFallbackColor)
        {
            if (parent == null)
                return;

            var existing = parent.Find(name);
            GameObject layerGo = existing != null ? existing.gameObject : new GameObject(name);
            if (existing == null)
                layerGo.transform.SetParent(parent, false);

            layerGo.SetActive(true);

            var renderer = layerGo.GetComponent<SpriteRenderer>();
            if (renderer == null)
                renderer = layerGo.AddComponent<SpriteRenderer>();

            placeholderSprite ??= CreatePlaceholderSprite();

            // Debug fallback forces a bright flat color regardless of art, so we can verify
            // the layer renders at the correct position / scale / sorting in Play Mode.
            bool forceDebug = enableVisualDebugFallback && debugFallbackColor.HasValue;
            Sprite spriteToUse = forceDebug ? null : artSprite;

            if (forceDebug)
            {
                debugFarSprite = CreateDebugSprite(debugFallbackColor.Value);
                renderer.sprite = debugFarSprite;
                renderer.color = debugFallbackColor.Value;
                var cover = GetBackgroundCoverScale();
                layerGo.transform.localScale = new Vector3(cover.x, cover.y, 1f);
            }
            else if (spriteToUse != null)
            {
                renderer.sprite = spriteToUse;
                renderer.color = Color.white;
                ApplySpriteCoverScale(layerGo.transform, spriteToUse);
            }
            else
            {
                renderer.sprite = placeholderSprite;
                renderer.color = placeholderColor;
                var cover = GetBackgroundCoverScale();
                layerGo.transform.localScale = new Vector3(cover.x, cover.y, 1f);
            }

            VisualLayerSortingUtility.Apply(renderer, sortingLayer, sortingOrder,
                sortingLayer == VisualSortingLayers.BackgroundFar
                    ? VisualLayerSortingUtility.FallbackFarOrder
                    : VisualLayerSortingUtility.FallbackMidOrder);
            VisualLayerSortingUtility.ApplyUnlitMaterial(renderer);

            SyncLayerToCamera(layerGo.transform);

            var parallax = layerGo.GetComponent<ParallaxLayer2D>();
            if (parallax == null)
                parallax = layerGo.AddComponent<ParallaxLayer2D>();

            var followFactor = ToFullscreenFollowFactor(parallaxFactor);
            parallax.Configure(
                targetCamera != null ? targetCamera.transform : null,
                followFactor,
                Vector2.zero,
                smooth: false,
                lockX: true,
                lockY: true);
        }

        void EnsureNearForeground()
        {
            if (foregroundRoot == null)
                return;

            if (nearSideForegroundSprite == null)
                Debug.LogWarning("[Tar&Tulla][Visual] Near foreground sprite is not assigned on SceneVisualLayerSetup.", this);

            var existing = foregroundRoot.Find(NearForegroundName);
            GameObject fgGo = existing != null ? existing.gameObject : new GameObject(NearForegroundName);
            if (existing == null)
                fgGo.transform.SetParent(foregroundRoot, false);

            fgGo.SetActive(true);

            var renderer = fgGo.GetComponent<SpriteRenderer>();
            if (renderer == null)
                renderer = fgGo.AddComponent<SpriteRenderer>();

            Sprite spriteToUse = nearSideForegroundSprite;
            bool usingDebugSprite = false;

            if (spriteToUse == null && enableVisualDebugFallback)
            {
                debugNearSprite ??= CreateDebugFrameSprite();
                spriteToUse = debugNearSprite;
                usingDebugSprite = true;
            }

            if (spriteToUse != null)
                renderer.sprite = spriteToUse;

            VisualLayerSortingUtility.Apply(renderer, VisualSortingLayers.Foreground, 20,
                VisualLayerSortingUtility.FallbackForegroundOrder);
            VisualLayerSortingUtility.ApplyUnlitMaterial(renderer);

            renderer.color = usingDebugSprite
                ? new Color(0.1f, 0.95f, 1f, 0.85f)
                : new Color(1f, 1f, 1f, nearForegroundAlpha);

            var locked = fgGo.GetComponent<CameraLockedSprite2D>();
            if (locked == null)
                locked = fgGo.AddComponent<CameraLockedSprite2D>();

            locked.SetTargetCamera(targetCamera);
            locked.ConfigureVisual(
                usingDebugSprite ? 0.85f : nearForegroundAlpha,
                nearForegroundScaleMultiplier,
                nearForegroundOffset,
                logOnce: logVisualSetupOnStart);

            bool showVisual = enableNearForeground && (spriteToUse != null || enableVisualDebugFallback);
            locked.SetEnabledVisual(showVisual);
            locked.SnapToCameraNow();

            var collider2D = fgGo.GetComponent<Collider2D>();
            if (collider2D != null)
                Destroy(collider2D);
        }

        void SyncLayerToCamera(Transform layerTransform)
        {
            if (targetCamera == null || layerTransform == null)
                return;

            var camPos = targetCamera.transform.position;
            layerTransform.position = new Vector3(camPos.x, camPos.y, SpritePlaneZ);
        }

        void ApplySpriteCoverScale(Transform layerTransform, Sprite sprite, float bleed = 1.02f)
        {
            if (targetCamera == null || sprite == null)
                return;

            float worldHeight = targetCamera.orthographicSize * 2f;
            float worldWidth = worldHeight * GetViewportAspect(targetCamera);
            var bounds = sprite.bounds.size;
            float bw = Mathf.Max(0.001f, bounds.x);
            float bh = Mathf.Max(0.001f, bounds.y);
            float cover = Mathf.Max(worldWidth / bw, worldHeight / bh) * bleed;

            if (!float.IsFinite(cover) || cover <= 0f)
                cover = 1f;

            layerTransform.localScale = new Vector3(cover, cover, 1f);
        }

        Vector2 GetBackgroundCoverScale()
        {
            if (targetCamera == null)
                return new Vector2(12f, 22f);

            float height = targetCamera.orthographicSize * 2f;
            float width = height * GetViewportAspect(targetCamera);
            return new Vector2(width, height);
        }

        static float GetViewportAspect(Camera camera)
        {
            if (camera.pixelWidth > 0 && camera.pixelHeight > 0)
                return (float)camera.pixelWidth / camera.pixelHeight;

            return Mathf.Max(0.01f, camera.aspect);
        }

        static Vector2 ToFullscreenFollowFactor(Vector2 depthFactor)
        {
            const float followWeight = 0.94f;
            return new Vector2(
                Mathf.Lerp(depthFactor.x, 1f, followWeight),
                Mathf.Lerp(depthFactor.y, 1f, followWeight));
        }

        static Sprite CreatePlaceholderSprite()
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

        static Sprite CreateDebugSprite(Color color)
        {
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        }

        static Sprite CreateDebugFrameSprite()
        {
            const int w = 64;
            const int h = 96;
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var clear = new Color(0f, 0f, 0f, 0f);
            var edge = new Color(0.1f, 0.95f, 1f, 0.9f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool border = x < 6 || x >= w - 6 || y < 6 || y >= h - 6;
                    texture.SetPixel(x, y, border ? edge : clear);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 8f);
        }

        void LogVisualSetupReport(string source)
        {
            startupLogged = true;

            Debug.Log(
                $"[Tar&Tulla][Visual][{source}] SceneVisualLayerSetup enabled={enabled}, " +
                $"gameObject active={gameObject.activeInHierarchy}",
                this);

            if (targetCamera == null)
            {
                Debug.LogError($"[Tar&Tulla][Visual][{source}] targetCamera is null.", this);
                return;
            }

            float aspect = GetViewportAspect(targetCamera);
            Debug.Log(
                $"[Tar&Tulla][Visual][{source}] Camera '{targetCamera.name}': pos={targetCamera.transform.position}, " +
                $"orthoSize={targetCamera.orthographicSize:F2}, aspect={aspect:F3}, " +
                $"worldSize=({targetCamera.orthographicSize * 2f * aspect:F2} x {targetCamera.orthographicSize * 2f:F2})",
                this);

            LogRoot(source, BackgroundRootName, backgroundRoot);
            LogRoot(source, GameplayRootName, gameplayRoot);
            LogRoot(source, ForegroundRootName, foregroundRoot);

            Debug.Log(
                $"[Tar&Tulla][Visual][{source}] farBackgroundSprite={(farBackgroundSprite != null ? farBackgroundSprite.name : "MISSING")}, " +
                $"midBackgroundSprite={(midBackgroundSprite != null ? midBackgroundSprite.name : "MISSING")}, " +
                $"nearSideForegroundSprite={(nearSideForegroundSprite != null ? nearSideForegroundSprite.name : "MISSING")}, " +
                $"enableVisualDebugFallback={enableVisualDebugFallback}",
                this);

            LogLayerObject(source, backgroundRoot, FarBackgroundName);
            LogLayerObject(source, foregroundRoot, NearForegroundName);
        }

        static void LogRoot(string source, string name, Transform root)
        {
            if (root == null)
            {
                Debug.LogWarning($"[Tar&Tulla][Visual][{source}] {name}: NOT FOUND");
                return;
            }

            Debug.Log(
                $"[Tar&Tulla][Visual][{source}] {name}: found, activeSelf={root.gameObject.activeSelf}, " +
                $"activeInHierarchy={root.gameObject.activeInHierarchy}, childCount={root.childCount}");
        }

        void LogLayerObject(string source, Transform parent, string childName)
        {
            if (parent == null)
                return;

            var child = parent.Find(childName);
            if (child == null)
            {
                Debug.LogWarning($"[Tar&Tulla][Visual][{source}] {childName}: NOT FOUND under {parent.name}");
                return;
            }

            var renderer = child.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                Debug.LogWarning($"[Tar&Tulla][Visual][{source}] {childName}: missing SpriteRenderer");
                return;
            }

            var sprite = renderer.sprite;
            var bounds = sprite != null ? sprite.bounds.size : Vector3.zero;
            float camH = targetCamera != null ? targetCamera.orthographicSize * 2f : 0f;
            float camW = targetCamera != null ? camH * GetViewportAspect(targetCamera) : 0f;

            Debug.Log(
                $"[Tar&Tulla][Visual][{source}] {childName}: sprite={(sprite != null ? sprite.name : "NULL")}, " +
                $"shader={(renderer.sharedMaterial != null ? renderer.sharedMaterial.shader.name : "NULL")}, " +
                $"sortingLayer={renderer.sortingLayerName}({renderer.sortingOrder}), " +
                $"color={renderer.color}, alpha={renderer.color.a:F2}, " +
                $"pos={child.position}, localScale={child.localScale}, " +
                $"spriteBounds=({bounds.x:F2},{bounds.y:F2}), cam=({camW:F2}x{camH:F2}), " +
                $"renderer.enabled={renderer.enabled}, activeInHierarchy={child.gameObject.activeInHierarchy}",
                child);
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!showSafeCorridorInEditorOnly)
                return;

            DrawSafeCorridorGizmo();
        }

        void DrawSafeCorridorGizmo()
        {
            var cam = targetCamera != null ? targetCamera : Camera.main;
            if (cam == null || !cam.orthographic)
                return;

            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * GetViewportAspect(cam);
            float corridorHalfWidth = halfWidth * safeCorridorWidthPercent * 0.5f;

            Vector3 center = cam.transform.position;
            float top = center.y + halfHeight;
            float bottom = center.y - halfHeight;
            float left = center.x - corridorHalfWidth;
            float right = center.x + corridorHalfWidth;

            Gizmos.color = new Color(0.25f, 0.95f, 0.55f, 0.85f);
            Gizmos.DrawLine(new Vector3(left, bottom, 0f), new Vector3(left, top, 0f));
            Gizmos.DrawLine(new Vector3(right, bottom, 0f), new Vector3(right, top, 0f));
            Gizmos.DrawLine(new Vector3(left, top, 0f), new Vector3(right, top, 0f));
            Gizmos.DrawLine(new Vector3(left, bottom, 0f), new Vector3(right, bottom, 0f));
        }
#endif
    }
}
