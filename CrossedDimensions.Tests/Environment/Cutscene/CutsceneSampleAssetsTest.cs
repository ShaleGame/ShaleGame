using CrossedDimensions.Characters;
using CrossedDimensions.Environment.Cutscene;
using CrossedDimensions.Environment.Triggers;
using Godot;
using Shouldly;
using Xunit;

namespace CrossedDimensions.Tests.Environment.Cutscene;

[Collection("GodotHeadless")]
public sealed class CutsceneSampleAssetsTest
{
    private const string SampleCutsceneScenePath =
        "res://Environment/Cutscene/Samples/SampleCutsceneScene.tscn";
    private const string SampleCutsceneTriggerPath =
        "res://Environment/Cutscene/Samples/SampleCutsceneTrigger.tscn";

    public CutsceneSampleAssetsTest(GodotHeadlessFixedFpsFixture godot)
    {
        _ = godot;
    }

    [Fact]
    public void SampleCutsceneScene_ShouldUseAnimationPlayerDrivenFlow()
    {
        var packedScene = ResourceLoader.Load<PackedScene>(SampleCutsceneScenePath);
        packedScene.ShouldNotBeNull();

        var cutscene = packedScene.Instantiate<CutsceneScene>();

        cutscene.StartAnimation.ShouldBe("sample_walk");
        cutscene.AnimationPlayer.ShouldNotBeNull();
        cutscene.AnimationPlayer.GetAnimation(cutscene.StartAnimation).ShouldNotBeNull();

        var cutsceneActor = cutscene.GetNode<Character>("CutsceneActor");
        cutsceneActor.Controller.ShouldBeNull();

        cutscene.Free();
    }

    [Fact]
    public void SampleCutsceneTrigger_ShouldPointAtSampleCutsceneScene()
    {
        var packedScene = ResourceLoader.Load<PackedScene>(SampleCutsceneTriggerPath);
        packedScene.ShouldNotBeNull();

        var trigger = packedScene.Instantiate<CutsceneTrigger>();

        trigger.Cutscene.ShouldNotBeNull();
        trigger.Cutscene.CutsceneScenePath.ShouldBe(SampleCutsceneScenePath);
        trigger.Cutscene.RepositionPlayerOnReturn.ShouldBeFalse();
        trigger.Cutscene.ReturnPlayerPosition.ShouldBe(new Vector2(112, 0));
        trigger.ReturnPlayerMarker.ShouldNotBeNull();
        trigger.ReturnPlayerMarker.Position.ShouldBe(new Vector2(112, 0));
        trigger.SaveKey.ShouldBe("samples/sample_cutscene_trigger_consumed");
        trigger.DisableAfterPlaying.ShouldBeTrue();
        trigger.DisableImmediatelyOnTrigger.ShouldBeTrue();
        trigger.DestroyAfterPlaying.ShouldBeFalse();

        trigger.Free();
    }
}
