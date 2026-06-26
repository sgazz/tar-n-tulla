using UnityEngine;

namespace TarTulla.CameraSystems
{
    /// <summary>
    /// One-shot sorting layer checks for visual setup (Editor / Development Build only).
    /// </summary>
    public static class VisualSortingLayerValidator
    {
        const string NearForegroundName = "NearSideForeground";
        const string RopeObjectName = "ElasticRope";
        const string PlatformsName = "Platforms";
        const string BackgroundRootName = "BackgroundRoot";
        const string GameplayRootName = "GameplayRoot";
        const string ForegroundRootName = "ForegroundRoot";

        static bool validated;

        public static void ValidateOnce(Transform gameRoot)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (validated || gameRoot == null)
                return;

            validated = true;

            if (!VisualLayerSortingUtility.LayerExists(VisualSortingLayers.Gameplay))
            {
                Debug.LogWarning(
                    "[Tar&Tulla][Visual] Gameplay sorting layer not found — add sorting layers in Project Settings → Tags and Layers.");
                return;
            }

            int gameplayIndex = VisualLayerSortingUtility.GetLayerIndex(VisualSortingLayers.Gameplay);
            int foregroundIndex = VisualLayerSortingUtility.GetLayerIndex(VisualSortingLayers.Foreground);

            ValidateNearForeground(gameRoot, foregroundIndex);
            ValidateRope(gameRoot, gameplayIndex);
            ValidatePlatforms(gameRoot, gameplayIndex);
            ValidateBackgrounds(gameRoot, gameplayIndex);
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        static void ValidateNearForeground(Transform gameRoot, int expectedLayerIndex)
        {
            var foregroundRoot = gameRoot.Find(ForegroundRootName);
            if (foregroundRoot == null)
                return;

            var nearFg = foregroundRoot.Find(NearForegroundName);
            if (nearFg == null)
                return;

            var renderer = nearFg.GetComponent<SpriteRenderer>();
            if (renderer == null || !renderer.enabled)
                return;

            int layerIndex = VisualLayerSortingUtility.GetLayerIndex(renderer.sortingLayerName);
            if (layerIndex != expectedLayerIndex)
            {
                Debug.LogWarning(
                    $"[Tar&Tulla][Visual] NearSideForeground sorting layer is '{renderer.sortingLayerName}' — expected '{VisualSortingLayers.Foreground}'.",
                    nearFg);
            }
        }

        static void ValidateRope(Transform gameRoot, int expectedLayerIndex)
        {
            var gameplayRoot = gameRoot.Find(GameplayRootName);
            if (gameplayRoot == null)
                return;

            var ropeRoot = gameplayRoot.Find("Rope");
            if (ropeRoot == null)
                return;

            var elasticRope = ropeRoot.Find(RopeObjectName);
            if (elasticRope == null)
                return;

            var lineRenderer = elasticRope.GetComponent<LineRenderer>();
            if (lineRenderer == null)
                return;

            int layerIndex = VisualLayerSortingUtility.GetLayerIndex(lineRenderer.sortingLayerName);
            if (layerIndex != expectedLayerIndex)
            {
                Debug.LogWarning(
                    $"[Tar&Tulla][Visual] ElasticRope sorting layer is '{lineRenderer.sortingLayerName}' — expected '{VisualSortingLayers.Gameplay}'.",
                    elasticRope);
            }
        }

        static void ValidatePlatforms(Transform gameRoot, int expectedLayerIndex)
        {
            var gameplayRoot = gameRoot.Find(GameplayRootName);
            if (gameplayRoot == null)
                return;

            var platformsRoot = gameplayRoot.Find(PlatformsName);
            if (platformsRoot == null || platformsRoot.childCount == 0)
                return;

            for (int i = 0; i < platformsRoot.childCount; i++)
            {
                var platform = platformsRoot.GetChild(i);
                var renderer = platform.GetComponent<SpriteRenderer>();
                if (renderer == null)
                    continue;

                int layerIndex = VisualLayerSortingUtility.GetLayerIndex(renderer.sortingLayerName);
                if (layerIndex != expectedLayerIndex)
                {
                    Debug.LogWarning(
                        $"[Tar&Tulla][Visual] Platform '{platform.name}' sorting layer is '{renderer.sortingLayerName}' — expected '{VisualSortingLayers.Gameplay}'.",
                        platform);
                    return;
                }
            }
        }

        static void ValidateBackgrounds(Transform gameRoot, int gameplayLayerIndex)
        {
            var backgroundRoot = gameRoot.Find(BackgroundRootName);
            if (backgroundRoot == null)
                return;

            for (int i = 0; i < backgroundRoot.childCount; i++)
            {
                var layer = backgroundRoot.GetChild(i);
                var renderer = layer.GetComponent<SpriteRenderer>();
                if (renderer == null || !renderer.enabled)
                    continue;

                string layerName = renderer.sortingLayerName;
                int layerIndex = VisualLayerSortingUtility.GetLayerIndex(layerName);

                if (layerName == "Default")
                {
                    Debug.LogWarning(
                        $"[Tar&Tulla][Visual] Background '{layer.name}' is on Default — expected BackgroundFar or BackgroundMid.",
                        layer);
                    continue;
                }

                if (layerIndex < 0 || layerIndex >= gameplayLayerIndex)
                {
                    Debug.LogWarning(
                        $"[Tar&Tulla][Visual] Background '{layer.name}' may render in front of gameplay (layer '{layerName}').",
                        layer);
                }
            }
        }
#endif
    }
}
