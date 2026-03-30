using System.Linq;
using CrossedDimensions.Components;
using CrossedDimensions.Items;
using CrossedDimensions.Saves;
using Godot;

namespace CrossedDimensions.Tests.Components;

[Collection("GodotHeadless")]
public class InventoryComponentTest(GodotHeadlessFixedFpsFixture godot)
{
    private const string RocketLauncherPath = "res://Items/RocketLauncher.tscn";
    private const string PelletShooterPath = "res://Items/PelletShooter.tscn";

    [Fact]
    public void Ready_ShouldEquipAnyWeapon()
    {
        var inventory = new InventoryComponent();
        var weapon = new Weapon();
        weapon.IsActive = false;
        inventory.AddChild(weapon);

        godot.Tree.Root.AddChild(inventory);

        weapon.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void EquipWeapon_ShouldActivateWeapon_DeactivatePrevious_AndEmitSignal()
    {
        var inventory = new InventoryComponent();
        var weaponA = new Weapon();
        var weaponB = new Weapon();
        inventory.AddChild(weaponA);
        inventory.AddChild(weaponB);

        godot.Tree.Root.AddChild(inventory);

        bool signalEmitted = false;
        inventory.EquippedWeaponChanged += (prev, prevIndex, curr, currIndex) =>
        {
            signalEmitted = true;
        };

        inventory.EquipWeapon(weaponA);

        weaponA.IsActive.ShouldBeTrue();
        inventory.IsWeaponEquipped(weaponA).ShouldBeTrue();

        inventory.EquipWeapon(weaponB);

        weaponA.IsActive.ShouldBeFalse();
        weaponB.IsActive.ShouldBeTrue();
        inventory.IsWeaponEquipped(weaponB).ShouldBeTrue();

        signalEmitted.ShouldBeTrue();
    }

    [Fact]
    public void CycleWeapon_ShouldWrapForwardAndAround()
    {
        var inventory = new InventoryComponent();
        var left = new Weapon();
        var middle = new Weapon();
        var right = new Weapon();
        inventory.AddChild(left);
        inventory.AddChild(middle);
        inventory.AddChild(right);

        godot.Tree.Root.AddChild(inventory);

        inventory.EquipWeapon(right);

        inventory.CycleWeapon(1);
        inventory.EquippedWeapon.ShouldBe(left);
    }

    [Fact]
    public void CycleWeapon_ShouldWrapBackward()
    {
        var inventory = new InventoryComponent();
        var left = new Weapon();
        var middle = new Weapon();
        var right = new Weapon();
        inventory.AddChild(left);
        inventory.AddChild(middle);
        inventory.AddChild(right);

        godot.Tree.Root.AddChild(inventory);
        inventory.EquipWeapon(middle);

        inventory.CycleWeapon(-1);
        inventory.EquippedWeapon.ShouldBe(left);
    }

    [Fact]
    public void CycleWeapon_SkipsNonWeapons()
    {
        var inventory = new InventoryComponent();
        inventory.AddChild(new ItemInstance());
        var weapon = new Weapon();
        inventory.AddChild(weapon);

        godot.Tree.Root.AddChild(inventory);

        inventory.CycleWeapon(1);
        inventory.EquippedWeapon.ShouldBe(weapon);
    }

    [Fact]
    public void CycleWeapon_DirectionZeroIsNoOp()
    {
        var inventory = new InventoryComponent();
        var weapon = new Weapon();
        inventory.AddChild(weapon);

        godot.Tree.Root.AddChild(inventory);

        inventory.CycleWeapon(1);
        var previouslyEquipped = inventory.EquippedWeapon;

        inventory.CycleWeapon(0);
        inventory.EquippedWeapon.ShouldBe(previouslyEquipped);
    }

    [Fact]
    public void CycleWeapon_WithNoWeaponsRemainsNull()
    {
        var inventory = new InventoryComponent();
        inventory.AddChild(new ItemInstance());

        godot.Tree.Root.AddChild(inventory);

        inventory.CycleWeapon(1);
        inventory.EquippedWeapon.ShouldBeNull();
    }

    [Fact]
    public void LoadFromSave_WhenSaveHasNoInventoryKey_DoesNothing()
    {
        var inventory = new InventoryComponent();
        var save = new SaveFile();

        inventory.LoadFromSave(save);

        inventory.GetChildren().OfType<Weapon>().Count().ShouldBe(0);
        inventory.EquippedWeapon.ShouldBeNull();
    }

    [Fact]
    public void LoadFromSave_WhenSaveHasWeapons_AddsAndEquipsByIndex()
    {
        var inventory = new InventoryComponent();
        var save = new SaveFile
        {
            InventoryWeapons = new Godot.Collections.Array<string>
            {
                PelletShooterPath,
                RocketLauncherPath,
            },
            InventoryEquippedIndex = 1,
        };

        inventory.LoadFromSave(save);

        var weapons = inventory.GetChildren().OfType<Weapon>().ToList();
        weapons.Count.ShouldBe(2);
        inventory.EquippedWeapon.ShouldBe(weapons[1]);
        inventory.EquippedWeapon.SceneFilePath.ShouldBe(RocketLauncherPath);
    }

    [Fact]
    public void LoadFromSave_WhenWeaponAlreadyExists_SkipsDuplicate()
    {
        var inventory = new InventoryComponent();
        var existing = ResourceLoader
            .Load<PackedScene>(RocketLauncherPath)
            .Instantiate<Weapon>();
        inventory.AddChild(existing);

        var save = new SaveFile
        {
            InventoryWeapons = new Godot.Collections.Array<string> { RocketLauncherPath },
            InventoryEquippedIndex = 0,
        };

        inventory.LoadFromSave(save);

        inventory.GetChildren().OfType<Weapon>().Count().ShouldBe(1);
    }

    [Fact]
    public void PersistToSave_StoresWeaponPathsAndEquippedIndex()
    {
        var inventory = new InventoryComponent();
        var weaponA = new Weapon();
        weaponA.SetMeta("scene_file_path", PelletShooterPath);
        var weaponB = new Weapon();
        weaponB.SetMeta("scene_file_path", RocketLauncherPath);
        inventory.AddChild(weaponA);
        inventory.AddChild(weaponB);
        inventory.EquipWeapon(weaponB, recursive: false);

        var save = new SaveFile();

        inventory.PersistToSave(save);

        save.InventoryWeapons.Count.ShouldBe(2);
        save.InventoryWeapons[0].ShouldBe(PelletShooterPath);
        save.InventoryWeapons[1].ShouldBe(RocketLauncherPath);
        save.InventoryEquippedIndex.ShouldBe(1);
    }
}
