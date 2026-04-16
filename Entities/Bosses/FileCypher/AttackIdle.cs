using Godot;
using CrossedDimensions.States;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class AttackIdle : State
{
    [Signal]
    public delegate void AttackHasFinishedEventHandler();

    public override State Enter(State previousState)
    {
        EmitSignal(SignalName.AttackHasFinished);
        return base.Enter(previousState);
    }
}
