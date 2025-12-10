using Godot;

namespace CrossDimensions.States.Weapons;

/// <summary>
/// A state where a weapon is idle, waiting for user input.
/// </summary>
public partial class WeaponIdleState : WeaponState
{
    /// <summary>
    /// The state to transition to when the weapon is used using primary fire.
    /// </summary>
    [Export]
    public WeaponState PrimaryUseState { get; set; }

    /// <summary>
    /// The state to transition to when the weapon is used using secondary
    /// fire.
    /// </summary>
    [Export]
    public WeaponState SecondaryUseState { get; set; }

    public override State Process(double delta)
    {
        if (Weapon.IsPrimaryUseHeld && PrimaryUseState is not null)
        {
            return PrimaryUseState;
        }

        if (Weapon.IsSecondaryUseHeld && SecondaryUseState is not null)
        {
            return SecondaryUseState;
        }

        return base.Process(delta);
    }
}
