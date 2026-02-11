using Godot;

namespace CrossedDimensions.States.Characters;

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
        var controller = CharacterContext.Controller;
        var cloneable = CharacterContext.Cloneable;

        if (controller.IsSplitting && controller.IsMoving)
        {
            var splitState = SplitState as CharacterSplitState;
            if (cloneable?.Mirror is null &&
                (splitState is null || splitState.CanSplit))
            {
                return SplitState;
            }
            // if mirror exists, merge unless there is no cloneable component
            else if (!cloneable?.IsClone ?? false)
            {
                cloneable.Merge();
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
