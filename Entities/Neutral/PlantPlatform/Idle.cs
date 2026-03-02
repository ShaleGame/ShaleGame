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
    private Callable _bodyExitedCallable;
    public int NumOfPlayers = 0;

    private double _idleTime = 0;
    private double _idleDuration = 0.2; // Time to wait before descending

    private bool _originSet = false;
    public Vector2 OriginPoint { get; private set; }

    public override void _Ready()
    {
        _bodyEnteredCallable = new Callable(this, nameof(OnBodyEntered));
        _bodyExitedCallable = new Callable(this, nameof(OnBodyExited));

        base._Ready();
    }

    public override State Enter(State previousState)
    {
        _plantPlatform = Context as Character;
        _area = _plantPlatform.GetNode<Area2D>("CollisionZone");
        _sprite = _plantPlatform.GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        if (!_originSet)
        {
            OriginPoint = _plantPlatform.GlobalPosition;
            _originSet = true;
        }

        _idleTime = 0;
        NumOfPlayers = 0;

        _sprite.Play("Idle");

        // Avoid duplicate signal attachments
        if (!_area.IsConnected(Area2D.SignalName.BodyEntered, _bodyEnteredCallable))
        {
            _area.Connect(Godot.Area2D.SignalName.BodyEntered, _bodyEnteredCallable);
        }

        if (!_area.IsConnected(Area2D.SignalName.BodyExited, _bodyExitedCallable))
        {
            _area.Connect(Godot.Area2D.SignalName.BodyExited, _bodyExitedCallable);
        }

        return base.Enter(previousState);
    }

    public override void Exit(State nextState)
    {
        if (_area != null)
        {
            if (_area.IsConnected(Area2D.SignalName.BodyEntered, _bodyEnteredCallable))
            {
                _area.Disconnect(Area2D.SignalName.BodyEntered, _bodyEnteredCallable);
            }

            if (_area.IsConnected(Area2D.SignalName.BodyExited, _bodyExitedCallable))
            {
                _area.Disconnect(Area2D.SignalName.BodyExited, _bodyExitedCallable);
            }
        }

        base.Exit(nextState);
    }

    public override State PhysicsProcess(double delta)
    {
        // Wait for player to step on platform, then wait a moment before descending

        if (NumOfPlayers > 0)
        {
            _idleTime += delta;
        }

        if (_idleTime >= _idleDuration)
        {
            _idleTime = 0;
            GetParent<StateMachine>()?.ChangeState("Descend");
        }

        return base.Process(delta);
    }

    private void OnBodyEntered(Node body)
    {
        if (body is Character)
        {
            NumOfPlayers++;
        }
    }

    private void OnBodyExited(Node body)
    {
        if (body is Character)
        {
            NumOfPlayers = Math.Max(0, NumOfPlayers - 1);

            if (NumOfPlayers == 0)
            {
                _idleTime = 0; // Reset idle time if no players are on the platform
            }
        }
    }
}
