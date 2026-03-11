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
    private Character _player;

    private bool _attackFinished = false;

    public override State Enter(State previousState)
    {
        _siracus = Context as Character;

        _idle = GetParent().GetNode<State>("Idle");

        _player = GetTree().GetFirstNodeInGroup("Player") as Character;

        var attackStateMachine = _siracus.FindChild("Attacks") as StateMachine;

        if (attackStateMachine != null)
        {
            attackNum = attackStateMachine.GetChildCount();
            _attackIdle = attackStateMachine.GetChild(0) as AttackIdle;

            // Pick attack to use
            if (attackNum != 0)
            {
                // Idle is num 0 on the list of childs. Slash is num 1 and only for melee.

                // If player is too close
                var distancePlayer = _siracus.GlobalPosition.DistanceTo(_player.GlobalPosition);

                if (distancePlayer <= 10)
                {
                    _pickedAttack = attackStateMachine.GetChild(1) as State;

                    attackStateMachine.ChangeState(_pickedAttack);
                } else
                {
                    var rng = new RandomNumberGenerator();

                    var childPick = rng.RandiRange(2, attackNum - 1);

                    _pickedAttack = attackStateMachine.GetChild(childPick) as State;

                    attackStateMachine.ChangeState(_pickedAttack);
                }

                GD.Print("Attack picked: ", _pickedAttack.Name);

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
