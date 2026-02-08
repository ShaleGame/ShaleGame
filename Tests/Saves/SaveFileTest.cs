using CrossedDimensions.Saves;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.Saves;

[TestSuite]
public partial class SaveFileTest
{
    [TestCase]
    [RequireGodotRuntime]
    public void SaveFile_ShouldStoreVariantTypes()
    {
        var saveFile = new SaveFile();
        saveFile.KeyValue["player_health"] = 100;
        saveFile.KeyValue["has_key"] = true;
        AssertThat(saveFile.KeyValue["player_health"]).IsEqual(100);
        AssertThat(saveFile.KeyValue["has_key"]).IsTrue();
    }
}
