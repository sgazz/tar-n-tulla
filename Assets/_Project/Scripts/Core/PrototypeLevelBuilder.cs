using System.Collections.Generic;
using UnityEngine;
using TarTulla.CameraSystems;
using TarTulla.Characters;
using TarTulla.Game;
using TarTulla.Input;
using TarTulla.Platforms;
using TarTulla.Rope;

namespace TarTulla.Core
{
    [DefaultExecutionOrder(-80)]
    public class PrototypeLevelBuilder : MonoBehaviour
    {
        const string TarObjectName = "Tar";
        const string TullaObjectName = "Tulla";
        const string RopeObjectName = "ElasticRope";
        const string TiltInputObjectName = "MobileTiltInput";

        [SerializeField] Transform charactersRoot;
        [SerializeField] Transform levelRoot;
        [SerializeField] Transform systemsRoot;
        [SerializeField] CharacterSettings characterSettings;
        [SerializeField] RopeSettings ropeSettings;
        [SerializeField] AirControlSettings airControlSettings;
        [SerializeField] VerticalCameraFollow2D cameraFollow;
        [SerializeField] bool buildOnPlay;
        [SerializeField] bool enableStreamDebugLogs;

        [Header("Generation Fallback")]
        [SerializeField] int platformCount = 24;
        [SerializeField] int generationSeed = 1337;
        [SerializeField] float startY = -2f;
        [SerializeField] float verticalSpacingMin = 2.4f;
        [SerializeField] float verticalSpacingMax = 3.2f;
        [SerializeField] float horizontalRange = 2f;
        [SerializeField] float platformWidth = 3.2f;
        [SerializeField] float platformHeight = 0.3f;

        [Header("Jumpers")]
        [SerializeField] float jumperRadius = 0.35f;
        [SerializeField] float jumperSpawnHeightAboveStart = 1.15f;

        readonly List<GameObject> generatedPlatforms = new();

        PlatformGenerationSettings activeSettings;
        int activeSeed;
        float highestGeneratedY;
        Vector2 lastPlatformPosition;
        int lastHorizontalDirection = 1;
        int generatedPlatformIndex;
        float previousPlatformWidth;

        public Transform TarTransform { get; private set; }
        public Transform TullaTransform { get; private set; }
        public float StartBaselineY => GetPlatformSettings().startY;
        public float HighestGeneratedY => highestGeneratedY;
        public int ActivePlatformCount => generatedPlatforms.Count;
        public bool UsesProceduralGeneration => activeSettings.useProceduralGeneration;

        void Awake()
        {
            ResolveRoots();
            activeSettings = GetPlatformSettings();

            if (buildOnPlay)
                BuildPrototypeLayout();
        }

        [ContextMenu("Build Prototype Layout")]
        public void BuildPrototypeLayout()
        {
            ResolveRoots();

            if (!TarTullaTuningAccess.HasActiveProfile)
            {
                if (characterSettings == null || ropeSettings == null)
                {
                    Debug.LogError("[Tar&Tulla] PrototypeLevelBuilder: Missing CharacterSettings or RopeSettings.");
                    return;
                }

                if (airControlSettings == null)
                {
                    Debug.LogError("[Tar&Tulla] PrototypeLevelBuilder: Missing AirControlSettings.");
                    return;
                }
            }

            ClearGeneratedContent();
            activeSettings = GetPlatformSettings();
            BeginGeneratorRun(activeSettings);

            int initialCount = activeSettings.useProceduralGeneration
                ? activeSettings.initialPlatformCount
                : activeSettings.platformCount;

            GeneratePlatformCount(initialCount);

            var tarStart = new Vector2(-0.75f, activeSettings.startY + jumperSpawnHeightAboveStart);
            var tullaStart = new Vector2(0.75f, activeSettings.startY + jumperSpawnHeightAboveStart);

            var tiltInput = EnsureMobileTiltInput();
            var tar = CreateJumper(TarObjectName, tarStart, new Color(0.35f, 0.75f, 0.95f), tiltInput, activeSettings.gravityScale);
            var tulla = CreateJumper(TullaObjectName, tullaStart, new Color(0.95f, 0.55f, 0.35f), tiltInput, activeSettings.gravityScale);

            TarTransform = tar.transform;
            TullaTransform = tulla.transform;

            CreateRope(tar, tulla);
            WireCamera(tar.transform, tulla.transform);

            LogStream(
                $"mode={(activeSettings.useProceduralGeneration ? "procedural" : "fixed")}, seed={activeSeed}, " +
                $"generated={generatedPlatformIndex}, highestY={highestGeneratedY:F1}, active={generatedPlatforms.Count}");

            if (enableStreamDebugLogs)
            {
                var sampleBounds = ResolveHorizontalBounds(activeSettings, activeSettings.platformWidth);
                LogStream(
                    $"playfield camHalfW={sampleBounds.cameraHalfWidth:F2}, margin={activeSettings.screenHorizontalMargin:F2}, " +
                    $"effectiveHalfRange={sampleBounds.effectiveHalfRange:F2}, platformW={activeSettings.platformWidth:F2}");
            }

            Debug.Log($"[Tar&Tulla] Prototype layout built ({generatedPlatformIndex} platforms, seed {activeSeed}).");
        }

