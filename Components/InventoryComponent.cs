using Godot;
using Godot.Collections;
using CrossedDimensions.Characters;
using CrossedDimensions.Items;

namespace CrossedDimensions.Components;

public partial class InventoryComponent : Node
{
    [Signal]
    public delegate void EquippedWeaponChangedEventHandler(Weapon previousWeapon, Weapon currentWeapon);

    [Export]
    public Array<ItemInstance> Items { get; set; } = new();

    public Character OwnerCharacter { get; private set; }

    public Weapon EquippedWeapon { get; private set; }

    public bool IsWeaponEquipped(Weapon weapon)
        => EquippedWeapon == weapon;

    public void Initialize(Character owner)
    {
        OwnerCharacter = owner;
    }

    public void EquipWeapon(Weapon weapon)
    {
        if (EquippedWeapon == weapon)
        {
            return;
        }

        var previousWeapon = EquippedWeapon;
        EquippedWeapon = weapon;

        if (OwnerCharacter is not null)
        {
            weapon.OwnerCharacter = OwnerCharacter;
        }

        EmitSignal(SignalName.EquippedWeaponChanged, previousWeapon, EquippedWeapon);
    }
}
