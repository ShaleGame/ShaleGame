using Godot;
using System;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.Siracus;

// Picks an attack to perform

public partial class Attacking : State
{
    
    private int attackNum = 0;

    private Character _siracus;

    private State _pickedAttack = null;
    private AttackIdle _attackIdle;
    private State _idle;

    private bool _attackFinished = false;

    public override State Enter(State previousState)
    {
        _siracus = Context as Character;

        _idle = GetParent().GetNode<State>("Idle");

        var attackStateMachine = _siracus.FindChild("Attacks") as StateMachine;

        if (attackStateMachine != null)
        {
            attackNum = attackStateMachine.GetChildCount();
            _attackIdle = attackStateMachine.GetChild(0) as AttackIdle;

            // Pick attack to use
            if (attackNum != 0)
            {
                // Idle is num 0 on the list of childs

                var rng = new RandomNumberGenerator();

                var childPick = rng.RandiRange(1, attackNum);

                _pickedAttack = attackStateMachine.GetChild(childPick) as State;

                attackStateMachine.ChangeState(_pickedAttack);

                // Assign signal to exit function

                _attackIdle.AttackHasFinished += AttackFinished;
            }
        }

        _attackFinished = false;

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (_attackFinished)
        {
            // Return to idle when attack finished
            _attackIdle.AttackHasFinished -= AttackFinished;
            return _idle;
        }

        return base.Process(delta);
    }

    public void AttackFinished()
    {
        _attackFinished = true;
    }

}
