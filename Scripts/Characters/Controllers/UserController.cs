using Godot;

namespace CrossDimensions.Characters.Controllers;

public sealed partial class UserController : CharacterController
{
    public override Vector2 MovementInput
    {
        get
        {
            Vector2 vec = Input.GetVector("move_left", "move_right", "move_up", "move_down");
            vec.X *= XScale;
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

    public override bool IsJumping => Input.IsActionPressed("jump");

    public override bool IsMouse1Held => Input.IsActionPressed("mouse1");

    public override bool IsMouse2Held => Input.IsActionPressed("mouse2");

    public override bool IsSplitting => Input.IsActionJustPressed("split");
}
