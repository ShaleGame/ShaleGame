using CrossedDimensions.Characters;
using Godot;
using System;

namespace CrossedDimensions.States.Enemies;

[GlobalClass]
public partial class BatIdle : State
{
    [Export] public float DetectionRadius = 200f;

    private Character _bat;
    private Node2D _player;
    private State _attacking;

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
                return _attacking;
            }
        }

        return base.Process(delta);
    }
}
