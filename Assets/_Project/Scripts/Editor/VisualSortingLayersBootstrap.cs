#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TarTulla.CameraSystems.Editor
{
    /// <summary>
    /// Ensures Tar&Tulla sorting layers exist in TagManager (Editor only).
    /// Run via menu if layers are missing after pulling project changes.
    /// </summary>
    [InitializeOnLoad]
    public static class VisualSortingLayersBootstrap
    {
        static readonly string[] RequiredLayers =
        {
            VisualSortingLayers.BackgroundFar,
            VisualSortingLayers.BackgroundMid,
            VisualSortingLayers.Gameplay,
            VisualSortingLayers.Foreground,
            VisualSortingLayers.UI
        };

        static VisualSortingLayersBootstrap()
        {
            EditorApplication.delayCall += () =>
            {
                EnsureSortingLayers();
                EnsureGlobalLightSortingLayers();
            };
        }

        [MenuItem("Tar&Tulla/Ensure Global Light 2D Sorting Layers")]
        public static void EnsureGlobalLightSortingLayers()
        {
            if (Application.isPlaying)
                return;

            var lights = Object.FindObjectsByType<Light2D>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            bool changed = false;

            for (int i = 0; i < lights.Length; i++)
            {
                var light = lights[i];
                if (light.lightType != Light2D.LightType.Global)
                    continue;

                var serializedLight = new SerializedObject(light);
                var applyLayers = serializedLight.FindProperty("m_ApplyToSortingLayers");
                if (applyLayers == null)
                    continue;

                int mask = 0;
                for (int j = 0; j < SortingLayer.layers.Length; j++)
                    mask |= 1 << j;

                applyLayers.intValue = mask;
                serializedLight.ApplyModifiedProperties();
                EditorUtility.SetDirty(light);
                changed = true;
            }

            if (changed)
                Debug.Log("[Tar&Tulla] Global Light 2D now targets all sorting layers.");
        }

        [MenuItem("Tar&Tulla/Ensure Visual Sorting Layers")]
        public static void EnsureSortingLayers()
        {
            if (Application.isPlaying)
                return;

            var tagManagerAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (tagManagerAsset == null || tagManagerAsset.Length == 0)
            {
                Debug.LogError("[Tar&Tulla] Could not load ProjectSettings/TagManager.asset");
                return;
            }

            var tagManager = new SerializedObject(tagManagerAsset[0]);
            var sortingLayers = tagManager.FindProperty("m_SortingLayers");
            bool changed = false;

            for (int i = 0; i < RequiredLayers.Length; i++)
            {
                string layerName = RequiredLayers[i];
                if (SortingLayerWorks(layerName))
                    continue;

                int index = FindLayerIndex(sortingLayers, layerName);
                if (index < 0)
                {
                    sortingLayers.InsertArrayElementAtIndex(sortingLayers.arraySize);
                    index = sortingLayers.arraySize - 1;
                    sortingLayers.GetArrayElementAtIndex(index).FindPropertyRelative("name").stringValue = layerName;
                }

                var element = sortingLayers.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("uniqueID").intValue = GenerateUniqueSortingLayerId(sortingLayers);
                element.FindPropertyRelative("locked").boolValue = false;
                changed = true;
            }

            if (!changed)
                return;

            tagManager.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log("[Tar&Tulla] Visual sorting layers ensured in TagManager.");
        }

        static bool SortingLayerWorks(string layerName)
        {
            if (layerName == "Default")
                return true;

            return SortingLayer.NameToID(layerName) != 0;
        }

        static int FindLayerIndex(SerializedProperty sortingLayers, string name)
        {
            for (int i = 0; i < sortingLayers.arraySize; i++)
            {
                var element = sortingLayers.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative("name").stringValue == name)
                    return i;
            }

            return -1;
        }

        static int GenerateUniqueSortingLayerId(SerializedProperty sortingLayers)
        {
            for (int attempt = 0; attempt < 64; attempt++)
            {
                int id = GUID.Generate().GetHashCode();
                if (id == 0)
                    continue;

                if (!IdExists(sortingLayers, id))
                    return id;
            }

            return Mathf.Abs("TarTulla".GetHashCode());
        }

        static bool IdExists(SerializedProperty sortingLayers, int id)
        {
            for (int i = 0; i < sortingLayers.arraySize; i++)
            {
                if (sortingLayers.GetArrayElementAtIndex(i).FindPropertyRelative("uniqueID").intValue == id)
                    return true;
            }

            return false;
        }
    }
}
#endif
