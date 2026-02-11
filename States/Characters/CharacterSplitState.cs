using System;
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

    private double _timeLeft;

    private double _cooldownEndTime;

    public double CooldownRemaining
    {
        get
        {
            return Math.Max(0.0, _cooldownEndTime - CurrentTime);
        }
    }

    public bool CanSplit => CurrentTime >= _cooldownEndTime;

    private double CurrentTime => Time.GetTicksMsec() / 1000.0;

    public override State Enter(State previousState)
    {
        if (CharacterContext.Cloneable is null)
        {
            return IdleState;
        }

        _inputDirection = CharacterContext.Controller
            .MovementInput
            .Normalized();

        _timeLeft = 0.1;

        // when performing a split
        if (PerformSplit())
        {
            _cooldownEndTime = CurrentTime + CharacterContext.Cloneable.SplitCooldownDuration;
        }

        return null;
    }

    public override State Process(double delta) => null;

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
        var cloneable = CharacterContext.Cloneable;
        if (cloneable is null || !CharacterContext.Controller.IsSplitting || !CanSplit)
        {
            return false;
        }

        if (cloneable.Mirror is null)
        {
            var clone = cloneable.Split();
            return clone is not null;
        }

        if (!cloneable.IsClone)
        {
            cloneable.Merge();
        }

        return false;
    }
}
