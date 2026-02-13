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

    public override State Enter(State previousState)
    {
        if (CharacterContext.Controller.IsMoving)
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

        return null;
    }

    public override State Process(double delta)
    {
        if (CharacterContext.Controller.IsMoving)
        {
            return MoveState;
        }

        if (CharacterContext.Controller.IsSplitting)
        {
            var cloneable = CharacterContext.Cloneable;
            if (cloneable is not null && !cloneable.IsClone)
            {
                CharacterContext.Cloneable.Merge();
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