        public void EnsurePlatformsAhead(float referenceY)
        {
            if (!activeSettings.useProceduralGeneration)
                return;

            float targetY = referenceY + activeSettings.platformBufferAhead;
            int placed = 0;

            while (highestGeneratedY < targetY && generatedPlatforms.Count < activeSettings.maxActivePlatforms)
            {
                float segmentCeiling = highestGeneratedY + activeSettings.generationSegmentHeight;
                float batchTarget = Mathf.Min(targetY, segmentCeiling);

                while (highestGeneratedY < batchTarget && generatedPlatforms.Count < activeSettings.maxActivePlatforms)
                {
                    PlaceNextPlatform();
                    placed++;
                }

                if (highestGeneratedY >= targetY)
                    break;

                if (placed == 0)
                    break;
            }

            if (enableStreamDebugLogs && placed > 0)
            {
                LogStream(
                    $"EnsureAhead refY={referenceY:F1}, placed={placed}, highestY={highestGeneratedY:F1}, active={generatedPlatforms.Count}");
            }
        }

        public int CleanupOldPlatforms(float cameraY)
        {
            if (!activeSettings.useProceduralGeneration)
                return 0;

            float threshold = cameraY - activeSettings.cleanupDistanceBelowCamera;
            int removed = 0;

            for (int i = generatedPlatforms.Count - 1; i >= 0; i--)
            {
                var platform = generatedPlatforms[i];
                if (platform == null)
                {
                    generatedPlatforms.RemoveAt(i);
                    continue;
                }

                if (platform.transform.position.y >= threshold)
                    continue;

                DestroyImmediateSafe(platform);
                generatedPlatforms.RemoveAt(i);
                removed++;
            }

            if (enableStreamDebugLogs && removed > 0)
                LogStream($"Cleanup cameraY={cameraY:F1}, removed={removed}, active={generatedPlatforms.Count}");

            return removed;
        }

        public void ClearGeneratedLayout()
        {
            DestroyChildren(levelRoot);
            generatedPlatforms.Clear();
            ResetGeneratorState();
        }

        public void ClearGeneratedContent()
        {
            DestroyChildren(charactersRoot);
            ClearGeneratedLayout();

            if (systemsRoot == null)
                return;

            DestroyIfExists(RopeObjectName);
        }

        void BeginGeneratorRun(PlatformGenerationSettings settings)
        {
            activeSettings = settings;
            activeSeed = settings.randomizeSeedOnRun ? Random.Range(1, int.MaxValue) : settings.seed;
            ResetGeneratorState();
            Random.InitState(activeSeed);
        }

        void ResetGeneratorState()
        {
            generatedPlatforms.Clear();
            generatedPlatformIndex = 0;
            lastHorizontalDirection = 1;
            previousPlatformWidth = activeSettings.platformWidth * activeSettings.safeLandingWidthMultiplier;
            highestGeneratedY = activeSettings.startY;
            lastPlatformPosition = new Vector2(0f, activeSettings.startY);
        }

