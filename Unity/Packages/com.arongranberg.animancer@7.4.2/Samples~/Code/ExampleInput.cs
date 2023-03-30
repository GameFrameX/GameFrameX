// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Animancer.Examples
{
    /// <summary>
    /// A standard wrapper for receiving input from the
    /// <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@latest">Input System</see> package or the
    /// <see href="https://docs.unity3d.com/Manual/class-InputManager.html">Legacy Input Manager</see>.
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/examples/basics/input">Input</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples/ExampleInput
    /// 
    [HelpURL(Strings.DocsURLs.APIDocumentation + "." + nameof(Examples) + "/" + nameof(ExampleInput))]
    public static class ExampleInput
    {
        /************************************************************************************************************************/

        /// <summary>The current screen position of the mouse pointer.</summary>
        public static Vector2 MousePosition
#if ENABLE_INPUT_SYSTEM
            => Mouse.current.position.ReadValue();
#else
            => Input.mousePosition;
#endif

        /// <summary>The amount that the mouse has moved since last frame.</summary>
        public static Vector2 MousePositionDelta
#if ENABLE_INPUT_SYSTEM
            => Mouse.current.delta.ReadValue();
#else
            => new Vector2(Input.GetAxisRaw("Mouse X") * 20, Input.GetAxisRaw("Mouse Y") * 20);
#endif

        /// <summary>The amount that the mouse scroll value has changed since last frame.</summary>
        public static Vector2 MouseScrollDelta
#if ENABLE_INPUT_SYSTEM
            => Mouse.current.scroll.ReadValue() * 0.01f;
#else
            => Input.mouseScrollDelta;
#endif

        /************************************************************************************************************************/

        /// <summary>Was the left mouse button pressed this frame?</summary>
        public static bool LeftMouseDown
#if ENABLE_INPUT_SYSTEM
            => Mouse.current.leftButton.wasPressedThisFrame;
#else
            => Input.GetMouseButtonDown(0);
#endif

        /// <summary>Is the left mouse button currently being held down?</summary>
        public static bool LeftMouseHold
#if ENABLE_INPUT_SYSTEM
            => Mouse.current.leftButton.isPressed;
#else
            => Input.GetMouseButton(0);
#endif

        /// <summary>Was the left mouse button released this frame?</summary>
        public static bool LeftMouseUp
#if ENABLE_INPUT_SYSTEM
            => Mouse.current.leftButton.wasReleasedThisFrame;
#else
            => Input.GetMouseButtonUp(0);
#endif

        /************************************************************************************************************************/

        /// <summary>Was the right mouse button pressed this frame?</summary>
        public static bool RightMouseDown
#if ENABLE_INPUT_SYSTEM
            => Mouse.current.rightButton.wasPressedThisFrame;
#else
            => Input.GetMouseButtonDown(1);
#endif

        /// <summary>Is the right mouse button currently being held down?</summary>
        public static bool RightMouseHold
#if ENABLE_INPUT_SYSTEM
            => Mouse.current.rightButton.isPressed;
#else
            => Input.GetMouseButton(1);
#endif

        /************************************************************************************************************************/

        /// <summary>Was <see cref="KeyCode.Space"/> pressed this frame?</summary>
        public static bool SpaceDown
#if ENABLE_INPUT_SYSTEM
            => Keyboard.current.spaceKey.wasPressedThisFrame;
#else
            => Input.GetKeyDown(KeyCode.Space);
#endif

        /// <summary>Is <see cref="KeyCode.Space"/> currently being held down?</summary>
        public static bool SpaceHold
#if ENABLE_INPUT_SYSTEM
            => Keyboard.current.spaceKey.isPressed;
#else
            => Input.GetKey(KeyCode.Space);
#endif

        /// <summary>Was <see cref="KeyCode.Space"/> released this frame?</summary>
        public static bool SpaceUp
#if ENABLE_INPUT_SYSTEM
            => Keyboard.current.spaceKey.wasReleasedThisFrame;
#else
            => Input.GetKeyUp(KeyCode.Space);
#endif

        /************************************************************************************************************************/

        /// <summary>Is <see cref="KeyCode.LeftShift"/> currently being held down?</summary>
        public static bool LeftShiftHold
#if ENABLE_INPUT_SYSTEM
            => Keyboard.current.leftShiftKey.isPressed;
#else
            => Input.GetKey(KeyCode.LeftShift);
#endif

        /************************************************************************************************************************/

        /// <summary>Was <see cref="KeyCode.Alpha1"/> released this frame?</summary>
        public static bool Number1Up
#if ENABLE_INPUT_SYSTEM
            => Keyboard.current.digit1Key.wasReleasedThisFrame;
#else
            => Input.GetKeyUp(KeyCode.Alpha1);
#endif

        /// <summary>Was <see cref="KeyCode.Alpha2"/> released this frame?</summary>
        public static bool Number2Up
#if ENABLE_INPUT_SYSTEM
            => Keyboard.current.digit2Key.wasReleasedThisFrame;
#else
            => Input.GetKeyUp(KeyCode.Alpha2);
#endif

        /************************************************************************************************************************/

#if ENABLE_INPUT_SYSTEM
        private static InputAction _WasdAction;
#endif

        /// <summary>WASD Controls.</summary>
        public static Vector2 WASD
#if ENABLE_INPUT_SYSTEM
        {
            get
            {
                if (_WasdAction == null)
                {
                    _WasdAction = new InputAction(nameof(WASD), InputActionType.Value);
                    _WasdAction.AddCompositeBinding("2DVector")
                        .With("Up", "<Keyboard>/w")
                        .With("Down", "<Keyboard>/s")
                        .With("Left", "<Keyboard>/a")
                        .With("Right", "<Keyboard>/d");
                    _WasdAction.Enable();
                }

                return _WasdAction.ReadValue<Vector2>();
            }
        }
#else
            => new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif

        /************************************************************************************************************************/
    }
}
