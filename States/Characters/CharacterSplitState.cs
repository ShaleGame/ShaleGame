using Godot;

namespace CrossedDimensions.States.Characters;

/// <summary>
/// State for when the character is performing a split.
/// </summary>
public sealed partial class CharacterSplitState : CharacterState
{
    [Export]
    public State IdleState { get; set; }

    private Vector2 _inputDirection = Vector2.Zero;

    private CrossedDimensions.Characters.Character _clone;

    private double _timeLeft;

    private bool _doDash = false;

    public override State Enter(State previousState)
    {
        _inputDirection = CharacterContext.Controller
            .MovementInput
            .Normalized();

        _timeLeft = 0.1f;
        
        // when performing a split
        PerformSplit();

        return null;
    }

    public override State Process(double delta)
    {
        return null;
    }

    public override State PhysicsProcess(double delta)
    {
        CharacterContext.VelocityFromExternalForces = Vector2.Zero;
        CharacterContext.Velocity = _inputDirection * CharacterContext
            .Cloneable.SplitForce;
        CharacterContext.MoveAndSlide();

        if ((_timeLeft -= delta) <= 0)
        {
            return IdleState;
        }

        return null;
    }

    private bool PerformSplit()
    {
        if (CharacterContext.Controller.IsSplitting)
        {
            if (CharacterContext.Cloneable.Mirror is null)
            {
                var clone = CharacterContext.Cloneable.Split();
                return clone is not null;
            }
            else if (!CharacterContext.Cloneable.IsClone)
            {
                CharacterContext.Cloneable.Merge();
                return true;
            }
        }

        return false;
    }
}