        void GeneratePlatformCount(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (generatedPlatforms.Count >= activeSettings.maxActivePlatforms)
                    break;

                PlaceNextPlatform();
            }
        }

        void PlaceNextPlatform()
        {
            var s = activeSettings;
            int index = generatedPlatformIndex;
            bool isEasy = index < s.easyStartPlatformCount;
            int recoveryEvery = s.recoveryPlatformEvery > 0 ? s.recoveryPlatformEvery : s.wideRecoveryPlatformEvery;
            bool isRecovery = recoveryEvery > 0 && index > 0 && index % recoveryEvery == 0;

            float currentX = lastPlatformPosition.x;
            float currentY = lastPlatformPosition.y;

            if (index > 0)
            {
                float difficulty = GetDifficultyFactor(currentY);
                float spacingMin = isEasy ? s.minVerticalGap : s.verticalSpacingMin;
                float spacingMax = isEasy
                    ? Mathf.Min(s.verticalSpacingMin, s.maxVerticalGap)
                    : Mathf.Lerp(s.verticalSpacingMax, s.maxVerticalSpacingAtHighDifficulty, difficulty);

                float spacing = Random.Range(spacingMin, spacingMax);
                spacing = Mathf.Clamp(spacing, s.minVerticalGap, s.maxVerticalGap);
                currentY += spacing;
            }
            else
            {
                currentX = 0f;
                currentY = s.startY;
            }

            float width = ResolvePlatformWidth(s, index, isEasy, isRecovery, currentY);

            if (index > 0)
            {
                if (isEasy)
                {
                    float step = Random.Range(s.maxHorizontalGap * 0.15f, s.maxHorizontalGap * 0.45f);
                    currentX = Mathf.Clamp(
                        currentX + lastHorizontalDirection * step,
                        -s.horizontalRange * 0.5f,
                        s.horizontalRange * 0.5f);
                }
                else
                {
                    if (s.forceAlternatingPattern)
                        lastHorizontalDirection *= -1;
                    else if (Random.value < s.horizontalDirectionChangeChance)
                        lastHorizontalDirection = Random.value < 0.5f ? -1 : 1;

                    float step = Random.Range(s.maxHorizontalGap * 0.35f, s.maxHorizontalGap);
                    float proposedX = currentX + lastHorizontalDirection * step;
                    float maxCenterDelta = Mathf.Max(s.maxHorizontalGap, (width + previousPlatformWidth) * 0.5f);
                    proposedX = Mathf.Clamp(proposedX, currentX - maxCenterDelta, currentX + maxCenterDelta);
                    currentX = Mathf.Clamp(proposedX, -s.horizontalRange, s.horizontalRange);
                }
            }

            var bounds = ResolveHorizontalBounds(s, width);
            if (s.clampPlatformsToVisibleWidth)
            {
                float clampedX = Mathf.Clamp(currentX, -bounds.effectiveHalfRange, bounds.effectiveHalfRange);
                if (enableStreamDebugLogs && generatedPlatformIndex < 3)
                {
                    LogStream(
                        $"clamp idx={index} camHalfW={bounds.cameraHalfWidth:F2}, platformW={width:F2}, " +
                        $"margin={s.screenHorizontalMargin:F2}, effectiveHalf={bounds.effectiveHalfRange:F2}, x={currentX:F2}->{clampedX:F2}");
                }

                currentX = clampedX;
            }

            var position = new Vector2(currentX, currentY);
            var platform = CreatePlatform($"Platform_{index + 1}", position, new Vector2(width, s.platformHeight), s);
            generatedPlatforms.Add(platform);

            lastPlatformPosition = position;
            highestGeneratedY = currentY;
            previousPlatformWidth = width;
            generatedPlatformIndex++;
        }

