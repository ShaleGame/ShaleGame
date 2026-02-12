using Godot;

namespace CrossedDimensions.States;

/// <summary>
/// A state machine that manages states for a given context node. The node
/// should delegate its state-specific process, physics process, and input
/// events to the state machine, which will in turn delegate them to the
/// current state.
/// </summary>
[GlobalClass]
public partial class StateMachine : Node
{
    [Signal]
    public delegate void StateChangedEventHandler(State newState);

    [Signal]
    public delegate void StateEnteredEventHandler(State previousState, State newState);

    /// <summary>
    /// The node that this state machine is managing.
    /// </summary>
    public virtual Node Context { get; set; }

    /// <summary>
    /// The current active state of the state machine.
    /// </summary>
    public State CurrentState { get; protected set; }

    /// <summary>
    /// The initial state of the state machine.
    /// </summary>
    [Export]
    public State InitialState { get; set; }

    /// <summary>
    /// This can be overriden so state machines extending this class can have
    /// more specific context types. For example, character state machines can
    /// have Character context types.
    /// </summary>
    public virtual void Initialize(Node context)
    {
        Context = context;
        CurrentState = InitialState;
        ChangeState(CurrentState);
    }

    /// <summary>
    /// Forces a state change to the given new state. If the new state is null,
    /// no state change occurs. A State Enter call can return another state to
    /// enter, so this method will recursively enter states until a null state
    /// is returned, indicating no further state changes.
    /// </summary>
    public void ChangeState(State newState)
    {
        if (newState is null)
        {
            return;
        }

        var previousState = CurrentState;

        CurrentState?.Exit(newState);

        // set and emit change
        CurrentState = newState;
        CurrentState.Context = Context;

        EmitSignal(SignalName.StateEntered, previousState, CurrentState);

        // recursively enter the new state
        var next = CurrentState.Enter(previousState);

        if (next is null)
        {
            EmitSignal(SignalName.StateChanged, CurrentState);
        }
        else
        {
            ChangeState(next);
        }
    }

    /// <summary>
    /// Delegates the process call to the current state and handles any state
    /// changes that may occur.
    /// </summary>
    public void Process(double delta)
    {
        ChangeState(CurrentState?.Process(delta));
    }

    /// <summary>
    /// Delegates the physics process call to the current state and handles any
    /// state changes that may occur.
    /// </summary>
    public void PhysicsProcess(double delta)
    {
        ChangeState(CurrentState?.PhysicsProcess(delta));
    }

    /// <summary>
    /// Delegates the input event to the current state and handles any state
    /// changes that may occur.
    /// </summary>
    public void Input(InputEvent @event)
    {
        ChangeState(CurrentState?.Input(@event));
    }

    /// <summary>
    /// Changes the current state to the first child state of the given type.
    /// </summary>
    /// <typeparam name="T">The type of state to change to.</typeparam>
    public void ChangeState<T>() where T : State
    {
        foreach (var child in GetChildren())
        {
            if (child is T state)
            {
                ChangeState(state);
                return;
            }
        }
    }

    /// <summary>
    /// Changes the current state to the child state with the given node name.
    /// </summary>
    public void ChangeState(string nodeName)
    {
        ChangeState(GetNode<State>(nodeName));
    }
}
