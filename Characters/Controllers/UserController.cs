using Godot;

namespace CrossedDimensions.Characters.Controllers;

public sealed partial class UserController : CharacterController
{
    public override Vector2 MovementInput
    {
        get
        {
            Vector2 vec = Input.GetVector("move_left", "move_right", "move_up", "move_down");
            vec *= XScale;
            return vec;
        }
    }

    public override Vector2 Target
    {
        get
        {
            var mousePosition = GetGlobalMousePosition();
            return mousePosition - GlobalPosition;
        }
    }

    public override bool IsJumping => Input.IsActionJustPressed("jump");

    public override bool IsJumpHeld => Input.IsActionPressed("jump");

    public override bool IsJumpReleased => Input.IsActionJustReleased("jump");

    public override bool IsMouse1Held => Input.IsActionPressed("mouse1");

    public override bool IsMouse2Held => Input.IsActionPressed("mouse2");

    public override bool IsSplitting => Input.IsActionJustPressed("split");

    public override void _Input(InputEvent @event)
    {
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
