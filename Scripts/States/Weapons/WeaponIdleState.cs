using Godot;

namespace CrossDimensions.States.Weapons;

public partial class WeaponIdleState : WeaponState
{
    /// <summary>
    /// The state to transition to when the weapon is used.
    /// </summary>
    [Export]
    public WeaponState UseState { get; set; }

    public override State PrimaryUseStart()
    {
        return UseState;
    }
}
