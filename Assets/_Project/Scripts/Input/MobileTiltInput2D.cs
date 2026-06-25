using UnityEngine;
using TarTulla.Game;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TarTulla.Input
{
    public class MobileTiltInput2D : MonoBehaviour
    {
        [SerializeField] AirControlSettings settings;
        [SerializeField] bool enableDebugLogs;

        float smoothedInput;

        public float HorizontalInput => smoothedInput;

        bool HasTuningSource => TarTullaTuningAccess.HasActiveProfile || settings != null;

        float TiltSensitivity => GetTiltValue(t => t.tiltSensitivity, s => s.tiltSensitivity, 8f);
        float InputDeadZone => GetTiltValue(t => t.inputDeadZone, s => s.inputDeadZone, 0.08f);
        float Smoothing => GetTiltValue(t => t.smoothing, s => s.smoothing, 8f);

        void Update()
        {
            if (!HasTuningSource)
                return;

            float raw = ReadRawInput();
            raw = ApplyDeadZone(raw);
            raw = Mathf.Clamp(raw, -1f, 1f);

            float blend = 1f - Mathf.Exp(-Smoothing * Time.deltaTime);
            smoothedInput = Mathf.Lerp(smoothedInput, raw, blend);

            if (enableDebugLogs && Mathf.Abs(smoothedInput) > InputDeadZone)
                Debug.Log($"[Tar&Tulla][TiltInput] HorizontalInput={smoothedInput:F2}, sensitivity={TiltSensitivity}");
        }

        public void Configure(AirControlSettings airControlSettings)
        {
            settings = airControlSettings;
        }

        float ReadRawInput()
        {
#if UNITY_EDITOR
            if (ShouldUseEditorKeyboard())
                return PrototypeKeyboardInput.ReadHorizontalAxis();
#endif

            return ReadAccelerometerInput();
        }

        bool ShouldUseEditorKeyboard()
        {
            if (settings != null && settings.debugUseKeyboardInEditor)
                return true;

            return TarTullaTuningAccess.HasActiveProfile;
        }

        float ReadAccelerometerInput()
        {
#if ENABLE_INPUT_SYSTEM
            var accelerometer = Accelerometer.current;
            if (accelerometer != null)
            {
                float tilt = accelerometer.acceleration.ReadValue().x * TiltSensitivity;
                return Mathf.Clamp(tilt, -1f, 1f);
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (SystemInfo.supportsAccelerometer)
            {
                float tilt = UnityEngine.Input.acceleration.x * TiltSensitivity;
                return Mathf.Clamp(tilt, -1f, 1f);
            }
#endif

            return 0f;
        }

        float ApplyDeadZone(float value)
        {
            if (Mathf.Abs(value) <= InputDeadZone)
                return 0f;

            float sign = Mathf.Sign(value);
            float magnitude = Mathf.Abs(value);
            return sign * ((magnitude - InputDeadZone) / (1f - InputDeadZone));
        }

        float GetTiltValue(
            System.Func<TarTullaGameplayProfile.TiltTuning, float> fromProfile,
            System.Func<AirControlSettings, float> fromSettings,
            float fallback)
        {
            var profile = TarTullaTuningAccess.GetActiveProfile();
            if (profile != null)
                return fromProfile(profile.Tilt);

            if (settings != null)
                return fromSettings(settings);

            return fallback;
        }
    }
}
