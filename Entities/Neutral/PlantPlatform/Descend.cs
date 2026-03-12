using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using System;

namespace CrossedDimensions.Entities.Neutral.PlantPlatform;

public partial class Descend : State
{
    private Character _plantPlatform;

    private Area2D _area;

    private AnimatedSprite2D _sprite;

    private Callable _bodyExitedCallable;
    private Callable _bodyEnteredCallable;

    private int _numOfPlayers = 0;

    [Export] public float DescendSpeed { get; set; } = 50f;

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

        _sprite.Play("Descending");

        if (previousState is Idle)
        {
            Idle _idle = GetParent().GetNode<Idle>("Idle");

            _numOfPlayers = _idle.NumOfPlayers;
        }

        if (previousState is Rise)
        {
            Rise _idle = GetParent().GetNode<Rise>("Rise");

            _numOfPlayers = _idle.NumOfPlayers;
        }

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

    public override State Process(double delta)
    {
        // Move the platform downards until the player steps off or it hits an object below it

        if (_plantPlatform == null)
        {
            return base.Process(delta);
        }

        if (_plantPlatform.Freezable.IsFrozen)
        {
            _plantPlatform.Velocity = Vector2.Zero;
            return base.Process(delta);
        }

        _plantPlatform.Velocity = new Vector2(0, DescendSpeed);
        _plantPlatform.MoveAndSlide();

        if (_plantPlatform.IsOnFloor())
        {
            _plantPlatform.Velocity = Vector2.Zero;
        }

        return base.Process(delta);
    }

    private void OnBodyEntered(Node body)
    {
        if (body is Character)
        {
            _numOfPlayers++;
        }
    }

    private void OnBodyExited(Node body)
    {
        if (body is Character)
        {
            _numOfPlayers = Math.Max(0, _numOfPlayers - 1);

            if (_numOfPlayers == 0)
            {
                GetParent<StateMachine>()?.ChangeState("Rise");
            }
        }
    }
}
