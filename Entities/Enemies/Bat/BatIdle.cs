using CrossedDimensions.Characters;
using Godot;
using System;
using System.Threading;

namespace CrossedDimensions.States.Enemies;

[GlobalClass]
public partial class BatIdle : State
{
    [Export] public float DetectionRadius = 200f;
    private Godot.Timer spotPlayerTimer;

    private Character _bat;
    private Node2D _player;
    private State _attacking;

    private bool spottedPlayer;

    public override State Enter(State previousState)
    {
        _bat = Context as Character;

        // Get reference to Attacking state
        _attacking = GetParent().GetNode<State>("Attacking");

        // Get player reference
        _player = GetTree().GetFirstNodeInGroup("Player") as Node2D;

        // Tell movement state machine to hang
        var movementSh = _bat?.GetNode<StateMachine>("MovementStateMachine");
        movementSh?.ChangeState("Hanging");

        spotPlayerTimer = _bat.GetNode<Godot.Timer>("SpotPlayerTimer");
        // Avoid duplicate signal attachments
        if (!spotPlayerTimer.IsConnected(Godot.Timer.SignalName.Timeout, new Callable(this, nameof(TimerSetOff))))
        {
            spotPlayerTimer.Timeout += TimerSetOff;
        }

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        // Check if player is within detection radius
        if (_bat != null && _player != null)
        {
            float distanceToPlayer = _bat.GlobalPosition.DistanceTo(_player.GlobalPosition);
            if (distanceToPlayer <= DetectionRadius)
            {

                if (spottedPlayer)
                {
                    return _attacking;
                } else if (spotPlayerTimer.IsStopped())
                {
                    spotPlayerTimer.Start();
                } else
                {
                }
            }

            spottedPlayer = false;
        }

        return base.Process(delta);
    }

    public void TimerSetOff()
    {
        spottedPlayer = true;
    }
}
