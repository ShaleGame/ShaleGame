using CrossedDimensions.Characters;
using Godot;
using System;
using System.Numerics;
using System.Threading;

namespace CrossedDimensions.States.Enemies;

[GlobalClass]
public partial class BatAttacking : State
{

    private Character _bat;
    private Node2D _player;
    private State _idle;
    private bool _hasSwooped;
    private Godot.Vector2 _swoopTarget;

    public override State Enter(State previousState)
    {
        _bat = Context as Character;
        _hasSwooped = false;

        // Get reference to Idle state
        _idle = GetParent().GetNode<State>("Idle");

        // Get player reference
        _player = GetTree().GetFirstNodeInGroup("Player") as Node2D;

        // Record player's current position as swoop target
        if (_player != null)
        {
            _swoopTarget = _player.GlobalPosition;
        }

        // Tell movement state machine to start swooping
        var movementSM = _bat?.GetNode<StateMachine>("MovementStateMachine");
        if (movementSM != null)
        {
            // Pass swoop target to Swooping state
            var swoopingState = movementSM.GetNode<BatSwooping>("Swooping");
            if (swoopingState != null)
            {
                swoopingState.SwoopTarget = _swoopTarget;
            }
            movementSM.ChangeState("Swooping");
        }

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        // Check if we've reached the swoop target
        if (_bat != null && !_hasSwooped && _bat.GlobalPosition.DistanceTo(_swoopTarget) < 10f)
        {
            _hasSwooped = true;

            // Start returning
            var movementSM = _bat.GetNode<StateMachine>("MovementStateMachine");
            movementSM?.ChangeState("Returning");
        }

        return base.Process(delta);
    }

    public override void Exit(State nextState)
    {
        _hasSwooped = false;

        base.Exit(nextState);
    }
}
