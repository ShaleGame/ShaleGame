using CrossedDimensions.Components;
using Godot;

namespace CrossedDimensions.States.Weapons.Behaviors;

/// <summary>
/// A weapon behavior that unfreezes all components tracked by a
/// <see cref="FreezeTrackerComponent"/> when entered.
/// </summary>
public partial class ReleaseFreezeBehavior : WeaponState
{
    [Export]
    public FreezeTrackerComponent FreezeTracker { get; set; }

    public override State Enter(State previousState)
    {
        FreezeTracker?.UnfreezeAll();
        return base.Enter(previousState);
    }
}
