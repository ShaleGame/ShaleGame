using Godot;
using System;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using CrossedDimensions.BoundingBoxes;

namespace CrossedDimensions.Entities.Bosses.Siracus;

// Timed waiting between different attacks

// Also handles spawning in

public partial class Idle : State
{
    
    private bool _spawnedIn = false;

    private Character _siracus;

    private double _timeBetween = 3;
    private double _currentTime = 0;

    private State _attacking;
    private State _change;

    private AnimatedSprite2D _animSprite;

    [Export]
    public Hurtbox hurt;
    
    [Export]
    public Hitbox hit;

    private RandomNumberGenerator rng;

    
    public override State Enter(State previousState)
    {

        _siracus = Context as Character;

        _animSprite = _siracus.FindChild("AnimatedSprite2D") as AnimatedSprite2D;
        _animSprite.AnimationFinished += AnimationFinished;

        // make sure Siracus spawns at SiracusHole (strictly named that)
        if (!_spawnedIn || _animSprite.Visible == false)
        {

            var spawnPoint = GetTree().GetFirstNodeInGroup("SiracusHole") as Node2D;

            var attackStateMachine = _siracus.FindChild("Attacks") as StateMachine;

            if (spawnPoint != null && !_spawnedIn)
            {

                _siracus.GlobalPosition = spawnPoint.GlobalPosition;

                _spawnedIn = true;

            }

            _animSprite.Play("Emerge");
            _animSprite.Visible = true;

            SetCollisionActive(true);
        } else
        {
            _animSprite.Play("Idle");
        }

        _currentTime = 0;

        _attacking = GetParent().GetNode<State>("Attacking");
        _change = GetParent().GetNode<State>("Change");

        rng = new RandomNumberGenerator();

        return base.Enter(previousState);
    }

    // Create a timer while idle and then send to attacking when it wants to attack

    public override State Process(double delta)
    {
        _currentTime += delta;

        if (_currentTime >= _timeBetween)
        {
            var coinFlip = rng.RandiRange(0, 4);

            if (coinFlip == 4)
            {
                return _change;
            } else
            {
                return _attacking;
            }

            
        }

        return base.Process(delta);
    }

    private void AnimationFinished()
    {
        if (_animSprite.Animation == "Emerge")
        {
            _animSprite.Play("Idle");
        }
    }

    public override void Exit(State nextState)
    {
        if (_animSprite != null)
            _animSprite.AnimationFinished -= AnimationFinished;
        
        base.Exit(nextState);
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
