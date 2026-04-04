using CrossedDimensions.Saves;
using Godot;
using Xunit;
using Shouldly;

namespace CrossedDimensions.Tests.Saves;

[Collection("GodotHeadless")]
public class SaveFileTest(GodotHeadlessFixedFpsFixture godot)
{
    [Fact]
    public void SaveFile_ShouldStoreVariantTypes()
    {
        var saveFile = new SaveFile();
        saveFile.KeyValue["player_health"] = 100;
        saveFile.KeyValue["has_key"] = true;
        saveFile.KeyValue["player_health"].ShouldBe(100);
        saveFile.KeyValue["has_key"].AsBool().ShouldBeTrue();
    }

    [Fact]
    public void SaveFile_SetsKey()
    {
        var save = new SaveFile();
        save.SetKey("key", 0);
        save.KeyValue["key"].ShouldBe(0);
    }

    [Fact]
    public void SaveFile_GetsKey()
    {
        var save = new SaveFile();
        save.KeyValue["key"] = 2;
        save.GetKey<int>("key").ShouldBe(2);
    }

    [Fact]
    public void SaveFile_TriesToGetExistingKey()
    {
        var save = new SaveFile();
        save.KeyValue["key"] = 5;

        var ok = save.TryGetKey<int>("key", out var value);
        ok.ShouldBeTrue();
        value.ShouldBe(5);
    }

    [Fact]
    public void SaveFile_TriesToGetNonExistingKey()
    {
        var save = new SaveFile();

        var ok = save.TryGetKey<int>("missing", out var value);
        ok.ShouldBeFalse();
        value.ShouldBe(default(int));
    }

    [Fact]
    public void SaveFile_GetsNonExistingKeyWithDefaultValue()
    {
        var save = new SaveFile();

        var result = save.GetKeyOrDefault<int>("missing", 42);
        result.ShouldBe(42);
    }

    [Fact]
    public void InventoryWeapons_WhenMissingKey_ReturnsEmptyArray()
    {
        var save = new SaveFile();

        save.InventoryWeapons.Count.ShouldBe(0);
    }

    [Fact]
    public void InventoryProperties_RoundTripValues()
    {
        var save = new SaveFile();
        var weapons = new Godot.Collections.Array<string>
        {
            "res://Items/PelletShooter.tscn",
            "res://Items/RocketLauncher.tscn",
        };

        save.InventoryWeapons = weapons;
        save.InventoryEquippedIndex = 1;

        save.InventoryWeapons.Count.ShouldBe(2);
        save.InventoryWeapons[0].ShouldBe("res://Items/PelletShooter.tscn");
        save.InventoryWeapons[1].ShouldBe("res://Items/RocketLauncher.tscn");
        save.InventoryEquippedIndex.ShouldBe(1);
    }
}
