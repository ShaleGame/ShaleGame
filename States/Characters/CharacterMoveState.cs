using Godot;

namespace CrossedDimensions.States.Characters;

/// <summary>
/// State for when the character is moving on the ground.
/// </summary>
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
            var cloneable = CharacterContext.Cloneable;

            if (SplitState is not CharacterSplitState splitState)
            {
                return null;
            }

            if (cloneable == null)
            {
                return null;
            }

            if (cloneable.Mirror is null)
            {
                if (splitState.CanSplit)
                {
                    return SplitState;
                }
            }
            else if (!cloneable.IsClone)
            {
                cloneable.Merge();
            }
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

        if (!CharacterContext.AllowJumpInput)
        {
            //reset allow jumping input if on ground
            CharacterContext.AllowJumpInput = true;
        }

        return null;
    }
}
