using Godot;

namespace CrossDimensions.States.Weapons;

public partial class WeaponStateMachine : StateMachine
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
    /// The weapon this state machine belongs to.
    /// </summary>
    public Items.Weapon Weapon { get; private set; }
}
