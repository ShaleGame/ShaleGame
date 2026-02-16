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

        interactable.HoldSecs.ShouldBe(1.0f);
        interactable.InteractPriority.ShouldBe(0);
        interactable.InteractAction.ToString().ShouldBe("interact");
    }
}
