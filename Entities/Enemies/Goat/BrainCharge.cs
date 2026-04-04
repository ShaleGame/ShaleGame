using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Enemies;

public partial class BrainCharge : State
{
    private Character _goat;

    [Export]
    public State Run { get; set; }

    private AnimatedSprite2D _animSprite;

    private const double ChargeTime = 1.0;
    private double _currentTime;

    public override State Enter(State previousState)
    {
        _goat = Context as Character;
        if (_goat is null)
        {
            return null;
        }

        _animSprite = _goat.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        if (_animSprite is null)
        {
            return null;
        }

        _animSprite.Play("Charge");

        _goat.Speed = 0;

        _currentTime = 0;

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        _currentTime += delta;

        if (_currentTime >= ChargeTime)
        {
            return Run;
        }

        return base.Process(delta);
    }
}
