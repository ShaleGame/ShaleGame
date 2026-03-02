using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using System;

namespace CrossedDimensions.Entities.Neutral.PlantPlatform;

public partial class Rise : State
{

    private Character _plantPlatform;

    private Area2D _area;

    private AnimatedSprite2D _sprite;

    private Callable _bodyEnteredCallable;

    public int NumOfPlayers = 0;

    private Vector2 _originPoint;

    [Export] public float AscendSpeed { get; set; } = 50f;

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

        Idle _idle = GetParent().GetNode<Idle>("Idle");
        _originPoint = _idle.OriginPoint;

        NumOfPlayers = 0;

        // Descending animation works for both descending and rising
        _sprite.Play("Descending");

        // Avoid duplicate signal attachments
        if (!_area.IsConnected(Area2D.SignalName.BodyEntered, _bodyEnteredCallable))
        {
            _area.Connect(Godot.Area2D.SignalName.BodyEntered, _bodyEnteredCallable);
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
        }

        base.Exit(nextState);
    }

    public override State Process(double delta)
    {
        // Move the platform upwards until it reaches its original position

        if (_plantPlatform == null)
        {
            return base.Process(delta);
        }

        _plantPlatform.Velocity = new Vector2(0, -AscendSpeed);
        _plantPlatform.MoveAndSlide();

        if (_plantPlatform.GlobalPosition.Y <= _originPoint.Y)
        {
            _plantPlatform.GlobalPosition = _originPoint;
            _plantPlatform.Velocity = Vector2.Zero;
            GetParent<StateMachine>()?.ChangeState("Idle");
        }

        return base.Process(delta);
    }

    private void OnBodyEntered(Node body)
    {
        // Transition to Descend state

        var parentSM = GetParent() as StateMachine;
        parentSM?.ChangeState("Descend");
    }

}
