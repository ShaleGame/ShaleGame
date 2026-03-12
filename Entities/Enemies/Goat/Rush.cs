using Godot;
using System;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using CrossedDimensions.BoundingBoxes;

namespace CrossedDimensions.Entities.Enemies;

// Goat is rushing at player until it hits a wall or no floor

public partial class Rush : State
{

    [Export]
    public RayCast2D wallRaycast;

    [Export]
    public RayCast2D floorRaycast;

    [Export]
    public float speed = 30000;

    private Character _goat;

    private AnimatedSprite2D _animSprite;

    private int _direction = -1;

    private Hitbox _hitbox;

    private StateMachine _selfMachine;
    private StateMachine _brainMachine;

    private bool _collided = false;

    public override State Enter(State previousState)
    {
        _goat = Context as Character;

        _animSprite = _goat.FindChild("AnimatedSprite2D") as AnimatedSprite2D;
        _animSprite.Play("Rush");

        _hitbox = _goat.FindChild("RushHitbox") as Hitbox;
        _hitbox.AreaEntered += OnCollision;

        _direction = _animSprite.FlipH ? 1 : -1;

        _selfMachine = GetParent<StateMachine>();

        _brainMachine = _goat.FindChild("Brain") as StateMachine;

        changeMonitoring(true);

        _collided = false;

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

            DetectPlayer _noPlayer = _brainMachine.FindChild("DetectPlayer") as DetectPlayer;
            _noPlayer.UndetectPlayer();

            State _walk = _selfMachine.FindChild("Walk") as State;

            changeMonitoring(false);

            return _walk;
        }

        if (_collided)
        {
            DetectPlayer _noPlayer = _brainMachine.FindChild("DetectPlayer") as DetectPlayer;
            _noPlayer.UndetectPlayer();

            State _walk = _selfMachine.FindChild("Walk") as State;

            changeMonitoring(false);

            return _walk;
        }

        _goat.Velocity = new Vector2((float)(delta * speed * _direction), 0);

        _goat.MoveAndSlide();

        return base.PhysicsProcess(delta);
    }

    public override void Exit(State nextState)
    {
        _hitbox.AreaEntered -= OnCollision;

        base.Exit(nextState);
    }

    private void changeMonitoring(bool active)
    {
        _hitbox.Monitorable = active;
        _hitbox.Monitorable = active;
    }

    private void OnCollision(Area2D area)
    {
        _collided = true;
    }

}
