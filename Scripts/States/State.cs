using Godot;

namespace CrossDimensions.States;

public partial class State : Node
{
    public virtual Node Context { get; set; }

    /// <summary>
    /// Called when entering this state. Can return another state to
    /// immediately transition to that state.
    /// </summary>
    public virtual State Enter(State previousState)
    {
        return null;
    }

    /// <summary>
    /// Called when exiting this state. Use this to clean up any state
    /// specific data if necessary.
    /// </summary>
    public virtual void Exit(State nextState)
    {

    }

    /// <summary>
    /// Called every frame while this state is active. Override to implement
    /// state-specific logic.
    /// </summary>
    public virtual State Process(double delta)
    {
        return null;
    }

    /// <summary>
    /// Called every physics frame while this state is active. Override to
    /// implement state-specific physics logic.
    /// </summary>
    public virtual State PhysicsProcess(double delta)
    {
        return null;
    }

    /// <summary>
    /// Called when an input event is received while this state is active.
    /// Override to implement state-specific input handling.
    /// </summary>
    public virtual State Input(InputEvent @event)
    {
        return null;
    }
}
