using Godot;

namespace CrossedDimensions.Characters.Controllers;

/// <summary>
/// Autoload that drives the shared aim <see cref="VirtualCursor"/> while on
/// controller. Anchored to the active camera's screen center, so the cursor
/// offset reads like a mouse screen position. It exposes one world-space aim
/// point that both the player and clone read, preserving the mouse's fire
/// convergence when split.
/// </summary>
[GlobalClass]
public partial class AimManager : Node
{
    private const float AimDeadzone = 0.2f;

    private readonly VirtualCursor _cursor = new();
    private InputManager _input;

    /// <summary>
    /// World-space point both bodies aim at while on controller.
    /// </summary>
    public Vector2 CursorWorld { get; private set; }

    /// <summary>
    /// Whether aim should currently come from the cursor (controller).
    /// </summary>
    public bool IsControllerAiming =>
        _input is not null && _input.Device == InputDevice.Controller;

    /// <summary>
    /// The wrapped cursor, exposed for tuning and inspection.
    /// </summary>
    public VirtualCursor Cursor => _cursor;

    public override void _Ready()
    {
        _input = GetNodeOrNull<InputManager>("/root/InputManager");
    }

    public override void _Process(double delta)
    {
        if (!IsControllerAiming)
        {
            return;
        }

        _cursor.Process(ReadAimStick(), (float)delta);

        Camera2D camera = GetViewport()?.GetCamera2D();
        if (camera is not null)
        {
            CursorWorld = _cursor.CursorWorld(camera.GetScreenCenterPosition());
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey eventKey)
        {
            if (eventKey.IsActionPressed("aim_precision"))
            {
                _cursor.ToggleMode();
            }
        }
    }

    // TODO: migrate to rebindable `aim_*` input-map actions (read via GetVector)
    // once joypad bindings land. Raw right-stick read for device 0 meanwhile.
    private static Vector2 ReadAimStick()
    {
        var stick = new Vector2(
            Input.GetJoyAxis(0, JoyAxis.RightX),
            Input.GetJoyAxis(0, JoyAxis.RightY));

        return stick.Length() < AimDeadzone
            ? Vector2.Zero
            : stick.LimitLength(1f);
    }
}
