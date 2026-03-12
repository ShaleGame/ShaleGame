using Godot;
using System;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Enemies;

// Idle walk/patrol

public partial class Walk : State
{
    
    [Export]
    public RayCast2D wallRaycast;

    [Export]
    public RayCast2D floorRaycast;

    [Export]
    public float speed = 300f;

    private Character _goat;

    private int _direction = -1;

    private AnimatedSprite2D _animSprite;

    public override State Enter(State previousState)
    {
        _goat = Context as Character;

        _animSprite = _goat.FindChild("AnimatedSprite2D") as AnimatedSprite2D;
        _animSprite.Play("Walking");

        _direction = _animSprite.FlipH ? 1 : -1;

        return base.Enter(previousState);
    }

    public override State PhysicsProcess(double delta)
    {
        if (wallRaycast.IsColliding() || !floorRaycast.IsColliding())
        {
            _direction *= -1;

            wallRaycast.TargetPosition = new Vector2(wallRaycast.TargetPosition.X * -1, wallRaycast.TargetPosition.Y);

            floorRaycast.Position = new Vector2(floorRaycast.Position.X * -1, floorRaycast.Position.Y);

            _animSprite.FlipH = _direction > 0 ? true : false;
        }

        _goat.Velocity = new Vector2((float)(delta * speed), 0);

        _goat.MoveAndSlide();

        return base.PhysicsProcess(delta);
    }

}
