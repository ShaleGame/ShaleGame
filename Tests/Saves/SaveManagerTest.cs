using CrossedDimensions.Saves;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.Saves;

[TestSuite]
public partial class SaveManagerTest
{
    [TestCase]
    [RequireGodotRuntime]
    public void SaveManager_SetsInstance()
    {
        var saveManager = new SaveManager();
        AddNode(saveManager);
        AssertThat(SaveManager.Instance).IsEqual(saveManager);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void SaveManager_CreatesNewSave()
    {
        var saveManager = new SaveManager();
        AddNode(saveManager);
        saveManager.CreateNewSave();
        AssertThat(SaveManager.Instance.CurrentSave).IsNotNull();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void SaveManager_SetsKey()
    {
        var saveManager = new SaveManager();
        AddNode(saveManager);
        saveManager.CreateNewSave();
        saveManager.SetKey("key", 0);
        AssertThat(saveManager.CurrentSave.KeyValue["key"]).IsEqual(0);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void SaveManager_GetsKey()
    {
        var saveManager = new SaveManager();
        AddNode(saveManager);
        saveManager.CreateNewSave();
        saveManager.CurrentSave.KeyValue["key"] = 2;
        AssertThat(saveManager.GetKey<int>("key")).IsEqual(2);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void SaveManager_TriesToGetExistingKey()
    {
        var saveManager = new SaveManager();
        AddNode(saveManager);
        saveManager.CreateNewSave();
        saveManager.CurrentSave.KeyValue["key"] = 5;

        var ok = saveManager.TryGetKey<int>("key", out var value);
        AssertThat(ok).IsTrue();
        AssertThat(value).IsEqual(5);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void SaveManager_TriesToGetNonExistingKey()
    {
        var saveManager = new SaveManager();
        AddNode(saveManager);
        saveManager.CreateNewSave();

        var ok = saveManager.TryGetKey<int>("missing", out var value);
        AssertThat(ok).IsFalse();
        AssertThat(value).IsEqual(default(int));
    }

    [TestCase]
    [RequireGodotRuntime]
    public void SaveManager_GetsNonExistingKeyWithDefaultValue()
    {
        var saveManager = new SaveManager();
        AddNode(saveManager);
        saveManager.CreateNewSave();

        var result = saveManager.GetKeyOrDefault<int>("missing", 42);
        AssertThat(result).IsEqual(42);
    }
}
