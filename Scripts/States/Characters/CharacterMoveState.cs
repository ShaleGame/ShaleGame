using Godot;

namespace CrossDimensions.States.Characters;

public partial class CharacterMoveState : CharacterState
{
    [Export]
    public State IdleState { get; set; }

    [Export]
    public State AirState { get; set; }

    [Export]
    public State SplitState { get; set; }

    public override State Enter(State previousState)
    {
        if (!CharacterContext.Controller.IsMoving)
        {
            return IdleState;
        }

        if (!CharacterContext.IsOnFloor())
        {
            return AirState;
        }

        return null;
    }

    public override State Process(double delta)
    {
        if (!CharacterContext.Controller.IsMoving)
        {
            return IdleState;
        }

        if (CharacterContext.Controller.IsSplitting)
        {
            return SplitState;
        }

        return null;
    }

    public override State PhysicsProcess(double delta)
    {
        ApplyGravity(delta);
        ApplyFriction(delta, 1024f);
        PerformJump();
        ApplyMovement(delta);
        CharacterContext.MoveAndSlide();
        RecalculateExternalVelocity();

        // check if in air
        if (!CharacterContext.IsOnFloor())
        {
            return AirState;
        }

        return null;
    }
}