        float ResolvePlatformWidth(PlatformGenerationSettings s, int index, bool isEasy, bool isRecovery, float platformY)
        {
            if (index == 0)
                return s.platformWidth * s.safeLandingWidthMultiplier;

            if (isRecovery)
            {
                float baseWidth = s.widthVariationEnabled ? s.platformWidthMax : s.platformWidth;
                return baseWidth * s.recoveryPlatformWidthMultiplier;
            }

            if (isEasy)
                return s.platformWidth * s.safeLandingWidthMultiplier;

            float width;
            if (!s.widthVariationEnabled)
                width = s.platformWidth;
            else if (Random.value < s.narrowPlatformChance)
                width = s.platformWidthMin;
            else
                width = Random.Range(s.platformWidthMin, s.platformWidthMax);

            if (s.difficultyRampEnabled)
            {
                float difficulty = GetDifficultyFactor(platformY);
                width = Mathf.Lerp(width, s.minPlatformWidthAtHighDifficulty, difficulty);
            }

            return width;
        }

        float GetDifficultyFactor(float platformY)
        {
            if (!activeSettings.difficultyRampEnabled)
                return 0f;

            float heightAboveStart = platformY - activeSettings.startY;
            if (heightAboveStart <= activeSettings.difficultyRampStartHeight)
                return 0f;

            float rampRange = Mathf.Max(40f, activeSettings.difficultyRampStartHeight * 2f);
            float normalized = (heightAboveStart - activeSettings.difficultyRampStartHeight) / rampRange;
            return Mathf.Clamp01(normalized * activeSettings.difficultyRampStrength);
        }

        float ResolveCameraHalfWidth(PlatformGenerationSettings settings)
        {
            if (settings.useCameraBasedHorizontalBounds && Camera.main != null)
                return Camera.main.orthographicSize * Camera.main.aspect;

            return settings.manualHalfWidthFallback;
        }

        HorizontalPlayfieldBounds ResolveHorizontalBounds(PlatformGenerationSettings settings, float platformWidth)
        {
            float cameraHalfWidth = ResolveCameraHalfWidth(settings);
            float platformHalfWidth = platformWidth * 0.5f;
            float allowedHalfX = Mathf.Max(0f, cameraHalfWidth - platformHalfWidth - settings.screenHorizontalMargin);
            float effectiveHalfRange = settings.clampPlatformsToVisibleWidth
                ? Mathf.Min(settings.horizontalRange, allowedHalfX)
                : settings.horizontalRange;

            return new HorizontalPlayfieldBounds
            {
                cameraHalfWidth = cameraHalfWidth,
                allowedHalfX = allowedHalfX,
                effectiveHalfRange = effectiveHalfRange
            };
        }

