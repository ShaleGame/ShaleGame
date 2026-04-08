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

    private Marker2D _yPosition;

    private AnimatedSprite2D _animSprite;

    private double _curTime = 0;
    [Export]
    public double maxTime = 0.25f;

    private int _curTails = 0;
    [Export]
    public int maxTails = 3;

    private bool _IsTailActive = false;

    [Export]
    public PackedScene tail;

    [Export]
    public Hurtbox hurt;

    [Export]
    public Hitbox hit;

    public override State Enter(State previousState)
    {
        // Enter hole, set default vars

        GD.Print("Attacking");

        _siracus = Context as Character;
        _player = GetTree().GetFirstNodeInGroup("Player") as Character;

        // Uses bottom right of boss room marker for y coordinate
        _yPosition = GetTree().Root.FindChild("BottomRight", true, false) as Marker2D;

        _animSprite = _siracus.FindChild("AnimatedSprite2D") as AnimatedSprite2D;
        _animSprite.AnimationFinished += AnimationFinished;

        _animSprite.PlayBackwards("Emerge");

        _curTime = 0;
        _curTails = 0;
        _IsTailActive = false;

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (!_IsTailActive)
            _curTime += delta;

        if (_curTime >= maxTime && _player.IsOnFloor())
            SpawnTail();

        return base.Process(delta);
    }

    public override void Exit(State nextState)
    {
        if (_animSprite != null)
        {
            _animSprite.AnimationFinished -= AnimationFinished;
        }

        base.Exit(nextState);
    }

    private void SpawnTail()
    {

        var tailInstance = tail.Instantiate() as SiracusTail;

        float spawnX = _player.GlobalPosition.X;
        tailInstance.GlobalPosition = new Vector2(spawnX, _yPosition.GlobalPosition.Y);
        tailInstance.TailDespawned += OnTailDespawned;

        GetTree().CurrentScene.AddChild(tailInstance);

        _curTails++;
        _IsTailActive = true;
        _curTime = 0;
    }

    public void OnTailDespawned()
    {
        _IsTailActive = false;

        if (_curTails >= maxTails)
            TransitionToIdle();
    }

    private void TransitionToIdle()
    {
        var stateMachine = GetParent() as StateMachine;

        var idleState = stateMachine.FindChild("AttackIdle") as State;

        stateMachine.ChangeState(idleState);
    }

    public void AnimationFinished()
    {
        bool isReversedEmerge = _animSprite.Animation == "Emerge";
        if (!isReversedEmerge) return;

        GD.Print("Hi!");

        _animSprite.Visible = false;
        SetCollisionActive(false);
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
