using Godot;
using System;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using System.Threading.Tasks;

namespace CrossedDimensions.Entities.Enemies;

public partial class BrainCharge : State
{

    private Character _goat;

    [Export]
    public State run;

    private int _direction = -1;
    private AnimatedSprite2D _animSprite;

    private double _chargeTime = 1;
    private double _curTime = 0;

    public override State Enter(State previousState)
    {
        _goat = Context as Character;

        _animSprite = _goat.FindChild("AnimatedSprite2D") as AnimatedSprite2D;

        _animSprite.Play("Charge");

        _goat.Speed = 0;

        _curTime = 0;

        return base.Enter(previousState);
    }


    public override State Process(double delta)
    {
        _curTime += delta;

        if (_curTime >= _chargeTime)
        {
            return run;
        }

        return base.Process(delta);
    }


}
