using CrossedDimensions.Saves;
using Xunit;
using Shouldly;

namespace CrossedDimensions.Tests.Saves;

[Collection("GodotHeadless")]
public class SaveFileTest
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
}
