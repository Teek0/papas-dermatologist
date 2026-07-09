using UnityEngine;
using UnityEngine.InputSystem;

public static class GameplayPointerInput
{
    private static readonly InputAction PointerPosition = new(
        "GameplayPointerPosition",
        InputActionType.PassThrough,
        "<Pointer>/position",
        expectedControlType: "Vector2");

    private static readonly InputAction PointerPress = new(
        "GameplayPointerPress",
        InputActionType.Button,
        "<Pointer>/press",
        expectedControlType: "Button");

    private static bool isEnabled;

    public static Vector2 ScreenPosition
    {
        get
        {
            EnsureEnabled();
            return PointerPosition.ReadValue<Vector2>();
        }
    }

    public static bool WasPressedThisFrame()
    {
        EnsureEnabled();
        return PointerPress.WasPressedThisFrame();
    }

    public static bool WasReleasedThisFrame()
    {
        EnsureEnabled();
        return PointerPress.WasReleasedThisFrame();
    }

    public static bool IsPressed()
    {
        EnsureEnabled();
        return PointerPress.IsPressed();
    }

    private static void EnsureEnabled()
    {
        if (isEnabled)
            return;

        PointerPosition.Enable();
        PointerPress.Enable();
        isEnabled = true;
    }
}
