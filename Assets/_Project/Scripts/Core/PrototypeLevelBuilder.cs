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

        public Transform TarTransform { get; private set; }
        public Transform TullaTransform { get; private set; }

        public float StartBaselineY => GetPlatformSettings().startY;

        PlatformGenerationSettings GetPlatformSettings()
        {
            var profile = TarTullaTuningAccess.GetActiveProfile();
            if (profile != null)
            {
                var p = profile.Platforms;
                return new PlatformGenerationSettings
                {
                    platformCount = p.platformCount,
                    seed = p.seed,
                    startY = p.startY,
                    verticalSpacingMin = p.verticalSpacingMin,
                    verticalSpacingMax = p.verticalSpacingMax,
                    horizontalRange = p.horizontalRange,
                    platformWidth = p.platformWidth,
                    platformHeight = p.platformHeight,
                    gravityScale = profile.Character.gravityScale
                };
            }

            return new PlatformGenerationSettings
            {
                platformCount = platformCount,
                seed = generationSeed,
                startY = startY,
                verticalSpacingMin = verticalSpacingMin,
                verticalSpacingMax = verticalSpacingMax,
                horizontalRange = horizontalRange,
                platformWidth = platformWidth,
                platformHeight = platformHeight,
                gravityScale = 3f
            };
        }

        struct PlatformGenerationSettings
        {
            public int platformCount;
            public int seed;
            public float startY;
            public float verticalSpacingMin;
            public float verticalSpacingMax;
            public float horizontalRange;
            public float platformWidth;
            public float platformHeight;
            public float gravityScale;
        }

        void Awake()
        {
            ResolveRoots();

            if (buildOnPlay)
                BuildPrototypeLayout();
        }

        [ContextMenu("Build Prototype Layout")]
        public void BuildPrototypeLayout()
        {
            ResolveRoots();

            bool hasProfile = TarTullaTuningAccess.HasActiveProfile;

            if (!hasProfile)
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
            var platformSettings = GetPlatformSettings();

            Debug.Log(
                $"[Tar&Tulla][Builder] Using {(hasProfile ? TarTullaTuningAccess.GetActiveProfile().name : "fallback")} " +
                $"platformCount={platformSettings.platformCount}, horizontalRange={platformSettings.horizontalRange}, seed={platformSettings.seed}");

            BuildPlatforms(platformSettings);

            var tarStart = new Vector2(-0.75f, platformSettings.startY + jumperSpawnHeightAboveStart);
            var tullaStart = new Vector2(0.75f, platformSettings.startY + jumperSpawnHeightAboveStart);

            var tiltInput = EnsureMobileTiltInput();
            var tar = CreateJumper(TarObjectName, tarStart, new Color(0.35f, 0.75f, 0.95f), tiltInput, platformSettings.gravityScale);
            var tulla = CreateJumper(TullaObjectName, tullaStart, new Color(0.95f, 0.55f, 0.35f), tiltInput, platformSettings.gravityScale);

            TarTransform = tar.transform;
            TullaTransform = tulla.transform;

            CreateRope(tar, tulla);
            WireCamera(tar.transform, tulla.transform);

            Debug.Log($"[Tar&Tulla] Prototype layout built ({platformSettings.platformCount} platforms, seed {platformSettings.seed}).");
        }

        void ResolveRoots()
        {
            if (charactersRoot != null && levelRoot != null && systemsRoot != null)
                return;

            var bootstrap = FindFirstObjectByType<GameBootstrap>();
            if (bootstrap == null)
                return;

            var gameRoot = bootstrap.transform;
            charactersRoot ??= gameRoot.Find("CharactersRoot");
            levelRoot ??= gameRoot.Find("LevelRoot");
            systemsRoot ??= gameRoot.Find("Systems");

            cameraFollow ??= FindFirstObjectByType<VerticalCameraFollow2D>();
        }

        public void ClearGeneratedContent()
        {
            DestroyChildren(charactersRoot);
            DestroyChildren(levelRoot);

            if (systemsRoot == null)
                return;

            DestroyIfExists(RopeObjectName);
        }

        void BuildPlatforms(PlatformGenerationSettings platformSettings)
        {
            var state = Random.state;
            Random.InitState(platformSettings.seed);

            float currentY = platformSettings.startY;
            float currentX = 0f;
            float previousWidth = platformSettings.platformWidth + 1.2f;

            for (int i = 0; i < platformSettings.platformCount; i++)
            {
                float width = platformSettings.platformWidth;
                float spacingMin = platformSettings.verticalSpacingMin;
                float spacingMax = platformSettings.verticalSpacingMax;
                float xRange = platformSettings.horizontalRange;

                if (i == 0)
                {
                    width = platformSettings.platformWidth + 1.2f;
                    currentX = 0f;
                }
                else if (i <= 5)
                {
                    width = platformSettings.platformWidth + 0.4f;
                    xRange *= 0.35f;
                    spacingMax = platformSettings.verticalSpacingMin + 0.6f;
                    currentX = Mathf.Clamp(
                        currentX + Random.Range(-xRange, xRange),
                        -platformSettings.horizontalRange * 0.5f,
                        platformSettings.horizontalRange * 0.5f);
                }
                else
                {
                    float direction = (i % 2 == 0) ? 1f : -1f;
                    float step = Random.Range(xRange * 0.35f, xRange);
                    float proposedX = currentX + direction * step;
                    float maxCenterDelta = (width + previousWidth) * 0.5f;
                    proposedX = Mathf.Clamp(proposedX, currentX - maxCenterDelta, currentX + maxCenterDelta);
                    currentX = Mathf.Clamp(proposedX, -platformSettings.horizontalRange, platformSettings.horizontalRange);
                    width = Mathf.Max(2.6f, platformSettings.platformWidth - 0.2f);
                }

                if (i > 0)
                {
                    float spacing = Random.Range(spacingMin, spacingMax);
                    currentY += spacing;
                }

                CreatePlatform($"Platform_{i + 1}", new Vector2(currentX, currentY), new Vector2(width, platformSettings.platformHeight));
                previousWidth = width;
            }

            Random.state = state;
        }

        void CreatePlatform(string name, Vector2 position, Vector2 size)
        {
            var platform = new GameObject(name);
            platform.transform.SetParent(levelRoot, false);
            platform.transform.position = position;
            platform.layer = LayerMask.NameToLayer("Default");

            var renderer = platform.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateRectSprite();
            renderer.color = new Color(0.55f, 0.58f, 0.62f);
            platform.transform.localScale = new Vector3(size.x, size.y, 1f);

            var collider = platform.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            collider.isTrigger = false;
            collider.usedByEffector = true;

            var rb = platform.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;

            var basicPlatform = platform.AddComponent<BasicPlatform>();
            basicPlatform.ApplyOneWaySetup();
        }

        JumperController2D CreateJumper(string name, Vector2 position, Color color, MobileTiltInput2D tiltInput, float gravityScale)
        {
            var jumper = new GameObject(name);
            jumper.transform.SetParent(charactersRoot, false);
            jumper.transform.position = position;

            var renderer = jumper.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateCircleSprite();
            renderer.color = color;
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
            ropeObject.transform.SetParent(systemsRoot, false);

            var lineRenderer = ropeObject.AddComponent<LineRenderer>();
            lineRenderer.numCapVertices = 4;

            var rope = ropeObject.AddComponent<ElasticRope2D>();
            rope.Configure(ropeSettings, tar, tulla);
        }

        void WireCamera(Transform tar, Transform tulla)
        {
            if (cameraFollow == null)
                cameraFollow = FindFirstObjectByType<VerticalCameraFollow2D>();

            cameraFollow?.SetTargets(tar, tulla);
        }

        void DestroyIfExists(string objectName)
        {
            var existing = systemsRoot.Find(objectName);
            if (existing != null)
                DestroyImmediateSafe(existing.gameObject);
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
    }
}
