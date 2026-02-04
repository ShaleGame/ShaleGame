using System.Collections.Generic;
using Godot;
using Godot.Collections;
using CrossedDimensions.Characters;
using CrossedDimensions.Items;

namespace CrossedDimensions.Components;

/// <summary>
/// Component that provides a simple inventory for characters. It discovers
/// contained <see cref="ItemInstance"/> children and manages weapon
/// activation/selection for contained <see cref="Weapon"/> instances.
/// </summary>
[GlobalClass]
public partial class InventoryComponent : Node2D
{
    /// <summary>
    /// Emitted when the equipped weapon changes. Provides the previous and the
    /// currently equipped weapon (either may be null).
    /// </summary>
    [Signal]
    public delegate void EquippedWeaponChangedEventHandler(
        Weapon previousWeapon, Weapon currentWeapon);

    /// <summary>
    /// The list of items owned by this inventory. Items are discovered from the
    /// node's children during <see cref="_Ready"/> and added to this list.
    /// </summary>
    [Export]
    public Array<ItemInstance> Items { get; set; } = new();

    /// <summary>
    /// The character that owns this inventory. This will be assigned in the
    /// editor or can be set programmatically. Discovered weapons will have
    /// their <see cref="Items.ItemInstance.OwnerCharacter"/> set to this value.
    /// </summary>
    [Export]
    public Character OwnerCharacter { get; set; }

    /// <summary>
    /// The currently equipped weapon, or null if no weapon is equipped.
    /// </summary>
    public Weapon EquippedWeapon { get; private set; }

    /// <summary>
    /// Checks if <paramref name="weapon" /> is currently equipped.
    /// </summary>
    /// <returns>
    /// <c>true</c> when the provided weapon is the one currently equipped by
    /// this inventory.
    /// </returns>
    public bool IsWeaponEquipped(Weapon weapon) => EquippedWeapon == weapon;

    /// <summary>
    /// Scans the node's children for <see cref="ItemInstance"/> objects, adds
    /// them to <see cref="Items"/>, and initializes any <see cref="Weapon"/>
    /// children by setting their owner and marking them inactive by default.
    /// </summary>
    public override void _Ready()
    {
        foreach (var child in GetChildren())
        {
            if (child is ItemInstance item)
            {
                Items.Add(item);

                if (item is Weapon weapon)
                {
                    weapon.OwnerCharacter = OwnerCharacter;
                    weapon.IsActive = false;
                }
            }
        }

        var controller = OwnerCharacter?.Controller;
        if (controller is not null)
        {
            controller.WeaponNextRequested += OnWeaponNextRequested;
            controller.WeaponPreviousRequested += OnWeaponPreviousRequested;
        }
    }

    public void CycleWeapon(int direction)
    {
        if (direction == 0 || Items.Count == 0)
        {
            return;
        }

        int startIndex = Items.IndexOf(EquippedWeapon);
        if (startIndex == -1)
        {
            startIndex = direction > 0 ? -1 : 0;
        }

        int count = Items.Count;
        for (int offset = 1; offset <= count; offset++)
        {
            int candidateIndex = (startIndex + direction * offset) % count;
            if (candidateIndex < 0)
            {
                candidateIndex += count;
            }

            if (Items[candidateIndex] is Weapon weapon)
            {
                EquipWeapon(weapon);
                return;
            }
        }
    }

    private void OnWeaponNextRequested()
    {
        CycleWeapon(1);
    }

    private void OnWeaponPreviousRequested()
    {
        CycleWeapon(-1);
    }

    /// <summary>
    /// Equip the specified weapon. This will deactivate the previously
    /// equipped weapon (if any), activate the new weapon, update the weapon's
    /// <see cref="ItemInstance.OwnerCharacter"/>, and emit the
    /// <see cref="EquippedWeaponChangedEventHandler"/> signal.
    /// </summary>
    public void EquipWeapon(Weapon weapon)
    {
        if (EquippedWeapon == weapon)
        {
            return;
        }

        var previousWeapon = EquippedWeapon;
        EquippedWeapon = weapon;

        if (previousWeapon is not null)
        {
            previousWeapon.IsActive = false;
        }

        weapon.IsActive = true;

        if (OwnerCharacter is not null)
        {
            weapon.OwnerCharacter = OwnerCharacter;
        }

        EmitSignal(SignalName.EquippedWeaponChanged, previousWeapon, EquippedWeapon);
    }
}
