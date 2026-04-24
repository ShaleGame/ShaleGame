using CrossedDimensions.UI.UISettings;

namespace CrossedDimensions.Tests.Settings;

[Collection("GodotHeadless")]
public class SettingsResourceTest
{
    public SettingsResourceTest(GodotHeadlessFixedFpsFixture godot)
    {

    }

    [Fact]
    public void WindowMode_WhenSetBelowRange_ShouldClampToZero()
    {
        var settings = new SettingsResource();

        settings.WindowMode = -1;

        settings.WindowMode.ShouldBe(0);
    }

    [Fact]
    public void WindowMode_WhenSetAboveRange_ShouldClampToTwo()
    {
        var settings = new SettingsResource();

        settings.WindowMode = 3;

        settings.WindowMode.ShouldBe(2);
    }

    [Fact]
    public void WindowMode_WhenSetToValidValues_ShouldSetCorrectly()
    {
        var settings = new SettingsResource();

        settings.WindowMode = 0;
        settings.WindowMode.ShouldBe(0);

        settings.WindowMode = 1;
        settings.WindowMode.ShouldBe(1);

        settings.WindowMode = 2;
        settings.WindowMode.ShouldBe(2);
    }

    [Fact]
    public void ScreenShakeEnabled_ShouldDefaultToTrue()
    {
        var settings = new SettingsResource();

        settings.ScreenShakeEnabled.ShouldBeTrue();
    }

    [Fact]
    public void VisualEffectsEnabled_ShouldDefaultToTrue()
    {
        var settings = new SettingsResource();

        settings.VisualEffectsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void HdrEnabled_ShouldDefaultToTrue()
    {
        var settings = new SettingsResource();

        settings.HdrEnabled.ShouldBeTrue();
    }

    [Fact]
    public void ToggleFields_WhenSet_ShouldPersistValues()
    {
        var settings = new SettingsResource();

        settings.ScreenShakeEnabled = false;
        settings.VisualEffectsEnabled = false;
        settings.HdrEnabled = false;

        settings.ScreenShakeEnabled.ShouldBeFalse();
        settings.VisualEffectsEnabled.ShouldBeFalse();
        settings.HdrEnabled.ShouldBeFalse();
    }
}