        void OnDrawGizmos()
        {
            var settings = Application.isPlaying ? activeSettings : GetPlatformSettings();
            if (!settings.drawPlayfieldBoundsGizmos)
                return;

            float cameraHalfWidth = ResolveCameraHalfWidth(settings);
            float left = -cameraHalfWidth + settings.screenHorizontalMargin;
            float right = cameraHalfWidth - settings.screenHorizontalMargin;
            float yMin = settings.startY - 2f;
            float yMax = Application.isPlaying ? highestGeneratedY + 12f : settings.startY + 32f;

            Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.9f);
            Gizmos.DrawLine(new Vector3(left, yMin, 0f), new Vector3(left, yMax, 0f));
            Gizmos.DrawLine(new Vector3(right, yMin, 0f), new Vector3(right, yMax, 0f));
        }

        GameObject CreatePlatform(string name, Vector2 position, Vector2 size, PlatformGenerationSettings settings)
        {
            var platform = new GameObject(name);
            platform.transform.SetParent(levelRoot, false);
            platform.transform.position = position;
            platform.layer = LayerMask.NameToLayer("Default");

            var renderer = platform.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateRectSprite();
            renderer.color = new Color(0.55f, 0.58f, 0.62f);
            ApplyGameplaySorting(renderer);
            platform.transform.localScale = new Vector3(size.x, size.y, 1f);

            var collider = platform.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            collider.isTrigger = false;
            collider.usedByEffector = settings.useOneWayPlatforms;

            var rb = platform.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;

            var basicPlatform = platform.AddComponent<BasicPlatform>();
            basicPlatform.Configure(settings.useOneWayPlatforms, settings.oneWaySurfaceArc);
            return platform;
        }

        PlatformGenerationSettings GetPlatformSettings()
        {
            var profile = TarTullaTuningAccess.GetActiveProfile();
            if (profile != null)
                return PlatformGenerationSettings.FromProfile(profile);

            return PlatformGenerationSettings.CreateFallback(
                platformCount, generationSeed, startY, verticalSpacingMin, verticalSpacingMax,
                horizontalRange, platformWidth, platformHeight);
        }

        void LogStream(string message)
        {
            if (!enableStreamDebugLogs)
                return;

            Debug.Log($"[Tar&Tulla][Builder][Stream] {message}", this);
        }

        void ResolveRoots()
        {
            if (charactersRoot != null && levelRoot != null && systemsRoot != null)
                return;

            var bootstrap = FindAnyObjectByType<GameBootstrap>();
            if (bootstrap == null)
                return;

            var gameRoot = bootstrap.transform;
            var gameplayRoot = gameRoot.Find("GameplayRoot");
            if (gameplayRoot != null)
            {
                charactersRoot ??= gameplayRoot.Find("Jumpers");
                levelRoot ??= gameplayRoot.Find("Platforms");
            }

            charactersRoot ??= gameRoot.Find("CharactersRoot");
            levelRoot ??= gameRoot.Find("LevelRoot");
            systemsRoot ??= gameRoot.Find("Systems");

            cameraFollow ??= FindAnyObjectByType<VerticalCameraFollow2D>();
        }

        Transform ResolveRopeRoot()
        {
            var bootstrap = FindAnyObjectByType<GameBootstrap>();
            if (bootstrap != null)
            {
                var gameplayRoot = bootstrap.transform.Find("GameplayRoot");
                var ropeRoot = gameplayRoot != null ? gameplayRoot.Find("Rope") : null;
                if (ropeRoot != null)
                    return ropeRoot;
            }

            return systemsRoot;
        }

        static void ApplyGameplaySorting(SpriteRenderer renderer)
        {
            renderer.sortingLayerName = VisualSortingLayers.Gameplay;
            renderer.sortingOrder = 0;
        }

        JumperController2D CreateJumper(string name, Vector2 position, Color color, MobileTiltInput2D tiltInput, float gravityScale)
        {
            var jumper = new GameObject(name);
            jumper.transform.SetParent(charactersRoot, false);
            jumper.transform.position = position;

            var renderer = jumper.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateCircleSprite();
            renderer.color = color;
            ApplyGameplaySorting(renderer);
            jumper.transform.localScale = Vector3.one * (jumperRadius * 2f);

            var collider = jumper.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;
            collider.isTrigger = false;

            var rb = jumper.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.mass = 1f;
            rb.gravityScale = gravityScale;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.sharedMaterial = CreateJumperPhysicsMaterial();

            var controller = jumper.AddComponent<JumperController2D>();
            controller.Configure(characterSettings, name);

            var airControl = jumper.AddComponent<JumperAirControl2D>();
            airControl.Configure(tiltInput, airControlSettings);

            return controller;
        }

        MobileTiltInput2D EnsureMobileTiltInput()
        {
            var existing = systemsRoot.Find(TiltInputObjectName);
            if (existing != null)
            {
                var existingInput = existing.GetComponent<MobileTiltInput2D>();
                if (existingInput != null)
                {
                    if (airControlSettings != null)
                        existingInput.Configure(airControlSettings);
                    return existingInput;
                }
            }

            var tiltObject = new GameObject(TiltInputObjectName);
            tiltObject.transform.SetParent(systemsRoot, false);

            var tiltInput = tiltObject.AddComponent<MobileTiltInput2D>();
            if (airControlSettings != null)
                tiltInput.Configure(airControlSettings);
            return tiltInput;
        }

        void CreateRope(JumperController2D tar, JumperController2D tulla)
        {
            var ropeObject = new GameObject(RopeObjectName);
            ropeObject.transform.SetParent(ResolveRopeRoot(), false);

            var lineRenderer = ropeObject.AddComponent<LineRenderer>();
            lineRenderer.numCapVertices = 4;

            var rope = ropeObject.AddComponent<ElasticRope2D>();
            rope.Configure(ropeSettings, tar, tulla);
        }

        void WireCamera(Transform tar, Transform tulla)
        {
            if (cameraFollow == null)
                cameraFollow = FindAnyObjectByType<VerticalCameraFollow2D>();

            cameraFollow?.SetTargets(tar, tulla);
        }

        void DestroyIfExists(string objectName)
        {
            if (TryDestroyNamedChild(systemsRoot, objectName))
                return;

            TryDestroyNamedChild(ResolveRopeRoot(), objectName);
        }

        static bool TryDestroyNamedChild(Transform root, string objectName)
        {
            if (root == null)
                return false;

            var existing = root.Find(objectName);
            if (existing == null)
                return false;

            DestroyImmediateSafe(existing.gameObject);
            return true;
        }

        static PhysicsMaterial2D CreateJumperPhysicsMaterial()
        {
            return new PhysicsMaterial2D("JumperMaterial")
            {
                friction = 0.2f,
                bounciness = 0f
            };
        }

        static Sprite CreateCircleSprite()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.5f - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    texture.SetPixel(x, y, dist <= radius ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        static Sprite CreateRectSprite()
        {
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        }

        static void DestroyChildren(Transform root)
        {
            if (root == null)
                return;

            for (int i = root.childCount - 1; i >= 0; i--)
                DestroyImmediateSafe(root.GetChild(i).gameObject);
        }

        static void DestroyImmediateSafe(GameObject target)
        {
            if (target == null)
                return;

            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
        }

        struct PlatformGenerationSettings
        {
            public string profileName;
            public bool useProceduralGeneration;
            public int platformCount;
            public int initialPlatformCount;
            public float platformBufferAhead;
            public float cleanupDistanceBelowCamera;
            public int maxActivePlatforms;
            public float generationSegmentHeight;
            public bool randomizeSeedOnRun;
            public bool difficultyRampEnabled;
            public float difficultyRampStartHeight;
            public float difficultyRampStrength;
            public float minPlatformWidthAtHighDifficulty;
            public float maxVerticalSpacingAtHighDifficulty;
            public int recoveryPlatformEvery;
            public float recoveryPlatformWidthMultiplier;
            public int seed;
            public float startY;
            public float verticalSpacingMin;
            public float verticalSpacingMax;
            public float minVerticalGap;
            public float maxVerticalGap;
            public float horizontalRange;
            public float maxHorizontalGap;
            public float horizontalDirectionChangeChance;
            public bool forceAlternatingPattern;
            public float platformWidth;
            public float platformHeight;
            public bool widthVariationEnabled;
            public float platformWidthMin;
            public float platformWidthMax;
            public float narrowPlatformChance;
            public int wideRecoveryPlatformEvery;
            public int easyStartPlatformCount;
            public float safeLandingWidthMultiplier;
            public bool useOneWayPlatforms;
            public float oneWaySurfaceArc;
            public bool useCameraBasedHorizontalBounds;
            public float screenHorizontalMargin;
            public bool clampPlatformsToVisibleWidth;
            public float manualHalfWidthFallback;
            public bool drawPlayfieldBoundsGizmos;
            public float gravityScale;

            public static PlatformGenerationSettings FromProfile(TarTullaGameplayProfile profile)
            {
                var p = profile.Platforms;
                return new PlatformGenerationSettings
                {
                    profileName = profile.name,
                    useProceduralGeneration = p.useProceduralGeneration,
                    platformCount = p.platformCount,
                    initialPlatformCount = p.initialPlatformCount,
                    platformBufferAhead = p.platformBufferAhead,
                    cleanupDistanceBelowCamera = p.cleanupDistanceBelowCamera,
                    maxActivePlatforms = p.maxActivePlatforms,
                    generationSegmentHeight = p.generationSegmentHeight,
                    randomizeSeedOnRun = p.randomizeSeedOnRun,
                    difficultyRampEnabled = p.difficultyRampEnabled,
                    difficultyRampStartHeight = p.difficultyRampStartHeight,
                    difficultyRampStrength = p.difficultyRampStrength,
                    minPlatformWidthAtHighDifficulty = p.minPlatformWidthAtHighDifficulty,
                    maxVerticalSpacingAtHighDifficulty = p.maxVerticalSpacingAtHighDifficulty,
                    recoveryPlatformEvery = p.recoveryPlatformEvery,
                    recoveryPlatformWidthMultiplier = p.recoveryPlatformWidthMultiplier,
                    seed = p.seed,
                    startY = p.startY,
                    verticalSpacingMin = p.verticalSpacingMin,
                    verticalSpacingMax = p.verticalSpacingMax,
                    minVerticalGap = p.minVerticalGap,
                    maxVerticalGap = p.maxVerticalGap,
                    horizontalRange = p.horizontalRange,
                    maxHorizontalGap = p.maxHorizontalGap,
                    horizontalDirectionChangeChance = p.horizontalDirectionChangeChance,
                    forceAlternatingPattern = p.forceAlternatingPattern,
                    platformWidth = p.platformWidth,
                    platformHeight = p.platformHeight,
                    widthVariationEnabled = p.widthVariationEnabled,
                    platformWidthMin = p.platformWidthMin,
                    platformWidthMax = p.platformWidthMax,
                    narrowPlatformChance = p.narrowPlatformChance,
                    wideRecoveryPlatformEvery = p.wideRecoveryPlatformEvery,
                    easyStartPlatformCount = p.easyStartPlatformCount,
                    safeLandingWidthMultiplier = p.safeLandingWidthMultiplier,
                    useOneWayPlatforms = p.useOneWayPlatforms,
                    oneWaySurfaceArc = p.oneWaySurfaceArc,
                    useCameraBasedHorizontalBounds = p.useCameraBasedHorizontalBounds,
                    screenHorizontalMargin = p.screenHorizontalMargin,
                    clampPlatformsToVisibleWidth = p.clampPlatformsToVisibleWidth,
                    manualHalfWidthFallback = p.manualHalfWidthFallback,
                    drawPlayfieldBoundsGizmos = p.drawPlayfieldBoundsGizmos,
                    gravityScale = profile.Character.gravityScale
                };
            }

            public static PlatformGenerationSettings CreateFallback(
                int count, int fallbackSeed, float fallbackStartY,
                float spacingMin, float spacingMax, float range, float width, float height)
            {
                return new PlatformGenerationSettings
                {
                    profileName = "fallback",
                    useProceduralGeneration = true,
                    platformCount = count,
                    initialPlatformCount = 12,
                    platformBufferAhead = 18f,
                    cleanupDistanceBelowCamera = 14f,
                    maxActivePlatforms = 40,
                    generationSegmentHeight = 10f,
                    randomizeSeedOnRun = false,
                    difficultyRampEnabled = true,
                    difficultyRampStartHeight = 25f,
                    difficultyRampStrength = 0.25f,
                    minPlatformWidthAtHighDifficulty = 1.8f,
                    maxVerticalSpacingAtHighDifficulty = 3.2f,
                    recoveryPlatformEvery = 7,
                    recoveryPlatformWidthMultiplier = 1.35f,
                    seed = fallbackSeed,
                    startY = fallbackStartY,
                    verticalSpacingMin = spacingMin,
                    verticalSpacingMax = spacingMax,
                    minVerticalGap = 1.5f,
                    maxVerticalGap = 3f,
                    horizontalRange = range,
                    maxHorizontalGap = 2.2f,
                    horizontalDirectionChangeChance = 0.55f,
                    forceAlternatingPattern = false,
                    platformWidth = width,
                    platformHeight = height,
                    widthVariationEnabled = false,
                    platformWidthMin = 1.8f,
                    platformWidthMax = 3.4f,
                    narrowPlatformChance = 0.15f,
                    wideRecoveryPlatformEvery = 6,
                    easyStartPlatformCount = 5,
                    safeLandingWidthMultiplier = 1.25f,
                    useOneWayPlatforms = true,
                    oneWaySurfaceArc = 150f,
                    useCameraBasedHorizontalBounds = true,
                    screenHorizontalMargin = 0.35f,
                    clampPlatformsToVisibleWidth = true,
                    manualHalfWidthFallback = 4f,
                    drawPlayfieldBoundsGizmos = true,
                    gravityScale = 3f
                };
            }
        }

        struct HorizontalPlayfieldBounds
        {
            public float cameraHalfWidth;
            public float allowedHalfX;
            public float effectiveHalfRange;
        }
    }
}
