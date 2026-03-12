using CrossedDimensions.Environment.Cutscene.Interactables;
using Godot;
using Shouldly;
using Xunit;

namespace CrossedDimensions.Tests.Environment.Cutscene;

[Collection("GodotHeadless")]
public class InteractableTest(GodotHeadlessFixedFpsFixture godot)
{
    [Fact]
    public void DefaultsAreCorrect()
    {
        var interactable = new Interactable();

        // 2026-03-11: changed to 0.5 seconds since 1 second felt too long
        interactable.HoldSecs.ShouldBe(0.5f);
        interactable.InteractPriority.ShouldBe(0);
        interactable.InteractAction.ToString().ShouldBe("interact");
    }
}
