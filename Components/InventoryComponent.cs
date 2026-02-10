using System.Linq;
using Godot;
using Godot.Collections;
using CrossedDimensions.Characters;
using CrossedDimensions.Items;
using CrossedDimensions.Extensions;

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
        Weapon previousWeapon, int previousIndex, Weapon currentWeapon, int currentIndex);

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
            controller.WeaponSlotRequested += OnWeaponSlotRequested;
        }

        if (OwnerCharacter?.Cloneable is not null)
        {
            OwnerCharacter.Cloneable.CharacterSplitPost += PostCharacterSplit;
        }

        ChildEnteredTree += OnChildEnteredTree;
    }

    private void OnChildEnteredTree(Node child)
    {
        if (child is Weapon weapon)
        {
            weapon.OwnerCharacter = OwnerCharacter;
            weapon.IsActive = false;

            // if we have a clone, and the clone does not have the
            // corresponding weapon, then add the weapon to the clone as well
            // so that it can be equipped after the split

            EquipWeapon(weapon);

            if (OwnerCharacter.Cloneable?.Mirror is not null)
            {
                var clone = OwnerCharacter.Cloneable.Mirror;
                if (!clone.Inventory.HasNode(new NodePath(weapon.Name)))
                {
                    var weaponClone = weapon.Duplicate() as Weapon;
                    clone.Inventory.AddChild(weaponClone);
                }
            }
        }
    }

    public void CycleWeapon(int direction)
    {
        // gather all weapons in a list
        var weapons = GetChildren().OfType<Weapon>().ToList();

        if (!weapons.Any())
        {
            return;
        }

        int currentIndex = EquippedWeapon is null
            ? -1
            : weapons.IndexOf(EquippedWeapon);

        if (currentIndex == -1)
        {
            // if the currently equipped weapon isn't in the list, just equip
            // the first one
            EquipWeapon(weapons[0]);
            return;
        }

        int nextIndex = (currentIndex + direction) % weapons.Count;
        if (nextIndex < 0)
        {
            nextIndex += weapons.Count;
        }

        EquipWeapon(weapons[nextIndex]);
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
    /// Handles controller slot events by delegating to index-based equip logic.
    /// </summary>
    /// <param name="index">Slot index provided by the controller signal.</param>
    private void OnWeaponSlotRequested(int index)
    {
        EquipWeaponByIndex(index);
    }

    private void PostCharacterSplit(Character original, Character clone)
    {
        // when the character splits, we want to make sure the clone equips
        // an identical weapon to the original character
        if (clone.Inventory is not null && EquippedWeapon is not null)
        {
            string name = EquippedWeapon.Name;
            clone.Inventory.EquipWeaponByName(name);
        }
    }

    /// <summary>
    /// Equip the specified weapon. This will deactivate the previously
    /// equipped weapon (if any), activate the new weapon, update the weapon's
    /// <see cref="ItemInstance.OwnerCharacter"/>, and emit the
    /// <see cref="EquippedWeaponChangedEventHandler"/> signal.
    /// </summary>
    public void EquipWeapon(Weapon weapon)
    {
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

        var weapons = GetChildren().OfType<Weapon>().ToList();

        int oldIndex = previousWeapon is null
            ? -1
            : weapons.IndexOf(previousWeapon);

        int newIndex = weapons.IndexOf(weapon);

        EmitSignal(
            SignalName.EquippedWeaponChanged,
            previousWeapon,
            oldIndex,
            EquippedWeapon,
            newIndex);
    }

    /// <summary>
    /// Equip a weapon by its node name. If no weapon with the specified name
    /// exists, this method does nothing.
    /// </summary>
    /// <param name="name">The name of the weapon node to equip.</param>
    public void EquipWeaponByName(string name)
    {
        if (this.HasNode<Weapon>(name, out var weapon))
        {
            GD.Print($"Equipping weapon: {weapon.GetPath()}");
            EquipWeapon(weapon);
        }
    }

    /// <summary>
    /// Equips the weapon located at the provided zero-based slot index. Non-weapon
    /// children are skipped so the index always maps to a weapon child in order.
    /// </summary>
    /// <param name="index">Zero-based slot index identifying the weapon.</param>
    public void EquipWeaponByIndex(int index)
    {
        if (index < 0)
        {
            return;
        }

        var weapon = GetChildren()
            .OfType<Weapon>()
            .Skip(index)
            .FirstOrDefault();

        if (weapon is not null)
        {
            EquipWeapon(weapon);
        }
    }
}
