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

    /// <summary>
    /// Called when the owner character requests to use the weapon (primary use,
    /// such as firing or swinging).
    /// </summary>
    public virtual State PrimaryUseStart()
    {
        return null;
    }

    /// <summary>
    /// Called when the owner character stops using the weapon (releasing
    /// the primary use input).
    /// </summary>
    public virtual State PrimaryUseEnd()
    {
        return null;
    }
}
