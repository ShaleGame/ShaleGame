using Godot;
using System;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using CrossedDimensions.BoundingBoxes;

public partial class Change : State
{
    
    private Godot.Collections.Array<Node> holes;

    private Character _siracus;

    private AnimatedSprite2D _animSprite;

    [Export]
    public Hurtbox hurt;
    
    [Export]
    public Hitbox hit;

    private RandomNumberGenerator rng;

    private double _curTime = 0;
    private double _maxTime = 1.0f;

    private bool _hiding = false;

    public override State Enter(State previousState)
    {
        _siracus = Context as Character;

        _animSprite = _siracus.FindChild("AnimatedSprite2D") as AnimatedSprite2D;
        _animSprite.AnimationFinished += AnimationFinished;

        _animSprite.PlayBackwards("Emerge");

        holes = GetTree().GetNodesInGroup("SiracusHole");

        rng = new RandomNumberGenerator();

        _hiding = false;

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (_hiding)
        {
            _curTime += delta;
        }

        if (_curTime >= _maxTime)
        {
            var newHole = holes[rng.RandiRange(0, holes.Count - 1)] as Node2D;

            if (newHole != null)
            {
                _siracus.GlobalPosition = newHole.GlobalPosition;

                _animSprite.AnimationFinished -= AnimationFinished;

                var stateMachine = GetParent() as StateMachine;

                var idleState = stateMachine.FindChild("Idle") as State;

                _curTime = 0;

                return idleState;
            }
        }

        return base.Process(delta);
    }

    private void AnimationFinished()
    {
        if (_animSprite.Animation == "Emerge")
        {
            GD.Print("Hi!");

            _animSprite.Visible = false;
            SetCollisionActive(false);
            _hiding = true;
            
        }
    }

    private void SetCollisionActive(bool active)
    {
        hurt.Monitorable = active;
        hurt.Monitoring = active;
        hit.Monitorable = active;
        hit.Monitoring = active;
        var collisionShape = _siracus.FindChild("CollisionShape2D") as CollisionShape2D;
        collisionShape.Disabled = !active;
    }
}
