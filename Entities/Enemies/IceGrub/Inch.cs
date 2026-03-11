using Godot;
using System;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Enemies.IceGrub;

public partial class Inch : State
{

    [Export]
    public RayCast2D wallRaycast;

    [Export]
    public RayCast2D floorRaycast;

    [Export]
    public double inchTime = 0.5f;

    [Export]
    public float inchSpeed = 100f;

    [Export]
    public float gravity = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");

    private double _curTime = 0.0f;
    private AnimatedSprite2D _animSprite;

    private Character _iceGrub;

    private int direction = -1;

    public override State Enter(State previousState)
    {
        _iceGrub = Context as Character;

        _animSprite = _iceGrub.FindChild("AnimatedSprite2D") as AnimatedSprite2D;

        _animSprite.Frame = 0;

        _curTime = 0.0f;

        return base.Enter(previousState);
    }

    public override State PhysicsProcess(double delta)
    {
        _curTime += delta;

        bool canTurn = _curTime > inchTime * 0.5;

        if (canTurn && ((wallRaycast.IsColliding()) || (!floorRaycast.IsColliding() && _iceGrub.IsOnFloor())))
        {
            direction *= -1;

            _animSprite.FlipH = direction > 0 ? true : false;

            wallRaycast.TargetPosition = new Vector2(wallRaycast.TargetPosition.X * -1, wallRaycast.TargetPosition.Y);
            floorRaycast.Position = new Vector2(floorRaycast.Position.X * -1, floorRaycast.Position.Y);

            _curTime = 0;
        }

        if (_iceGrub.IsOnFloor())
        {
            float t = (float)(_curTime % inchTime / inchTime);

            float speedCurve = (float)Math.Sin(t * Math.PI);

            _iceGrub.Velocity = new Vector2(direction * inchSpeed * speedCurve, _iceGrub.Velocity.Y);
        }
        else
        {
            _iceGrub.Velocity = new Vector2(_iceGrub.Velocity.X, _iceGrub.Velocity.Y + (float)(gravity * delta));
        }

        _iceGrub.MoveAndSlide();

        return base.PhysicsProcess(delta);
    }

}
