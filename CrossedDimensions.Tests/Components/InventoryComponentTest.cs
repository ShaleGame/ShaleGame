using CrossedDimensions.Components;
using CrossedDimensions.Items;

namespace CrossedDimensions.Tests.Components;

[Collection("GodotHeadless")]
public class InventoryComponentTest(GodotHeadlessFixedFpsFixture godot)
{
    [Fact]
    public void Ready_ShouldDeactivateWeapons()
    {
        var inventory = new InventoryComponent();
        var weapon = new Weapon();
        inventory.AddChild(weapon);

        godot.Tree.Root.AddChild(inventory);

        weapon.IsActive.ShouldBeFalse();
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

        inventory.CycleWeapon(1);
        inventory.EquippedWeapon.ShouldBe(left);

        inventory.CycleWeapon(1);
        inventory.EquippedWeapon.ShouldBe(middle);

        inventory.CycleWeapon(1);
        inventory.EquippedWeapon.ShouldBe(right);

        inventory.CycleWeapon(1);
        inventory.EquippedWeapon.ShouldBe(left);
    }

    [Fact]
    public void CycleWeapon_WithNoWeaponEquipped_SholdEquipFirstWeapon()
    {
        var inventory = new InventoryComponent();
        var left = new Weapon();
        var middle = new Weapon();
        var right = new Weapon();
        inventory.AddChild(left);
        inventory.AddChild(middle);
        inventory.AddChild(right);

        godot.Tree.Root.AddChild(inventory);

        inventory.CycleWeapon(10);
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
}
