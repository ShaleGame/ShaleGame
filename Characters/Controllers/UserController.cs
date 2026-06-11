using Godot;

namespace CrossedDimensions.Characters.Controllers;

[GlobalClass]
public sealed partial class UserController : CharacterController
{
    public override Vector2 MovementInput
    {
        get
        {
            if (!IsActive)
            {
                return Vector2.Zero;
            }
            Vector2 vec = Input.GetVector("move_left", "move_right", "move_up", "move_down");
            vec *= XScale;
            return vec;
        }
    }

    public override Vector2 Target
    {
        get
        {
            if (!IsActive)
            {
                return Vector2.Zero;
            }
            return GetAimPosition() - GlobalPosition;
        }
    }

    /// <summary>
    /// The world-space point being aimed at: the shared virtual cursor while on
    /// controller, otherwise the mouse. Both the player and clone resolve to the
    /// same cursor point, so split fire converges as it does at the mouse.
    /// </summary>
    private Vector2 GetAimPosition()
    {
        var aim = GetNodeOrNull<AimManager>("/root/AimManager");
        if (aim is not null && aim.IsControllerAiming)
        {
            return aim.CursorWorld;
        }
        return GetGlobalMousePosition();
    }

    public override bool IsJumping => IsActive && Input.IsActionJustPressed("jump");

    public override bool IsJumpHeld => IsActive && Input.IsActionPressed("jump");

    public override bool IsJumpReleased => IsActive && Input.IsActionJustReleased("jump");

    public override bool IsMouse1Held => IsActive && Input.IsActionPressed("mouse1");

    public override bool IsMouse2Held => IsActive && Input.IsActionPressed("mouse2");

    public override bool IsSplitting => IsActive && Input.IsActionJustPressed("split");

    public override bool IsSplitReleased => IsActive && Input.IsActionJustReleased("split");

    public override bool IsSplitHeld => IsActive && Input.IsActionPressed("split");

    public override bool IsInteractHeld => IsActive && Input.IsActionPressed("interact");

    public override void _Input(InputEvent @event)
    {
        if (!IsActive)
        {
            return;
        }

        if (@event.IsActionPressed("weapon_next"))
        {
            EmitSignal(SignalName.WeaponNextRequested);
        }

        if (@event.IsActionPressed("weapon_prev"))
        {
            EmitSignal(SignalName.WeaponPreviousRequested);
        }

        if (@event.IsActionPressed("slot0"))
        {
            EmitSignal(SignalName.WeaponSlotRequested, 0);
        }

        if (@event.IsActionPressed("slot1"))
        {
            EmitSignal(SignalName.WeaponSlotRequested, 1);
        }

        if (@event.IsActionPressed("slot2"))
        {
            EmitSignal(SignalName.WeaponSlotRequested, 2);
        }
    }
}
