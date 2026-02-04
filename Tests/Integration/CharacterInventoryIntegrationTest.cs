using CrossedDimensions.Characters;
using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.Integration;

[TestSuite]
[TestCategory("Integration")]
public partial class CharacterInventoryIntegrationTest
{
    private Node _scene;
    private Character _character;
    private Items.Weapon _rocketLauncher1;
    private Items.Weapon _rocketLauncher2;

    [BeforeTest]
    public void SetupTest()
    {
        var runner = ISceneRunner
            .Load("res://Tests/Integration/CharacterInventoryIntegrationTest.tscn");
        _scene = runner.Scene();
        _character = _scene.GetNode<Character>("Character");

        _rocketLauncher1 = _scene.GetNode<Items.Weapon>("%RocketLauncher1");
        _rocketLauncher2 = _scene.GetNode<Items.Weapon>("%RocketLauncher2");
    }

    [AfterTest]
    public void TearDownTest()
    {
        _scene?.QueueFree();
        _scene = null;
    }

    [TestCase]
    [RequireGodotRuntime]
    public void GivenScene_WhenLoaded_ShouldInitializeCorrectly()
    {
        AssertThat(_scene).IsNotNull();
        AssertThat(_character).IsNotNull();
        AssertThat(_character.Inventory).IsNotNull();
        AssertThat(_rocketLauncher1).IsNotNull();
        AssertThat(_rocketLauncher2).IsNotNull();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void GivenTwoItemsInInventory_WhenReady_ThenHaveBothItemsRegistered()
    {
        var inventory = _character.Inventory;

        AssertThat(inventory.Items.Count).IsEqual(2);
        AssertThat(inventory.Items).Contains(_rocketLauncher1);
        AssertThat(inventory.Items).Contains(_rocketLauncher2);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void GivenInventory_WhenEquippingWeapon_ThenActivateIt()
    {
        var inventory = _character.Inventory;

        inventory.EquipWeapon(_rocketLauncher1);

        AssertThat(inventory.IsWeaponEquipped(_rocketLauncher1)).IsTrue();
        AssertThat(_rocketLauncher1.IsActive).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void GivenInventoryWithEquippedWeapon_WhenCyclingNext_ThenEquipNextWeapon()
    {
        var inventory = _character.Inventory;

        inventory.EquipWeapon(_rocketLauncher1);
        inventory.CycleWeapon(1);

        AssertThat(inventory.IsWeaponEquipped(_rocketLauncher2)).IsTrue();
        AssertThat(_rocketLauncher2.IsActive).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void GivenInventoryWithEquippedWeapon_WhenCyclingPrevious_ThenEquipPreviousWeapon()
    {
        var inventory = _character.Inventory;

        inventory.EquipWeapon(_rocketLauncher2);
        inventory.CycleWeapon(-1);

        AssertThat(inventory.IsWeaponEquipped(_rocketLauncher1)).IsTrue();
        AssertThat(_rocketLauncher1.IsActive).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void GivenInventoryWithEquippedWeapon_WhenEquippingWeapon_ThenDeactivatePrevious()
    {
        var inventory = _character.Inventory;

        inventory.EquipWeapon(_rocketLauncher1);
        inventory.EquipWeapon(_rocketLauncher2);

        AssertThat(_rocketLauncher1.IsActive).IsFalse();
    }
}
