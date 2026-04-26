using Godot;
using CrossedDimensions.States;

namespace CrossedDimensions.Entities.Bosses.Drill;

/// <summary>
/// Idle state for the drill. Waits till the timer reaches 0 and then goes to attacking state.
/// </summary>

public partial class Idle : State
{

    [Export] public float WaitTime { get; set; } = 3f;
    [Export] public State AttackingState { get; set; } = null;

    private double _curTime = 0.0;

    public override State Enter(State previousState)
    {
        _curTime = 0.0;
        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        _curTime += delta;

        if (_curTime >= WaitTime && AttackingState != null)
        {
            _curTime = 0.0;

            return AttackingState;
        }

        return base.Process(delta);
    }

}
