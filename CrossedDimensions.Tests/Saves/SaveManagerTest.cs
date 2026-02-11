using CrossedDimensions.Saves;
using Godot;
using Xunit;
using Shouldly;

namespace CrossedDimensions.Tests.Saves;

[Collection("GodotHeadless")]
public class SaveManagerTest(GodotHeadlessFixedFpsFixture godot)
{
    [Fact]
    public void SaveManager_SetsInstance()
    {
        var saveManager = new SaveManager();
        godot.Tree.Root.AddChild(saveManager);
        SaveManager.Instance.ShouldBe(saveManager);
    }

    [Fact]
    public void SaveManager_CreatesNewSave()
    {
        var saveManager = new SaveManager();
        godot.Tree.Root.AddChild(saveManager);
        saveManager.CreateNewSave();
        SaveManager.Instance.CurrentSave.ShouldNotBeNull();
    }

    [Fact]
    public void SaveManager_SetsKey()
    {
        var saveManager = new SaveManager();
        godot.Tree.Root.AddChild(saveManager);
        saveManager.CreateNewSave();
        saveManager.SetKey("key", 0);
        saveManager.CurrentSave.KeyValue["key"].ShouldBe(0);
    }

    [Fact]
    public void SaveManager_GetsKey()
    {
        var saveManager = new SaveManager();
        godot.Tree.Root.AddChild(saveManager);
        saveManager.CreateNewSave();
        saveManager.CurrentSave.KeyValue["key"] = 2;
        saveManager.GetKey<int>("key").ShouldBe(2);
    }

    [Fact]
    public void SaveManager_TriesToGetExistingKey()
    {
        var saveManager = new SaveManager();
        godot.Tree.Root.AddChild(saveManager);
        saveManager.CreateNewSave();
        saveManager.CurrentSave.KeyValue["key"] = 5;

        var ok = saveManager.TryGetKey<int>("key", out var value);
        ok.ShouldBeTrue();
        value.ShouldBe(5);
    }

    [Fact]
    public void SaveManager_TriesToGetNonExistingKey()
    {
        var saveManager = new SaveManager();
        godot.Tree.Root.AddChild(saveManager);
        saveManager.CreateNewSave();

        var ok = saveManager.TryGetKey<int>("missing", out var value);
        ok.ShouldBeFalse();
        value.ShouldBe(default(int));
    }

    [Fact]
    public void SaveManager_GetsNonExistingKeyWithDefaultValue()
    {
        var saveManager = new SaveManager();
        godot.Tree.Root.AddChild(saveManager);
        saveManager.CreateNewSave();

        var result = saveManager.GetKeyOrDefault<int>("missing", 42);
        result.ShouldBe(42);
    }
}
