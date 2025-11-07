using Godot;

namespace CrossDimensions.States.Characters;

public partial class CharacterMoveState : CharacterState
{
    [Export]
    public State IdleState { get; set; }

    [Export]
    public State FallState { get; set; }

    public override State Enter(State previousState)
    {
        if (!CharacterContext.Controller.IsMoving)
        {
            return IdleState;
        }

        return null;
    }

    public override State Process(double delta)
    {
        if (!CharacterContext.Controller.IsMoving)
        {
            return IdleState;
        }

        return null;
    }

    public override State PhysicsProcess(double delta)
    {
        Vector2 gravity = ProjectSettings
            .GetSetting("physics/2d/default_gravity_vector")
            .AsVector2();
        gravity *= ProjectSettings
            .GetSetting("physics/2d/default_gravity")
            .AsSingle();

        Vector2 movementDir = CharacterContext.Controller.MovementInput;
        float x = Mathf.Sign(movementDir.X) * CharacterContext.Speed;

        // apply gravity to the character
        CharacterContext.Velocity += gravity * (float)delta;
        CharacterContext.Velocity += x * Vector2.Right;
        CharacterContext.MoveAndSlide();

        // check if in air
        if (!CharacterContext.IsOnFloor())
        {
            return FallState;
        }

        return null;
    }
}
