using Godot;
using System;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using CrossedDimensions.BoundingBoxes;

namespace CrossedDimensions.Entities.Bosses.Siracus;

// Disappears into hole and tail comes out of the ice floor to attack player

public partial class Tail : State
{

    private Character _siracus;
    private Character _player;

    private AnimatedSprite2D _animSprite;

    private double _curTime = 0;
    [Export]
    public double maxTime = 1.0f;

    private int _curTails = 0;
    [Export]
    public int maxTails = 5;

    private bool _TailCurrentlyUp = false;

    [Export]
    public PackedScene tail;

    [Export]
    public Hurtbox hurt;
    
    [Export]
    public Hitbox hit;

    public override State Enter(State previousState)
    {
        // Enter hole, set default vars

        _siracus = Context as Character;
        _player = GetTree().GetFirstNodeInGroup("Player") as Character;

        _animSprite = _siracus.FindChild("AnimatedSprite2D") as AnimatedSprite2D;
        _animSprite.AnimationFinished += AnimationFinished;

        _animSprite.PlayBackwards("Emerge");

        _curTime = 0;
        _curTails = 0;

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (!_TailCurrentlyUp)
        {
            _curTime += delta;
        }

        if (_curTime >= maxTime && _player.IsOnFloor())
        {
            var playerPos = _player.GlobalPosition;

            var xCord = playerPos.X;

            var tailScene = tail.Instantiate() as SiracusTail;
            
            tailScene.GlobalPosition = new Vector2(xCord, tailScene.GlobalPosition.Y + 50);

            tailScene.TailDespawned += TailDespawned;

            _curTails++;
            _TailCurrentlyUp = true;
        }

        return base.Process(delta);
    }

    public void TailDespawned()
    {
        _TailCurrentlyUp = false;

        if (_curTails >= maxTails)
        {
            var stateMachine = GetParent() as StateMachine;

            State attackIdle = stateMachine.FindChild("Idle") as State;

            stateMachine.ChangeState(attackIdle);
        }
    }

    public void AnimationFinished()
    {
        if (_animSprite.Animation == "Emerge" && _animSprite.SpeedScale < 0)
        {
            _animSprite.Visible = false;

            hurt.Monitorable = false;
            hurt.Monitoring = false;

            hit.Monitorable = false;
            hit.Monitoring = false;

        }
    }
}
