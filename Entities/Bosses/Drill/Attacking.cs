using Godot;
using Godot.Collections;
using CrossedDimensions.States;

namespace CrossedDimensions.Entities.Bosses.Drill;

/// <summary>
/// Picks a random attack to perform and waits for it to be finished. Then transitions to the idle state.
/// </summary>

public partial class Attacking : State
{

    [Export] public StateMachine AttackStateMachine { get; set; }

    [Export] public AttackIdle AttackIdle { get; set; }

    [Export] public State IdleState { get; set; }

    [Export] public Array<State> AttackStates { get; set; } = new Array<State>();

    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override State Enter(State previousState)
    {
        if (AttackIdle != null)
        {
            AttackIdle.AttackFinished += OnAttackFinished;
        }

        if (AttackStates.Count > 0)
        {
            State chosen = AttackStates[_rng.RandiRange(0, AttackStates.Count - 1)];
            chosen = AttackStates[1]; // Always pick drilling attack
            AttackStateMachine?.ChangeState(chosen);
        }

        return base.Enter(previousState);
    }

    public override void Exit(State nextState)
    {
        if (AttackIdle != null)
        {
            AttackIdle.AttackFinished -= OnAttackFinished;
        }

        base.Exit(nextState);
    }

    private void OnAttackFinished()
    {

        var parentMachine = GetParent() as StateMachine;

        parentMachine?.ChangeState(IdleState);

    }

}
