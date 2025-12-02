using Godot;

namespace CrossDimensions.States.Characters;

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
            if (CharacterContext.Cloneable?.Mirror is null)
            {
                return SplitState;
            }
            else
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
            GD.Print("$On ground (moving), restoring jump input");
        }

        return null;
    }
}
