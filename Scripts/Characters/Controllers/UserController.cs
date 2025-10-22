using Godot;

namespace CrossDimensions.Characters.Controllers;

public sealed partial class UserController : CharacterController
{
    public override Vector2 MovementInput
    {
        get
        {
            Vector2 input = Vector2.Zero;
            if (Input.IsActionPressed("move_right"))
            {
                input.X += 1;
            }
            if (Input.IsActionPressed("move_left"))
            {
                input.X -= 1;
            }
            if (Input.IsActionPressed("move_down"))
            {
                input.Y += 1;
            }
            if (Input.IsActionPressed("move_up"))
            {
                input.Y -= 1;
            }
            return input.Normalized();
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
}
