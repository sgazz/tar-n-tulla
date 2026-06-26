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
        bool loggedMissingSensor;

        public float HorizontalInput => smoothedInput;

        bool HasTuningSource => TarTullaTuningAccess.HasActiveProfile || settings != null;

        float TiltSensitivity => GetTiltValue(t => t.tiltSensitivity, s => s.tiltSensitivity, 8f);
        float InputDeadZone => GetTiltValue(t => t.inputDeadZone, s => s.inputDeadZone, 0.08f);
        float Smoothing => GetTiltValue(t => t.smoothing, s => s.smoothing, 8f);

        void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            InputSystem.onDeviceChange += HandleDeviceChange;
            EnableAvailableSensors();
#endif
        }

        void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            InputSystem.onDeviceChange -= HandleDeviceChange;
#endif
        }

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
            if (TryReadGravitySensor(out float gravityTilt))
                return gravityTilt;

            if (TryReadAccelerometerDevice(out float accelTilt))
                return accelTilt;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (SystemInfo.supportsAccelerometer)
            {
                float tilt = UnityEngine.Input.acceleration.x * TiltSensitivity;
                return Mathf.Clamp(tilt, -1f, 1f);
            }
#endif

            LogMissingSensorOnce();
            return 0f;
        }

#if ENABLE_INPUT_SYSTEM
        void HandleDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected)
                TryEnableSensor(device);
        }

        void EnableAvailableSensors()
        {
            foreach (var device in InputSystem.devices)
                TryEnableSensor(device);
        }

        static void TryEnableSensor(InputDevice device)
        {
            if (device == null || device.enabled)
                return;

            if (device is GravitySensor or Accelerometer)
                InputSystem.EnableDevice(device);
        }

        bool TryReadGravitySensor(out float tilt)
        {
            tilt = 0f;
            var sensor = GravitySensor.current;
            if (sensor == null)
                return false;

            if (!sensor.enabled)
                InputSystem.EnableDevice(sensor);

            Vector3 gravity = sensor.gravity.ReadValue();
            if (gravity.sqrMagnitude < 0.01f)
                return false;

            // Portrait: device X maps to left/right tilt when held upright.
            tilt = (gravity.x / gravity.magnitude) * TiltSensitivity;
            tilt = Mathf.Clamp(tilt, -1f, 1f);
            return true;
        }

        bool TryReadAccelerometerDevice(out float tilt)
        {
            tilt = 0f;
            var sensor = Accelerometer.current;
            if (sensor == null)
                return false;

            if (!sensor.enabled)
                InputSystem.EnableDevice(sensor);

            Vector3 acceleration = sensor.acceleration.ReadValue();
            if (acceleration.sqrMagnitude < 0.01f)
                return false;

            tilt = (acceleration.x / acceleration.magnitude) * TiltSensitivity;
            tilt = Mathf.Clamp(tilt, -1f, 1f);
            return true;
        }
#endif

        void LogMissingSensorOnce()
        {
            if (loggedMissingSensor)
                return;

            loggedMissingSensor = true;
            Debug.LogWarning(
                "[Tar&Tulla][TiltInput] No gravity/accelerometer sensor available. " +
                "Tilt control disabled on this device/build.",
                this);
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
