using System;
using CrossedDimensions.Characters;
using Godot;
using twodog.xunit;
using Xunit;
using Shouldly;
using CrossedDimensions.Items;

namespace CrossedDimensions.Tests.Integration;

[Collection("GodotHeadless")]
public class CharacterInventoryIntegrationTest : IDisposable
{
    private const string ScenePath = $"{Paths.TestPath}/Integration/CharacterInventoryIntegrationTest.tscn";

    private readonly GodotHeadlessFixture _godot;
    private Node _scene;
    private Character _character;
    private Weapon _rocketLauncher1;
    private Weapon _rocketLauncher2;

    public CharacterInventoryIntegrationTest(GodotHeadlessFixture godot)
    {
        _godot = godot;
        _scene = null;

        var packed = ResourceLoader.Load<PackedScene>(ScenePath);
        _scene = packed.Instantiate() as Node;
        _godot.Tree.Root.AddChild(_scene);

        _character = _scene.GetNode<Character>("Character");
        _rocketLauncher1 = _scene.GetNode<Weapon>("%RocketLauncher1");
        _rocketLauncher2 = _scene.GetNode<Weapon>("%RocketLauncher2");
    }

    public void Dispose()
    {
        _scene?.QueueFree();
        _scene = null;
    }

    [Fact]
    public void GivenScene_WhenLoaded_ShouldInitializeCorrectly()
    {
        _scene.ShouldNotBeNull();
        _character.ShouldNotBeNull();
        _character.Inventory.ShouldNotBeNull();
        _rocketLauncher1.ShouldNotBeNull();
        _rocketLauncher2.ShouldNotBeNull();
    }

    [Fact]
    public void GivenInventory_WhenEquippingWeapon_ThenActivateIt()
    {
        var inventory = _character.Inventory;

        inventory.EquipWeapon(_rocketLauncher1);

        inventory.IsWeaponEquipped(_rocketLauncher1).ShouldBeTrue();
        _rocketLauncher1.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GivenInventoryWithEquippedWeapon_WhenCyclingNext_ThenEquipNextWeapon()
    {
        var inventory = _character.Inventory;

        inventory.EquipWeapon(_rocketLauncher1);
        inventory.CycleWeapon(1);

        inventory.IsWeaponEquipped(_rocketLauncher2).ShouldBeTrue();
        _rocketLauncher2.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GivenInventoryWithEquippedWeapon_WhenCyclingPrevious_ThenEquipPreviousWeapon()
    {
        var inventory = _character.Inventory;

        inventory.EquipWeapon(_rocketLauncher2);
        inventory.CycleWeapon(-1);

        inventory.IsWeaponEquipped(_rocketLauncher1).ShouldBeTrue();
        _rocketLauncher1.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GivenInventoryWithEquippedWeapon_WhenEquippingWeapon_ThenDeactivatePrevious()
    {
        var inventory = _character.Inventory;

        inventory.EquipWeapon(_rocketLauncher1);
        inventory.EquipWeapon(_rocketLauncher2);

        _rocketLauncher1.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void GivenCharacter_WhenSplitting_ThenCloneEquipsSameWeaponKind()
    {
        var inventory = _character.Inventory;

        inventory.EquipWeapon(_rocketLauncher1);

        var clone = _character.Cloneable.Split();
        clone.Inventory._Ready();

        clone.Inventory.ShouldNotBeNull();
        clone.Inventory.EquippedWeapon.Name.ToString().ShouldBe(_rocketLauncher1.Name.ToString());
    }

    [Fact]
    public void GivenCharacter_WhenSplitting_ThenCloneEquipsDifferentWeaponInstance()
    {
        var inventory = _character.Inventory;

        inventory.EquipWeapon(_rocketLauncher1);

        var clone = _character.Cloneable.Split();
        clone.Inventory._Ready();

        clone.Inventory.EquippedWeapon.ShouldNotBeSameAs(_rocketLauncher1);
    }

    [Fact]
    public void GivenCharacter_WhenSplitting_ThenCloneActivatesDifferentWeaponInstance()
    {
        var inventory = _character.Inventory;

        inventory.EquipWeapon(_rocketLauncher1);

        var clone = _character.Cloneable.Split();

        clone.Inventory.EquippedWeapon.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GivenCharacter_WhenSwitchingToSlot0_ThenEquipFirstWeapon()
    {
        var inventory = _character.Inventory;

        inventory.EquipWeapon(_rocketLauncher2);
        inventory.EquipWeaponByIndex(0);

        _rocketLauncher1.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GivenCharacter_WhenSwitchingToSlot1_ThenEquipSecondWeapon()
    {
        var inventory = _character.Inventory;

        inventory.EquipWeapon(_rocketLauncher1);
        inventory.EquipWeaponByIndex(1);

        _rocketLauncher2.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GivenCharacterWithClone_WhenWeaponAdded_ThenBothInventoriesHaveWeapon()
    {
        var inventory = _character.Inventory;
        inventory._Ready();

        var clone = _character.Cloneable.Split();

        var newWeapon = new Weapon();
        newWeapon.Name = "NewWeapon";
        inventory.AddChild(newWeapon);
        inventory._Ready();

        clone.Inventory.HasNode("NewWeapon").ShouldBeTrue();
    }

    [Fact]
    public void GivenCharacterWithClone_WhenWeaponAdded_ThenBothInventoriesEquipWeapon()
    {
        var inventory = _character.Inventory;
        inventory._Ready();

        var clone = _character.Cloneable.Split();

        var newWeapon = new Weapon();
        newWeapon.Name = "NewWeapon";
        inventory.AddChild(newWeapon);

        inventory.EquipWeapon(newWeapon);

        clone.Inventory.EquippedWeapon.Name.ToString().ShouldBe("NewWeapon");
    }
}
