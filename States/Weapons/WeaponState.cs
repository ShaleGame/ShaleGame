using Godot;

namespace CrossDimensions.States.Weapons;

public abstract partial class WeaponState : State
{
    public override Node Context
    {
        get => base.Context;
        set
        {
            var weapon = value as Items.Weapon;
            base.Context = weapon;
            Weapon = weapon;
        }
    }

    /// <summary>
    /// The weapon this state belongs to.
    /// </summary>
    public Items.Weapon Weapon { get; private set; }
}
