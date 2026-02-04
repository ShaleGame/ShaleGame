using System.Threading.Tasks;
using CrossedDimensions.Components;
using CrossedDimensions.Items;
using CrossedDimensions.Characters;
using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.Components;

[TestSuite]
public partial class InventoryComponentTest
{
    [TestCase]
    [RequireGodotRuntime]
    public void Inventory_WhenCreated_ShouldHaveEmptyItems()
    {
        var inventory = new InventoryComponent();
        AddNode(inventory);

        AssertThat(inventory.Items.Count).IsEqual(0);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Ready_ShouldDiscoverChildItems_AndDeactivateWeapons()
    {
        var inventory = new InventoryComponent();
        var weapon = new Weapon();
        inventory.AddChild(weapon);

        AddNode(inventory);

        AssertThat(inventory.Items).Contains(weapon);
        AssertThat(weapon.IsActive).IsFalse();
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task EquipWeapon_ShouldActivateWeapon_DeactivatePrevious_AndEmitSignal()
    {
        var inventory = new InventoryComponent();
        var weaponA = new Weapon();
        var weaponB = new Weapon();
        inventory.AddChild(weaponA);
        inventory.AddChild(weaponB);

        AddNode(inventory);

        AssertSignal(inventory).StartMonitoring();

        inventory.EquipWeapon(weaponA);

        AssertThat(weaponA.IsActive).IsTrue();
        AssertThat(inventory.IsWeaponEquipped(weaponA)).IsTrue();

        inventory.EquipWeapon(weaponB);

        AssertThat(weaponA.IsActive).IsFalse();
        AssertThat(weaponB.IsActive).IsTrue();
        AssertThat(inventory.IsWeaponEquipped(weaponB)).IsTrue();

        await AssertSignal(inventory)
            .IsEmitted(InventoryComponent.SignalName.EquippedWeaponChanged, weaponA, weaponB)
            .WithTimeout(200);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CycleWeapon_ShouldWrapForwardAndAround()
    {
        var inventory = new InventoryComponent();
        var left = new Weapon();
        var middle = new Weapon();
        var right = new Weapon();
        inventory.AddChild(left);
        inventory.AddChild(middle);
        inventory.AddChild(right);

        AddNode(inventory);

        inventory.CycleWeapon(1);
        AssertThat(inventory.EquippedWeapon).IsEqual(left);

        inventory.CycleWeapon(1);
        AssertThat(inventory.EquippedWeapon).IsEqual(middle);

        inventory.CycleWeapon(1);
        AssertThat(inventory.EquippedWeapon).IsEqual(right);

        inventory.CycleWeapon(1);
        AssertThat(inventory.EquippedWeapon).IsEqual(left);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CycleWeapon_ShouldWrapBackward()
    {
        var inventory = new InventoryComponent();
        var left = new Weapon();
        var middle = new Weapon();
        var right = new Weapon();
        inventory.AddChild(left);
        inventory.AddChild(middle);
        inventory.AddChild(right);

        AddNode(inventory);

        inventory.CycleWeapon(-1);
        AssertThat(inventory.EquippedWeapon).IsEqual(right);

        inventory.CycleWeapon(-1);
        AssertThat(inventory.EquippedWeapon).IsEqual(middle);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CycleWeapon_SkipsNonWeapons()
    {
        var inventory = new InventoryComponent();
        inventory.AddChild(new ItemInstance());
        var weapon = new Weapon();
        inventory.AddChild(weapon);

        AddNode(inventory);

        inventory.CycleWeapon(1);
        AssertThat(inventory.EquippedWeapon).IsEqual(weapon);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CycleWeapon_DirectionZeroIsNoOp()
    {
        var inventory = new InventoryComponent();
        var weapon = new Weapon();
        inventory.AddChild(weapon);

        AddNode(inventory);

        inventory.CycleWeapon(1);
        var previouslyEquipped = inventory.EquippedWeapon;

        inventory.CycleWeapon(0);
        AssertThat(inventory.EquippedWeapon).IsEqual(previouslyEquipped);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CycleWeapon_WithNoWeaponsRemainsNull()
    {
        var inventory = new InventoryComponent();
        inventory.AddChild(new ItemInstance());

        AddNode(inventory);

        inventory.CycleWeapon(1);
        AssertThat(inventory.EquippedWeapon).IsNull();
    }
}
