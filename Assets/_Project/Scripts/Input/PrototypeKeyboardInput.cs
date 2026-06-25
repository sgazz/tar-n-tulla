#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TarTulla.Input
{
    /// <summary>
    /// Editor/debug keyboard helpers compatible with the active Input System package.
    /// </summary>
    public static class PrototypeKeyboardInput
    {
        public static float ReadHorizontalAxis()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return 0f;

            float input = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                input -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                input += 1f;
            return input;
#else
            float input = 0f;
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.A) || UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftArrow))
                input -= 1f;
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.D) || UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightArrow))
                input += 1f;
            return input;
#endif
        }

        public static bool WasResetRunPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
            return UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.R);
#endif
        }

        public static bool WasRebuildLayoutPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame;
#else
            return UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.B);
#endif
        }
    }
}
