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

    
    public override State Enter(State previousState)
    {
        _siracus = Context as Character;

        // make sure Siracus spawns at SiracusHole (strictly named that)
        if (!_spawnedIn)
        {
            var spawnPoint = GetTree().Root.FindChild("SiracusHole") as Node2D;

            if (spawnPoint != null)
            {
                _siracus.GlobalPosition = spawnPoint.GlobalPosition;

                _spawnedIn = true;
            }
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
}
