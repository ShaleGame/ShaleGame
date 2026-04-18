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
    
    [Export] public float MoveSpeed {get; set;} = 400f;
    [Export] public float ArrivalThreshold {get; set;} = 5f;
    [Export] public Node2D TargetPosition {get; set;}

    [Signal] public delegate void ArrivedEventHandler();

    private Character _drillBit;

    public override State Enter(State previousState)
    {
        _drillBit = Context as Character;
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
        }

        return base.Process(delta);
    }

}
