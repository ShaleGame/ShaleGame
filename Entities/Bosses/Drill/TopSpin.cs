using Godot;
using CrossedDimensions.States;

namespace CrossedDimensions.Entities.Bosses.Drill;

/// <summary>
/// Drops the drill bit to the floor, and makes it spin from side to side for the player to jump over and dodge. Once done, the drill top spins back up to the drill base.
/// </summary>

public partial class TopSpin : State
{

    [Export] public StateMachine DrillBitStateMachine { get; set; }
    [Export] public State DrillBitDropState { get; set; }
    [Export] public Up Up { get; set; }
    [Export] public StateMachine AttackStateMachine { get; set; }
    [Export] public State AttackIdleState { get; set; }
    [Export] public float Duration { get; set; } = 15f;

    private double _curTime = 0.0;
    private bool _calledBack = false;

    public override State Enter(State previousState)
    {
        _curTime = 0.0;
        _calledBack = false;

        if (Up != null)
        {
            Up.Arrived -= OnDrillBitArrived; // prevent double-subscription
            Up.Arrived += OnDrillBitArrived;
        }

        DrillBitStateMachine?.ChangeState(DrillBitDropState);

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (!_calledBack)
        {
            _curTime += delta;
            if (_curTime >= Duration)
            {
                _calledBack = true;
                DrillBitStateMachine?.ChangeState(Up);
            }
        }

        return base.Process(delta);
    }

    public override void Exit(State nextState)
    {
        if (Up != null)
        {
            Up.Arrived -= OnDrillBitArrived;
        }

        base.Exit(nextState);
    }

    private void OnDrillBitArrived()
    {
        AttackStateMachine?.ChangeState(AttackIdleState);
    }

}
