using UnityEngine;

namespace TarTulla.Game
{
    /// <summary>
    /// Explicit profile-first tuning resolution for gameplay systems.
    /// </summary>
    public static class TarTullaTuningAccess
    {
        public static bool HasActiveProfile =>
            TarTullaRuntime.Instance != null && TarTullaRuntime.Instance.Profile != null;

        public static TarTullaGameplayProfile GetActiveProfile() =>
            HasActiveProfile ? TarTullaRuntime.Instance.Profile : null;
    }
}
