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

    [Export]
    public State MergeHoldState { get; set; }

    public override State Process(double delta)
    {
        var controller = CharacterContext.Controller;
        var cloneable = CharacterContext.Cloneable;

        if (CharacterContext.IsFrozen)
        {
            return null;
        }

        if (controller.IsSplitting && cloneable != null)
        {
            if (SplitState is not CharacterSplitState splitState)
            {
                return null;
            }

            if (cloneable.Mirror is null)
            {
                if (controller.IsMoving && splitState.CanSplit)
                {
                    return SplitState;
                }
            }
            else if (!cloneable.IsClone)
            {
                if (MergeHoldState is not null)
                {
                    return MergeHoldState;
                }
                cloneable.Merge();
            }
        }

        return null;
    }

    public override State PhysicsProcess(double delta)
    {
        if (CharacterContext.IsFrozen)
        {
            return null;
        }

        ApplyGravity(delta);
        PerformJump();
        ApplyMovement(delta);
        float preCollisionVerticalVelocity = CharacterContext.Velocity.Y;
        CharacterContext.MoveAndSlide();
        bool corrected = ApplyUpwardCornerCorrection();
        RestoreUpwardVelocityAfterCornerCorrection(corrected, preCollisionVerticalVelocity);
        RecalculateExternalVelocity();

        // check if grounded
        if (CharacterContext.IsOnFloor())
        {
            return IdleState;
        }

        return null;
    }
}
