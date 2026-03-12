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
    public float speed = 600f;

    private Character _goat;

    private AnimatedSprite2D _animSprite;

    private int _direction = -1;

    private Hitbox _hitbox;

    private StateMachine _selfMachine;
    private StateMachine _brainMachine;

    public override State Enter(State previousState)
    {
        _goat = Context as Character;

        _animSprite = _goat.FindChild("AnimatedSprite2D") as AnimatedSprite2D;
        _animSprite.Play("Rush");

        _hitbox = FindChild("Hitbox") as Hitbox;

        _direction = _animSprite.FlipH ? 1 : -1;

        _selfMachine = GetParent<StateMachine>();

        _brainMachine = _goat.FindChild("Brain") as StateMachine;

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

            return _walk;
        }

        _goat.Velocity = new Vector2((float)(delta * speed), 0);

        _goat.MoveAndSlide();

        return base.PhysicsProcess(delta);
    }

}
