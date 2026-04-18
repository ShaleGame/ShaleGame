using CrossedDimensions.Environment;
using Shouldly;
using Xunit;

namespace CrossedDimensions.Tests.Environment;

[Collection("GodotHeadless")]
public class AreaManagerTest
{
    private readonly GodotHeadlessFixedFpsFixture _godot;

    public AreaManagerTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;
    }

    [Fact]
    public void NotifyAreaTitleTriggerEntered_WithAreaData_EmitsSignal()
    {
        var manager = new AreaManager();
        var data = new AreaData
        {
            Title = "Crystal Peak",
            Subtitle = "Whispering Ridges"
        };

        AreaData emitted = null;
        manager.AreaTriggerEntered += areaData => emitted = areaData;

        manager.NotifyAreaTitleTriggerEntered(data);

        emitted.ShouldNotBeNull();
        emitted.ShouldBeSameAs(data);
    }
}
