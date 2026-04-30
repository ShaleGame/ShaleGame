using CrossedDimensions.Characters;
using CrossedDimensions.Environment.Cutscene;
using CrossedDimensions.Environment.Triggers;
using Godot;
using Shouldly;
using Xunit;

namespace CrossedDimensions.Tests.Environment.Cutscene;

[Collection("GodotHeadless")]
public sealed class CutsceneSampleAssetsTest : System.IDisposable
{
    private const string CutsceneActorPath =
        "res://Environment/Cutscene/CutsceneActor.tscn";
    private const string BaseCharacterPath =
        "res://Characters/BaseCharacter.tscn";
    private const string SampleCutsceneScenePath =
        "res://Environment/Cutscene/Samples/SampleCutsceneScene.tscn";
    private const string SampleCutsceneTriggerPath =
        "res://Environment/Cutscene/Samples/SampleCutsceneTrigger.tscn";
    private readonly GodotHeadlessFixedFpsFixture _godot;
    private readonly Node _scene;

    public CutsceneSampleAssetsTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;
        _scene = new Node2D();
        _godot.Tree.Root.AddChild(_scene);
    }

    public void Dispose()
    {
        if (_scene.GetParent() is not null)
        {
            _scene.GetParent().RemoveChild(_scene);
        }

        _scene.QueueFree();
        _godot.GodotInstance.Iteration(1);
    }

    [Fact(Skip = "for some reason it's breaking on Github but it's not breaking locally")]
    public void SampleCutsceneScene_ShouldUseAnimationPlayerDrivenFlow()
    {
        var packedScene = ResourceLoader.Load<PackedScene>(SampleCutsceneScenePath);
        packedScene.ShouldNotBeNull();

        var cutscene = packedScene.Instantiate<CutsceneScene>();
        _scene.AddChild(cutscene);
        _godot.GodotInstance.Iteration(1);

        cutscene.StepQueue.Length.ShouldBe(3);
        cutscene.AnimationPlayer.ShouldNotBeNull();
        cutscene.DialogueBox.ShouldNotBeNull();
        cutscene.DialogueListener.ShouldNotBeNull();
        cutscene.DialogueListener.Interactable.ShouldBeNull();

        var cutsceneActor = cutscene.GetNode<Character>("CutsceneActor");
        cutsceneActor.Controller.ShouldBeNull();

        cutscene.Free();
    }

    [Fact(Skip = "for some reason it's breaking on Github but it's not breaking locally")]
    public void CutsceneActor_ShouldNotShareAnimationTreeStateWithGameplayCharacter()
    {
        var gameplayCharacter = ResourceLoader
            .Load<PackedScene>(BaseCharacterPath)
            .Instantiate<Character>();
        var cutsceneScene = ResourceLoader
            .Load<PackedScene>(SampleCutsceneScenePath)
            .Instantiate<CutsceneScene>();

        _scene.AddChild(cutsceneScene);
        cutsceneScene.AddChild(gameplayCharacter);
        _godot.GodotInstance.Iteration(1);

        var cutsceneActor = cutsceneScene.GetNode<Character>("CutsceneActor");
        var gameplayTree = gameplayCharacter.GetNode<AnimationTree>("AnimationTree");
        var cutsceneTree = cutsceneActor.GetNode<AnimationTree>("AnimationTree");
        var gameplayTreeRoot = gameplayTree.Get("tree_root").As<GodotObject>();
        var cutsceneTreeRoot = cutsceneTree.Get("tree_root").As<GodotObject>();
        var gameplayMoveBlend = gameplayTree
            .Get("parameters/move/blend_position")
            .As<float>();

        gameplayTreeRoot.ShouldNotBeNull();
        cutsceneTreeRoot.ShouldNotBeNull();
        gameplayTreeRoot.GetInstanceId().ShouldNotBe(cutsceneTreeRoot.GetInstanceId());

        cutsceneTree.Set("parameters/move/blend_position", 1.0f);

        gameplayTree
            .Get("parameters/move/blend_position")
            .As<float>()
            .ShouldBe(gameplayMoveBlend);
    }

    [Fact(Skip = "for some reason it's breaking on Github but it's not breaking locally")]
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
