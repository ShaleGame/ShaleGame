using Godot;

namespace CrossedDimensions.States.Weapons;

/// <summary>
/// A state where a weapon must wait in before proceeding to the next state.
/// This can be used to compose a reload state.
/// </summary>
public partial class WeaponWaitState : WeaponState
{
    /// <summary>
    /// The state to transition to when the wait is over.
    /// </summary>
    [Export]
    public WeaponState NextState { get; set; }

    /// <summary>
    /// The timer used to track the wait duration.
    /// </summary>
    [Export]
    public Timer Timer { get; set; }

    public override State Enter(State previousState)
    {
        Timer.Start();
        return base.Enter(previousState);
    }

    public override void Exit(State nextState)
    {
        Timer.Stop();
        base.Exit(nextState);
    }

    public override State Process(double delta)
    {
        if (Timer.TimeLeft <= 0)
        {
            return NextState;
        }

        return null;
    }
}
