using UnityEngine;

namespace TarTulla.CameraSystems
{
    public static class VisualLayerSortingUtility
    {
        public const int FallbackFarOrder = -1000;
        public const int FallbackMidOrder = -500;
        public const int FallbackGameplayOrder = 0;
        public const int FallbackForegroundOrder = 1000;

        public static bool LayerExists(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
                return false;

            var layers = SortingLayer.layers;
            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i].name == layerName)
                    return true;
            }

            return false;
        }

        static readonly System.Collections.Generic.HashSet<string> warnedMissingLayers = new();

        public static void Apply(SpriteRenderer renderer, string layerName, int layerOrder, int fallbackOrder)
        {
            if (renderer == null)
                return;

            if (LayerExists(layerName))
            {
                renderer.sortingLayerID = SortingLayer.NameToID(layerName);
                renderer.sortingOrder = layerOrder;
                return;
            }

            if (!warnedMissingLayers.Contains(layerName))
            {
                warnedMissingLayers.Add(layerName);
                Debug.LogWarning(
                    $"[Tar&Tulla][Visual] Sorting layer '{layerName}' not found — using Default with order {fallbackOrder}. " +
                    "In Editor: menu Tar&Tulla → Ensure Visual Sorting Layers.",
                    renderer);
            }

            renderer.sortingLayerID = SortingLayer.NameToID("Default");
            renderer.sortingOrder = fallbackOrder;
        }

        public static int GetLayerIndex(string layerName)
        {
            var layers = SortingLayer.layers;
            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i].name == layerName)
                    return i;
            }

            return -1;
        }

        static Material urpUnlitMaterial;

        public static void ApplyUnlitMaterial(SpriteRenderer renderer)
        {
            if (renderer == null)
                return;

            urpUnlitMaterial ??= CreateUnlitMaterial();
            if (urpUnlitMaterial != null)
                renderer.sharedMaterial = urpUnlitMaterial;
        }

        static Material CreateUnlitMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");

            if (shader == null)
            {
                Debug.LogWarning("[Tar&Tulla][Visual] Could not find URP 2D unlit sprite shader.");
                return null;
            }

            return new Material(shader);
        }
    }
}
