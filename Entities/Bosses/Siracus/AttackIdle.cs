using Godot;
using System;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.Siracus;

public partial class AttackIdle : State
{

    // Does nothing but acts as a way to signal it is not attacking

    [Signal]
    public delegate void AttackHasFinishedEventHandler();

    public override State Enter(State previousState)
    {
        EmitSignal(SignalName.AttackHasFinished);

        return base.Enter(previousState);
    }

}
