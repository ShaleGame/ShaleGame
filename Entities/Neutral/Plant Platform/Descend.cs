using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using System;

public partial class Descend : State
{
    private Character _plantPlatform;

    private Area2D _area;

    private AnimatedSprite2D _sprite;

    private Callable _bodyExitedCallable;

    [Export] public float DescendSpeed { get; set; } = 50f;

    public override void _Ready()
    {
        _bodyExitedCallable = new Callable(this, nameof(OnBodyExited));

        base._Ready();
    }

    public override State Enter(State previousState)
    {
        _plantPlatform = Context as Character;
        _area = _plantPlatform.GetNode<Area2D>("CollisionZone");
        _sprite = _plantPlatform.GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        _sprite.Play("Descending");

        // Avoid duplicate signal attachments
        if (!_area.IsConnected(Area2D.SignalName.BodyExited, _bodyExitedCallable))
        {
            _area.Connect(Godot.Area2D.SignalName.BodyExited, _bodyExitedCallable);
        }

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        // Move the platform downards until the player steps off or it hits an object below it

        if (_plantPlatform != null && _plantPlatform is Character)
        {
            _plantPlatform.Velocity = new Vector2(0, DescendSpeed);
            _plantPlatform.MoveAndSlide();

            if (_plantPlatform.IsOnFloor())
            {
                _plantPlatform.Velocity = Vector2.Zero;
            }
        }

        return base.Process(delta);
    }

    private void OnBodyExited(Node body)
    {
        // Transition back to Idle state
        var parentSM = GetParent() as StateMachine;
        parentSM?.ChangeState("Rise");
    }
}
