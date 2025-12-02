using Godot;

namespace CrossDimensions.States.Characters;

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
            if (!(CharacterContext.Cloneable?.IsClone ?? false))
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
            GD.Print("$On ground (idle), restoring jump input");
        }

        return null;
    }
}
