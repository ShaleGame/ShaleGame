using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using System;


public partial class Rise : State
{
    
    private Character _plantPlatform;

    private Area2D _area;

    private AnimatedSprite2D _sprite;

    private Callable _bodyEnteredCallable;

    private Vector2 originPoint;

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
        originPoint = _idle.originPoint;

        _sprite.Play("Descending");

        // Avoid duplicate signal attachments
        if (!_area.IsConnected(Area2D.SignalName.BodyEntered, _bodyEnteredCallable))
        {
            _area.Connect(Godot.Area2D.SignalName.BodyEntered, _bodyEnteredCallable);
        }

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        // Move the platform upwards until it reaches its original position

        if (_plantPlatform != null)
        {
            _plantPlatform.GlobalPosition -= new Godot.Vector2(0, AscendSpeed * (float)delta);

            if (_plantPlatform.GlobalPosition.Y <= originPoint.Y)
            {
                _plantPlatform.GlobalPosition = originPoint;
                var parentSM = GetParent() as StateMachine;
                parentSM?.ChangeState("Idle");
            }
        }

        return base.Process(delta);
    }

    private async void OnBodyEntered(Node body)
    {
        // Transition to Descend state

        var parentSM = GetParent() as StateMachine;
        parentSM?.ChangeState("Descend");
    }
}