using System.Linq;
using Godot;
using CrossedDimensions.Characters;
using CrossedDimensions.Items;
using CrossedDimensions.Extensions;
using CrossedDimensions.Saves;

namespace CrossedDimensions.Components;

/// <summary>
/// Component that provides a simple inventory for characters. It discovers
/// contained <see cref="ItemInstance"/> children and manages weapon
/// activation/selection for contained <see cref="Weapon"/> instances.
/// </summary>
[GlobalClass]
public partial class InventoryComponent : Node2D
{
    private bool _suppressSavePersistence;

    /// <summary>
    /// Emitted when the equipped weapon changes. Provides the previous and the
    /// currently equipped weapon (either may be null).
    /// </summary>
    [Signal]
    public delegate void EquippedWeaponChangedEventHandler(
        Weapon previousWeapon, int previousIndex, Weapon currentWeapon, int currentIndex);

    /// <summary>
    /// Emitted when a weapon is added to the inventory.
    /// </summary>
    [Signal]
    public delegate void WeaponAddedEventHandler(Weapon weapon, int index);

    /// <summary>
    /// Emitted when a weapon is removed from the inventory.
    /// </summary>
    [Signal]
    public delegate void WeaponRemovedEventHandler(Weapon weapon, int index);

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
        ChildExitingTree += OnChildExitingTree;

        bool isOriginalCharacter = OwnerCharacter?.Cloneable?.IsClone == false;
        if (isOriginalCharacter)
        {
            _suppressSavePersistence = true;
        }

        try
        {
            // on ready, auto-equip the first weapon if we have one and don't already
            if (EquippedWeapon is null)
            {
                var firstWeapon = GetChildren().OfType<Weapon>().FirstOrDefault();
                if (firstWeapon is not null)
                {
                    EquipWeapon(firstWeapon, recursive: false);
                }
            }

            if (isOriginalCharacter)
            {
                LoadFromSave(SaveManager.Instance?.CurrentSave);
            }
        }
        finally
        {
            if (isOriginalCharacter)
            {
                _suppressSavePersistence = false;
            }
        }
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

            // auto-equip first weapon on pickup. NOTE: this does not handle
            // the case where the player character starts with the weapon
            // on ready.
            if (EquippedWeapon is null && weapon.GetParent() == this)
            {
                EquipWeapon(weapon);
            }

            if (OwnerCharacter?.Cloneable?.Mirror is not null)
            {
                var clone = OwnerCharacter.Cloneable.Mirror;
                if (!clone.Inventory.HasNode(new NodePath(weapon.Name)))
                {
                    var weaponClone = weapon.Duplicate() as Weapon;
                    GD.Print($"Adding weapon clone {weaponClone.Name} to clone inventory");
                    clone.Inventory.AddChild(weaponClone);
                }
            }

            if (!_suppressSavePersistence &&
                OwnerCharacter.Cloneable?.IsClone == false)
            {
                PersistToSave(SaveManager.Instance?.CurrentSave);
            }

