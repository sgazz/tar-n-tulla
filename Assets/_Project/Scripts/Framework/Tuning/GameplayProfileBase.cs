using UnityEngine;

namespace GameplayFramework.Tuning
{
    public abstract class GameplayProfileBase : ScriptableObject
    {
        [Header("Profile Metadata")]
        [SerializeField] string profileId = "profile_default";
        [SerializeField] string displayName = "Default Profile";
        [TextArea(2, 4)]
        [SerializeField] string description;
        [SerializeField] int version = 1;

        public string ProfileId => profileId;
        public string DisplayName => displayName;
        public string Description => description;
        public int Version => version;

        public virtual bool ValidateProfile()
        {
            if (string.IsNullOrWhiteSpace(profileId))
            {
                Debug.LogWarning($"[{name}] Profile validation failed: profileId is empty.", this);
                return false;
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                Debug.LogWarning($"[{name}] Profile validation failed: displayName is empty.", this);
                return false;
            }

            return true;
        }
    }
}
