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
            Vector2 mousePosition = GetGlobalMousePosition();
            return mousePosition - GlobalPosition;
        }
    }

    public override bool IsJumping => IsActive && Input.IsActionJustPressed("jump");

    public override bool IsJumpHeld => IsActive && Input.IsActionPressed("jump");

    public override bool IsJumpReleased => IsActive && Input.IsActionJustReleased("jump");

    public override bool IsMouse1Held => IsActive && Input.IsActionPressed("mouse1");

    public override bool IsMouse2Held => IsActive && Input.IsActionPressed("mouse2");

    public override bool IsSplitting => IsActive && Input.IsActionJustPressed("split");

    public override bool IsInteractHeld => IsActive && Input.IsActionPressed("interact");

    public override bool IsSplitReleased => IsActive && Input.IsActionJustReleased("split");

    public override void _Input(InputEvent @event)
    {
        if (!IsActive) return;

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
