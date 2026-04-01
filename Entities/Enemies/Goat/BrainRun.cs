using Godot;
using System;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using CrossedDimensions.Characters.Controllers;

namespace CrossedDimensions.Entities.Enemies;

public partial class BrainRun : State
{

    private Character _goat;
    private EnemyController _controller;

    [Export]
    private State detect;

    [Export]
    public RayCast2D floorRaycast;
    [Export]
    public RayCast2D wallRaycast;

    private int _direction = -1;
    private AnimatedSprite2D _animSprite;

    public override State Enter(State previousState)
    {
        _goat = Context as Character;
        _controller = _goat.Controller as EnemyController;

        _animSprite = _goat.FindChild("AnimatedSprite2D") as AnimatedSprite2D;

        _goat.Speed = 300;

        _direction = _animSprite.FlipH ? 1 : -1;

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (wallRaycast.IsColliding() || !floorRaycast.IsColliding())
        {
            _direction *= -1;

            wallRaycast.TargetPosition = new Vector2(wallRaycast.TargetPosition.X * -1, wallRaycast.TargetPosition.Y);

            floorRaycast.Position = new Vector2(floorRaycast.Position.X * -1, floorRaycast.Position.Y);

            _animSprite.FlipH = _direction > 0;

            if (_controller != null && _controller.HasMethod("SetMovementInput"))
            {
                _controller.SetMovementInput(new Vector2(_direction, 0));
            }

            var stateMachine = GetParent() as StateMachine;
            stateMachine.ChangeState(detect);
        }

        return base.Process(delta);
    }

}
