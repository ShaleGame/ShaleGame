using System.Collections.Generic;
using Godot;

namespace CrossDimensions.States;

/// <summary>
/// A node-based state machine implementation.
/// </summary>
public abstract partial class StateMachine<TState> : Node where TState : IState<TState>
{
    public TState CurrentState { get; protected set; }

    public abstract TState InitialState { get; set; }

    public override void _Ready()
    {
        ChangeState(InitialState);
    }

    /// <summary>
    /// Changes the current state to the specified new state.
    /// </summary>
    public void ChangeState(TState newState)
    {
        var oldState = CurrentState;

        if (CurrentState != null)
        {
            CurrentState.Exit(newState);
        }

        // enter new state until it returns null (no further transitions)
        do
        {
            CurrentState = newState;
            newState = CurrentState.Enter(newState);
        } while (newState is not null);
    }

    /// <summary>
    /// Changes the current state to the first state of type T found
    /// in the state machine's children.
    /// </summary>
    public void ChangeState<T>() where T : TState
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
}
