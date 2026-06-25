using UnityEngine;

namespace TarTulla.Core
{
    [DefaultExecutionOrder(-100)]
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] Transform systemsRoot;
        [SerializeField] Transform levelRoot;
        [SerializeField] Transform charactersRoot;
        [SerializeField] Transform uiRoot;

        void Awake()
        {
            LockPortraitOrientation();
            EnsureClimbSystems();
            Debug.Log("[Tar&Tulla] Prototype scene initialized — Milestone 1C");
        }

        void EnsureClimbSystems()
        {
            var systems = systemsRoot != null ? systemsRoot.gameObject : gameObject;

            if (systems.GetComponent<ClimbProgressTracker>() == null)
                systems.AddComponent<ClimbProgressTracker>();

            if (systems.GetComponent<PrototypeRunController>() == null)
                systems.AddComponent<PrototypeRunController>();
        }

        static void LockPortraitOrientation()
        {
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.Portrait;
        }

        void OnValidate()
        {
            ValidateReference(nameof(systemsRoot), systemsRoot);
            ValidateReference(nameof(levelRoot), levelRoot);
            ValidateReference(nameof(charactersRoot), charactersRoot);
            ValidateReference(nameof(uiRoot), uiRoot);
        }

        static void ValidateReference(string fieldName, Transform reference)
        {
            if (reference == null)
            {
                Debug.LogWarning($"[Tar&Tulla] GameBootstrap: Missing reference — {fieldName}");
            }
        }
    }
}
