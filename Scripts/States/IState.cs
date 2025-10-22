using System.Collections.Generic;

namespace CrossDimensions.States;

public interface IState<T> where T : IState<T>
{
    /// <summary>
    /// Called when a state machine is entering this state
    /// </summary>
    /// <remarks>
    /// This returns an <c>IState</c> in case a state is being
    /// transitioned to but wants to transition to another state. For
    /// example, an attack state can return to an idle state, but that idle
    /// state can override it to the move state immediately when a movement
    /// input is detected.
    /// </remarks>
    public T Enter(T previousState);

    /// <summary>
    /// Called when a state machine is exiting this state
    /// </summary>
    public void Exit(T nextState);
}
