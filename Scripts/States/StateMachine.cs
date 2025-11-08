using Godot;

namespace CrossDimensions.States;

[GlobalClass]
public partial class StateMachine : Node
{
    /// <summary>
    /// The node that this state machine is managing.
    /// </summary>
    public Node Context { get; set; }

    public State CurrentState { get; protected set; }

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

    public void ChangeState(State newState)
    {
        if (newState is null)
        {
            return;
        }

        CurrentState?.Exit(newState);

        // recursively enter the new state
        var previousState = CurrentState;
        CurrentState = newState;
        CurrentState.Context = Context;
        ChangeState(CurrentState.Enter(previousState));
    }

    public void Process(double delta)
    {
        ChangeState(CurrentState?.Process(delta));
    }

    public void PhysicsProcess(double delta)
    {
        ChangeState(CurrentState?.PhysicsProcess(delta));
    }

    public void Input(InputEvent @event)
    {
        ChangeState(CurrentState?.Input(@event));
    }

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

    public void ChangeState(string nodeName)
    {
        ChangeState(GetNode<State>(nodeName));
    }
}
