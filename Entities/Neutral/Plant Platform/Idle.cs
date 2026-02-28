using Godot;
using CrossedDimensions.States;
using System;
using CrossedDimensions.Characters;

// plant just hovers until a player steps on its area

public partial class Idle : State
{
    
    private Character _plantPlatform;

    private Area2D _area;

    private AnimatedSprite2D _sprite;

    private Callable _bodyEnteredCallable;

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

        return base.Enter(previousState);
    }

    private async void OnBodyEntered(Node body)
    {
        // Transition to Descend state

        // Wait before transitioning
        await ToSignal(GetTree().CreateTimer(0.2f), "timeout");

        var parentSM = GetParent() as StateMachine;
        parentSM?.ChangeState("Descend");
    }
}
