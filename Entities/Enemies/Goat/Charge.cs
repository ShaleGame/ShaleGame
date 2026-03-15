using Godot;
using System;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Enemies;

// Goat charges up before rushing

public partial class Charge : State
{

    private Character _goat;

    private AnimatedSprite2D _animSprite;

    private double _chargeTime = 1;
    private double _curTime = 0;

    public override State Enter(State previousState)
    {
        _goat = Context as Character;

        _animSprite = _goat.FindChild("AnimatedSprite2D") as AnimatedSprite2D;
        _animSprite.Play("Charge");

        _curTime = 0;

        _goat.Velocity = new Vector2(0, 0);

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        _curTime += delta;

        Vector2 externalForces = new Vector2(_goat.VelocityFromExternalForces.X * 100f, 0);
        _goat.VelocityFromExternalForces = _goat.VelocityFromExternalForces.MoveToward(Vector2.Zero, (float)(delta * 800f));
        _goat.Velocity = new Vector2(0,0) + externalForces;
        
        _goat.MoveAndSlide();

        if (_curTime >= _chargeTime)
        {
            StateMachine stateMachine = GetParent<StateMachine>();

            State _rushState = stateMachine.FindChild("Rush") as State;

            if (_rushState != null)
            {
                stateMachine.ChangeState(_rushState);
            }
        }

        return base.Process(delta);
    }

}