            var weapons = GetChildren().OfType<Weapon>().ToList();
            int index = weapons.IndexOf(weapon);
            EmitSignal(SignalName.WeaponAdded, weapon, index);
        }
    }

    private void OnChildExitingTree(Node child)
    {
        if (child is Weapon weapon)
        {
            var weapons = GetChildren().OfType<Weapon>().ToList();
            int index = weapons.IndexOf(weapon);
            if (index == -1)
            {
                weapons.Add(weapon);
                index = weapons.Count - 1;
            }
            EmitSignal(SignalName.WeaponRemoved, weapon, index);
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

        EquipWeapon(weapons[nextIndex], false);
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
            clone.Inventory.EquipWeaponByName(name, recursive: false);
        }
    }

    /// <summary>
    /// Equip the specified weapon. This will deactivate tously
    /// equipped weapon (if any), activate the new weapon, update the weapon's
    /// <see cref="ItemInstance.OwnerCharacter"/>, and emit the
    /// <see cref="EquippedWeaponChangedEventHandler"/> signal.
    /// </summary>
    public void EquipWeapon(Weapon weapon, bool recursive = true)
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

        var clone = OwnerCharacter?.Cloneable?.Mirror;
        if (clone is not null && recursive)
        {
            var path = new NodePath(weapon.Name);
            var cloneWeapon = clone.Inventory.GetNode<Weapon>(path);
            if (cloneWeapon is not null)
            {
                if (clone.Inventory.EquippedWeapon != cloneWeapon)
                {
                    clone.Inventory.EquipWeapon(cloneWeapon);
                }
            }
        }

        EmitSignal(
            SignalName.EquippedWeaponChanged,
            previousWeapon,
            oldIndex,
            EquippedWeapon,
            newIndex);

        if (!_suppressSavePersistence &&
            OwnerCharacter is not null &&
            OwnerCharacter.Cloneable?.Original is null)
        {
            PersistToSave(SaveManager.Instance?.CurrentSave);
        }
    }

    /// <summary>
    /// Equip a weapon by its node name. If no weapon with the specified name
    /// exists, this method does nothing.
    /// </summary>
    /// <param name="name">The name of the weapon node to equip.</param>
    public void EquipWeaponByName(string name, bool recursive = true)
    {
        if (this.HasNode<Weapon>(name, out var weapon))
        {
            EquipWeapon(weapon, recursive);
        }
    }

    /// <summary>
    /// Equips the weapon located at the provided zero-based slot index. Non-weapon
    /// children are skipped so the index always maps to a weapon child in order.
    /// </summary>
    /// <param name="index">Zero-based slot index identifying the weapon.</param>
    public void EquipWeaponByIndex(int index, bool recursive = true)
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
            EquipWeapon(weapon, recursive);
        }
    }

    public Godot.Collections.Array<string> GetWeaponScenePaths()
    {
        var paths = new Godot.Collections.Array<string>();

        foreach (var weapon in GetChildren().OfType<Weapon>())
        {
            var path = GetWeaponScenePath(weapon);
            if (!string.IsNullOrEmpty(path))
            {
                paths.Add(path);
            }
        }

        return paths;
    }

    public bool HasWeaponFromScene(PackedScene scene)
    {
        if (scene is null)
        {
            return false;
        }

        return HasWeaponFromScenePath(scene.ResourcePath);
    }

    internal void LoadFromSave(SaveFile save)
    {
        if (save is null || !save.KeyValue.ContainsKey("inventory_weapons"))
        {
            return;
        }

        bool wasSuppressed = _suppressSavePersistence;
        _suppressSavePersistence = true;
        try
        {
            foreach (var path in save.InventoryWeapons)
            {
                if (string.IsNullOrEmpty(path) || HasWeaponFromScenePath(path))
                {
                    continue;
                }

                var packedScene = ResourceLoader.Load<PackedScene>(path);
                if (packedScene is null)
                {
                    GD.PushWarning($"InventoryComponent: failed to load '{path}'.");
                    continue;
                }

                var weapon = packedScene.Instantiate<Weapon>();
                if (weapon is null)
                {
                    GD.PushWarning($"InventoryComponent: scene '{path}' is not a Weapon.");
                    continue;
                }

                AddChild(weapon);
            }

            EquipWeaponByIndex(save.InventoryEquippedIndex);
        }
        finally
        {
            _suppressSavePersistence = wasSuppressed;
        }
    }

    internal void PersistToSave(SaveFile save)
    {
        if (save is null)
        {
            return;
        }

        save.InventoryWeapons = GetWeaponScenePaths();

        var weapons = GetChildren().OfType<Weapon>().ToList();
        int equippedIndex = EquippedWeapon is null
            ? -1
            : weapons.IndexOf(EquippedWeapon);

        save.InventoryEquippedIndex = equippedIndex;
    }

    private bool HasWeaponFromScenePath(string scenePath)
    {
        return GetChildren()
            .OfType<Weapon>()
            .Any(weapon => GetWeaponScenePath(weapon) == scenePath);
    }

    private static string GetWeaponScenePath(Weapon weapon)
    {
        if (weapon is null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrEmpty(weapon.SceneFilePath))
        {
            return weapon.SceneFilePath;
        }

        if (weapon.HasMeta("scene_file_path"))
        {
            return weapon.GetMeta("scene_file_path").AsString();
        }

        return string.Empty;
    }
}
