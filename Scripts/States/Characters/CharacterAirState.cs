using Godot;

namespace CrossDimensions.States.Characters;

public sealed partial class CharacterAirState : CharacterState
{
    [Export]
    public State IdleState { get; set; }

    [Export]
    public State SplitState { get; set; }

    public override State Process(double delta)
    {
        if (CharacterContext.Controller.IsSplitting)
        {
            return SplitState;
        }

        return null;
    }

    public override State PhysicsProcess(double delta)
    {
        ApplyGravity(delta);
        ApplyMovement(delta);
        Vector2 initialVelocity = CharacterContext.Velocity;
        CharacterContext.MoveAndSlide();
        RecalculateExternalVelocity();

        // check if grounded
        if (CharacterContext.IsOnFloor())
        {
            return IdleState;
        }

        return null;
    }
}
