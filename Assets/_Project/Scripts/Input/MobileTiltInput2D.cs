using UnityEngine;
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

        void Update()
        {
            if (settings == null)
                return;

            float raw = ReadRawInput();
            raw = ApplyDeadZone(raw);
            raw = Mathf.Clamp(raw, -1f, 1f);

            float blend = 1f - Mathf.Exp(-settings.smoothing * Time.deltaTime);
            smoothedInput = Mathf.Lerp(smoothedInput, raw, blend);

            if (enableDebugLogs && Mathf.Abs(smoothedInput) > settings.inputDeadZone)
                Debug.Log($"[Tar&Tulla][TiltInput] HorizontalInput={smoothedInput:F2}");
        }

        public void Configure(AirControlSettings airControlSettings)
        {
            settings = airControlSettings;
        }

        float ReadRawInput()
        {
#if UNITY_EDITOR
            if (settings.debugUseKeyboardInEditor)
                return PrototypeKeyboardInput.ReadHorizontalAxis();
#endif

            return ReadAccelerometerInput();
        }

        float ReadAccelerometerInput()
        {
#if ENABLE_INPUT_SYSTEM
            var accelerometer = Accelerometer.current;
            if (accelerometer != null)
            {
                float tilt = accelerometer.acceleration.ReadValue().x * settings.tiltSensitivity;
                return Mathf.Clamp(tilt, -1f, 1f);
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (SystemInfo.supportsAccelerometer)
            {
                float tilt = UnityEngine.Input.acceleration.x * settings.tiltSensitivity;
                return Mathf.Clamp(tilt, -1f, 1f);
            }
#endif

            return 0f;
        }

        float ApplyDeadZone(float value)
        {
            if (Mathf.Abs(value) <= settings.inputDeadZone)
                return 0f;

            float sign = Mathf.Sign(value);
            float magnitude = Mathf.Abs(value);
            return sign * ((magnitude - settings.inputDeadZone) / (1f - settings.inputDeadZone));
        }
    }
}
