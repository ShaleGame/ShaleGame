using Godot;
using System;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.Siracus;

// Timed waiting between different attacks

// Also handles spawning in

public partial class Idle : State
{
    
    private bool _spawnedIn = false;

    private Character _siracus;

    private double _timeBetween = 10;
    private double _currentTime = 0;

    private State _attacking;

    private AnimatedSprite2D _animSprite;

    
    public override State Enter(State previousState)
    {
        _siracus = Context as Character;

        _animSprite = _siracus.FindChild("AnimatedSprite2D") as AnimatedSprite2D;
        _animSprite.AnimationFinished += AnimationFinished;

        // make sure Siracus spawns at SiracusHole (strictly named that)
        if (!_spawnedIn || _animSprite.Visible == false)
        {
            var spawnPoint = GetTree().Root.FindChild("SiracusHole") as Node2D;

            if (spawnPoint != null)
            {
                _siracus.GlobalPosition = spawnPoint.GlobalPosition;

                _spawnedIn = true;

                _animSprite.Play("Emerge");
                _animSprite.Visible = true;
            }
        } else
        {
            _animSprite.Play("Idle");
        }

        _currentTime = 0;

        _attacking = GetParent().GetNode<State>("Attacking");

        return base.Enter(previousState);
    }

    // Create a timer while idle and then send to attacking when it wants to attack

    public override State Process(double delta)
    {
        _currentTime += delta;

        if (_currentTime >= _timeBetween)
        {
            return _attacking;
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
        _animSprite.AnimationFinished -= AnimationFinished;
        
        base.Exit(nextState);
    }
}
