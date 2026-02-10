using CrossedDimensions.Settings;

namespace CrossedDimensions.Tests.Settings;

[Collection("GodotHeadless")]
public class SettingsResourceTest
{
    public SettingsResourceTest(GodotHeadlessFixedFpsFixture godot)
    {

    }

    [Fact]
    public void MasterVolume_WhenSetBelowZero_ShouldClampToZero()
    {
        var settings = new SettingsResource();

        settings.MasterVolume = -0.5f;

        settings.MasterVolume.ShouldBe(0.0f);
    }

    [Fact]
    public void MasterVolume_WhenSetAboveOne_ShouldClampToOne()
    {
        var settings = new SettingsResource();

        settings.MasterVolume = 1.5f;

        settings.MasterVolume.ShouldBe(1.0f);
    }

    [Fact]
    public void MasterVolume_WhenSetToValidValue_ShouldSetCorrectly()
    {
        var settings = new SettingsResource();

        settings.MasterVolume = 0.75f;

        settings.MasterVolume.ShouldBe(0.75f);
    }

    [Fact]
    public void MusicVolume_WhenSetBelowZero_ShouldClampToZero()
    {
        var settings = new SettingsResource();

        settings.MusicVolume = -0.5f;

        settings.MusicVolume.ShouldBe(0.0f);
    }

    [Fact]
    public void MusicVolume_WhenSetAboveOne_ShouldClampToOne()
    {
        var settings = new SettingsResource();

        settings.MusicVolume = 1.5f;

        settings.MusicVolume.ShouldBe(1.0f);
    }

    [Fact]
    public void MusicVolume_WhenSetToValidValue_ShouldSetCorrectly()
    {
        var settings = new SettingsResource();

        settings.MusicVolume = 0.5f;

        settings.MusicVolume.ShouldBe(0.5f);
    }

    [Fact]
    public void SfxVolume_WhenSetBelowZero_ShouldClampToZero()
    {
        var settings = new SettingsResource();

        settings.SfxVolume = -0.5f;

        settings.SfxVolume.ShouldBe(0.0f);
    }

    [Fact]
    public void SfxVolume_WhenSetAboveOne_ShouldClampToOne()
    {
        var settings = new SettingsResource();

        settings.SfxVolume = 1.5f;

        settings.SfxVolume.ShouldBe(1.0f);
    }

    [Fact]
    public void SfxVolume_WhenSetToValidValue_ShouldSetCorrectly()
    {
        var settings = new SettingsResource();

        settings.SfxVolume = 0.25f;

        settings.SfxVolume.ShouldBe(0.25f);
    }

    [Fact]
    public void AllVolumeProperties_WhenSetToBoundaryValues_ShouldClampCorrectly()
    {
        var settings = new SettingsResource();

        settings.MasterVolume = 0.0f;
        settings.MusicVolume = 0.0f;
        settings.SfxVolume = 0.0f;

        settings.MasterVolume.ShouldBe(0.0f);
        settings.MusicVolume.ShouldBe(0.0f);
        settings.SfxVolume.ShouldBe(0.0f);

        settings.MasterVolume = 1.0f;
        settings.MusicVolume = 1.0f;
        settings.SfxVolume = 1.0f;

        settings.MasterVolume.ShouldBe(1.0f);
        settings.MusicVolume.ShouldBe(1.0f);
        settings.SfxVolume.ShouldBe(1.0f);
    }
}
