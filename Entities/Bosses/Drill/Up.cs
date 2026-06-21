using Godot;
using System;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.Drill;

/// <summary>
/// Gets called back up to the drill base, by doing a single large jump.
/// </summary>

public partial class Up : State
{

    [Export] public float MoveSpeed { get; set; } = 400f;
    [Export] public float ArrivalThreshold { get; set; } = 5f;
    [Export] public Node2D TargetPosition { get; set; }
    [Export] public State Still;

    [Signal] public delegate void ArrivedEventHandler();

    private Character _drillBit;

    public override State Enter(State previousState)
    {
        _drillBit = Context as Character;

        if (_drillBit != null)
        {
            var collision = _drillBit.GetChild(0) as CollisionShape2D;
            collision.Disabled = true;
        }

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (_drillBit == null || TargetPosition == null)
        {
            return base.Process(delta);
        }

        Vector2 direction = (TargetPosition.GlobalPosition - _drillBit.GlobalPosition).Normalized();
        _drillBit.Velocity = direction * MoveSpeed;
        _drillBit.MoveAndSlide();

        if (_drillBit.GlobalPosition.DistanceTo(TargetPosition.GlobalPosition) <= ArrivalThreshold)
        {
            _drillBit.Velocity = Vector2.Zero;
            EmitSignal(SignalName.Arrived);

            var stateMachine = GetParent<StateMachine>();
            stateMachine.ChangeState(Still);
        }

        return base.Process(delta);
    }

    public override void Exit(State nextState)
    {
        if (_drillBit != null)
        {
            var collision = _drillBit.GetChild(0) as CollisionShape2D;
            collision.Disabled = false;
        }

        base.Exit(nextState);
    }

}
