using Godot;

namespace CrossDimensions.States.Characters;

/// <summary>
/// State for when the character is in the air.
/// </summary>
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
            if (CharacterContext.Cloneable?.Mirror is null)
            {
                return SplitState;
            }
            else if (!CharacterContext.Cloneable.IsClone)
            {
                CharacterContext.Cloneable.Merge();
            }
        }

        return null;
    }

    public override State PhysicsProcess(double delta)
    {
        ApplyGravity(delta);
        PerformJump();
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
