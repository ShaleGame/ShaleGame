using Godot;
using CrossedDimensions.States;
using System;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Neutral.PlantPlatform;

public partial class Idle : State
{

    private Character _plantPlatform;

    private Area2D _area;

    private AnimatedSprite2D _sprite;

    private Callable _bodyEnteredCallable;
    private Callable _onBodyExitedCallable;
    private bool _playerOnPlatform = false;

    private double idleTime = 0;
    private double idleDuration = 0.2; // Time to wait before descending

    public Vector2 originPoint;

    public override void _Ready()
    {
        _bodyEnteredCallable = new Callable(this, nameof(OnBodyEntered));

        base._Ready();
    }

    public override State Enter(State previousState)
    {
        _plantPlatform = Context as Character;
        _area = _plantPlatform.GetNode<Area2D>("CollisionZone");
        _sprite = _plantPlatform.GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        originPoint = _plantPlatform.GlobalPosition;

        _sprite.Play("Idle");

        // Avoid duplicate signal attachments
        if (!_area.IsConnected(Area2D.SignalName.BodyEntered, _bodyEnteredCallable))
        {
            _area.Connect(Godot.Area2D.SignalName.BodyEntered, _bodyEnteredCallable);
        }

        if (!_area.IsConnected(Area2D.SignalName.BodyExited, _onBodyExitedCallable))
        {
            _area.Connect(Godot.Area2D.SignalName.BodyExited, _onBodyExitedCallable);
        }

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        // Wait for player to step on platform, then wait a moment before descending

        if (_playerOnPlatform)
        {
            idleTime += delta;
        }

        if (idleTime >= idleDuration)
        {
            var parentSM = GetParent() as StateMachine;
            parentSM?.ChangeState("Descend");
        }

        return base.Process(delta);
    }

    private void OnBodyEntered(Node body)
    {
        if (body is Character)
        {
            _playerOnPlatform = true;
        }
    }

    private void OnBodyExited(Node body)
    {
        if (body is Character)
        {
            _playerOnPlatform = false;
            idleTime = 0; // Reset idle time if player steps off
        }
    }
}
