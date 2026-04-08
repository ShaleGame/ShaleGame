using CrossedDimensions.Saves;
using Godot;
using Xunit;
using System;
using Shouldly;

namespace CrossedDimensions.Tests.Saves;

[Collection("GodotHeadless")]
public partial class SaveManagerTest(GodotHeadlessFixedFpsFixture godot)
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

        var save = saveManager.CreateNewSave();

        SaveManager.Instance.CurrentSave.ShouldBe(save);
        save.SaveName.ShouldNotBeNullOrEmpty();
        save.Timestamp.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void SaveManager_CreateNewSave_ReturnsFreshSaveState()
    {
        var saveManager = new SaveManager();
        godot.Tree.Root.AddChild(saveManager);

        var firstSave = saveManager.CreateNewSave();
        firstSave.SetKey("cutscene/consumed", true);

        var secondSave = saveManager.CreateNewSave();

        secondSave.ShouldNotBeSameAs(firstSave);
        secondSave.TryGetKey<bool>("cutscene/consumed", out _).ShouldBeFalse();
        SaveManager.Instance.CurrentSave.ShouldBe(secondSave);
    }

    [Fact]
    public void SaveManager_ForwardsKeySetSignalFromSave()
    {
        var saveManager = new SaveManager();
        godot.Tree.Root.AddChild(saveManager);

        // create and assign a SaveFile and connect to SaveManager's KeySet
        var save = new SaveFile();
        var receiver = new TestReceiver();
        godot.Tree.Root.AddChild(receiver);

        bool managerSignalFired = false;
        string firedKey = null;
        Variant firedValue = new Variant();

        receiver.OnKeySetAction = (key, value) =>
        {
            managerSignalFired = true;
            firedKey = key;
            firedValue = value;
        };

        saveManager.Connect("KeySet", new Callable(receiver, nameof(TestReceiver.OnKeySet)));

        // assign save as current - SaveManager should wire up signals automatically
        saveManager.CurrentSave = save;

        // set a key on the SaveFile directly and ensure SaveManager forwards it
        save.SetKey("test_key", 123);

        managerSignalFired.ShouldBeTrue();
        firedKey.ShouldBe("test_key");
        firedValue.As<int>().ShouldBe(123);
    }

    private partial class TestReceiver : Node
    {
        public Action<string, Variant> OnKeySetAction { get; set; }

        public void OnKeySet(string key, Variant value)
        {
            OnKeySetAction?.Invoke(key, value);
        }
    }
}
