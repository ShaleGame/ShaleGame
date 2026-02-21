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
}
