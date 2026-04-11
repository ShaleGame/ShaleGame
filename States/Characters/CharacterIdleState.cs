using Godot;

namespace CrossedDimensions.States.Characters;

/// <summary>
/// State for when the character is idle on the ground.
/// </summary>
public partial class CharacterIdleState : CharacterState
{
    [Export]
    public State MoveState { get; set; }

    [Export]
    public State AirState { get; set; }

    [Export]
    public State MergeHoldState { get; set; }

    [Export]
    public State SplitState { get; set; }

    public override State Enter(State previousState)
    {
        if (HasHorizontalMovementInput())
        {
            return MoveState;
        }

        // We need this check because a state like SplitState can just enter
        // to idle without checking if we are on the floor. Without this check,
        // the player can perform an extra instant jump after splitting in
        // mid-air.
        if (!CharacterContext.IsOnFloor())
        {
            return AirState;
        }

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (HasHorizontalMovementInput())
        {
            return MoveState;
        }

        // if splitting and movement key is held down
        var controller = CharacterContext.Controller;
        if (controller.IsSplitting && !controller.MovementInput.IsZeroApprox())
        {
            var cloneable = CharacterContext.Cloneable;
            if (cloneable is not null && !cloneable.IsClone)
            {
                if (cloneable.Mirror is null)
                {
                    if (SplitState is CharacterSplitState splitState && splitState.CanSplit)
                    {
                        return SplitState;
                    }
                }
                else
                {
                    if (MergeHoldState is not null)
                    {
                        return MergeHoldState;
                    }
                    CharacterContext.Cloneable.Merge();
                }
            }
        }

        return base.Process(delta);
    }

    public override State PhysicsProcess(double delta)
    {
        if (CharacterContext.IsFrozen)
        {
            var frozenNext = base.PhysicsProcess(delta);
            if (frozenNext is not null)
            {
                return frozenNext;
            }

            return null;
        }

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

        if (!CharacterContext.AllowJumpInput)
        {
            //reset allow jumping input if on ground
            CharacterContext.AllowJumpInput = true;
        }

        var next = base.PhysicsProcess(delta);
        if (next is not null)
        {
            return next;
        }

        return null;
    }

    private bool HasHorizontalMovementInput()
    {
        return !Mathf.IsZeroApprox(CharacterContext.Controller.MovementInput.X);
    }
}
