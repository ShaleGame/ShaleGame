using CrossedDimensions.Extensions;
using Godot;

namespace CrossedDimensions.States;

[GlobalClass]
public partial class State : Node
{
    [Signal]
    public delegate void EnteredEventHandler(State previousState);

    [Signal]
    public delegate void ExitedEventHandler(State nextState);

    public virtual Node Context { get; set; }

    /// <summary>
    /// Called when entering this state. Can return another state to
    /// immediately transition to that state.
    /// </summary>
    public virtual State Enter(State previousState)
    {
        EmitSignal(SignalName.Entered, previousState);
        return EnterChildren(previousState);
    }

    internal State EnterChildren(State previousState)
    {
        State next = null;
        foreach (var child in this.EnumerateChildren())
        {
            if (child is State state)
            {
                state.Context = Context;
                next = state.Enter(previousState);
            }
            else
            {
                child.Set("context", Context);
                next = child.CallScriptMethod<State>("enter", previousState);
            }

            if (next is not null)
            {
                return next;
            }
        }
        return null;
    }

    /// <summary>
    /// Called when exiting this state. Use this to clean up any state
    /// specific data if necessary.
    /// </summary>
    public virtual void Exit(State nextState)
    {
        EmitSignal(SignalName.Exited, nextState);
        ExitChildren(nextState);
    }

    internal void ExitChildren(State nextState)
    {
        foreach (var child in this.EnumerateChildren())
        {
            if (child is State state)
            {
                state.Exit(nextState);
            }
            else
            {
                child.CallScriptMethod<State>("exit", nextState);
            }
        }
    }

    /// <summary>
    /// Called every frame while this state is active. Override to implement
    /// state-specific logic.
    /// </summary>
    public virtual State Process(double delta)
    {
        return ProcessChildren(delta);
    }

    internal State ProcessChildren(double delta)
    {
        State next = null;
        foreach (var child in this.EnumerateChildren())
        {
            if (child is State state)
            {
                next = state.Process(delta);
            }
            else
            {
                next = child.CallScriptMethod<State>("process", delta);
            }

            if (next is not null)
            {
                return next;
            }
        }
        return null;
    }

    /// <summary>
    /// Called every physics frame while this state is active. Override to
    /// implement state-specific physics logic.
    /// </summary>
    public virtual State PhysicsProcess(double delta)
    {
        return PhysicsProcessChildren(delta);
    }

    internal State PhysicsProcessChildren(double delta)
    {
        State next = null;
        foreach (var child in this.EnumerateChildren())
        {
            if (child is State state)
            {
                next = state.PhysicsProcess(delta);
            }
            else
            {
                next = child.CallScriptMethod<State>("physics_process", delta);
            }

            if (next is not null)
            {
                return next;
            }
        }
        return null;
    }

    /// <summary>
    /// Called when an input event is received while this state is active.
    /// Override to implement state-specific input handling.
    /// </summary>
    public virtual State Input(InputEvent @event)
    {
        return InputChildren(@event);
    }

    internal State InputChildren(InputEvent @event)
    {
        State next = null;
        foreach (var child in this.EnumerateChildren())
        {
            if (child is State state)
            {
                next = state.Input(@event);
            }
            else
            {
                next = child.CallScriptMethod<State>("input", @event);
            }

            if (next is not null)
            {
                return next;
            }
        }
        return null;
    }
}
