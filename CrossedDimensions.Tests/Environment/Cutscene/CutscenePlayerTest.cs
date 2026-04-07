using System;
using CrossedDimensions.Environment.Cutscene;
using Godot;
using Shouldly;
using Xunit;

namespace CrossedDimensions.Tests.Environment.Cutscene;

[Collection("GodotHeadless")]
public class CutscenePlayerTest : IDisposable
{
    private readonly GodotHeadlessFixedFpsFixture _godot;
    private readonly Node _host;

    public CutscenePlayerTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;
        _host = new Node();
        _godot.Tree.Root.AddChild(_host);
        _godot.GodotInstance.Iteration(1);
    }

    public void Dispose()
    {
        if (_host.GetParent() is not null)
        {
            _host.GetParent().RemoveChild(_host);
        }

        _host.QueueFree();
        _godot.GodotInstance.Iteration(1);
    }

    [Fact]
    public void StartScene_ShouldSetSceneActiveTrue()
    {
        var (scenePlayer, animationPlayer) = CreateScenePlayer();

        scenePlayer.StartScene(animationPlayer, "cutscene");

        scenePlayer.SceneActive.ShouldBeTrue();
    }

    [Fact]
    public void StartScene_ShouldSendSceneStartSignal()
    {
        var (scenePlayer, animationPlayer) = CreateScenePlayer();

        var fired = false;
        scenePlayer.StartingScene += () => fired = true;

        scenePlayer.StartScene(animationPlayer, "cutscene");

        fired.ShouldBeTrue();
    }

    [Fact]
    public void EndScene_ShouldSetSceneActiveFalse()
    {
        var (scenePlayer, animationPlayer) = CreateScenePlayer();
        scenePlayer.StartScene(animationPlayer, "cutscene");

        scenePlayer.EndScene();

        scenePlayer.SceneActive.ShouldBeFalse();
    }

    [Fact]
    public void EndScene_ShouldSendSceneEndSignal()
    {
        var (scenePlayer, animationPlayer) = CreateScenePlayer();
        scenePlayer.StartScene(animationPlayer, "cutscene");

        var fired = false;
        scenePlayer.EndingScene += () => fired = true;

        scenePlayer.EndScene();

        fired.ShouldBeTrue();
    }

    [Fact]
    public void StartScene_ShouldStoreAnimationPlayer()
    {
        var (scenePlayer, animationPlayer) = CreateScenePlayer();

        scenePlayer.StartScene(animationPlayer, "cutscene");

        scenePlayer.AnimationPlayer.ShouldBeSameAs(animationPlayer);
    }

    [Fact]
    public void StartScene_ShouldStoreAnimationName()
    {
        var (scenePlayer, animationPlayer) = CreateScenePlayer();

        scenePlayer.StartScene(animationPlayer, "cutscene");

        scenePlayer.AnimationName.ShouldBe("cutscene");
    }

    [Fact]
    public void StartScene_ShouldPlayAnimation()
    {
        var (scenePlayer, animationPlayer) = CreateScenePlayer();

        scenePlayer.StartScene(animationPlayer, "cutscene");
        _godot.GodotInstance.Iteration(1);

        animationPlayer.IsPlaying().ShouldBeTrue();
        animationPlayer.CurrentAnimation.ToString().ShouldBe("cutscene");
    }

    [Fact]
    public void StartScene_WithoutArguments_ShouldUseAssignedAnimationPlayer()
    {
        var (scenePlayer, animationPlayer) = CreateScenePlayer();
        scenePlayer.AnimationPlayer = animationPlayer;
        scenePlayer.AnimationName = "cutscene";

        scenePlayer.StartScene();
        _godot.GodotInstance.Iteration(1);

        animationPlayer.IsPlaying().ShouldBeTrue();
        animationPlayer.CurrentAnimation.ToString().ShouldBe("cutscene");
    }

    [Fact]
    public void EndScene_ShouldStopAnimation()
    {
        var (scenePlayer, animationPlayer) = CreateScenePlayer();
        scenePlayer.StartScene(animationPlayer, "cutscene");
        _godot.GodotInstance.Iteration(1);

        scenePlayer.EndScene();

        animationPlayer.IsPlaying().ShouldBeFalse();
    }

    [Fact]
    public void AnimationFinished_ShouldEndScene()
    {
        var (scenePlayer, animationPlayer) = CreateScenePlayer();
        scenePlayer.StartScene(animationPlayer, "cutscene");

        animationPlayer.EmitSignal(
            AnimationPlayer.SignalName.AnimationFinished,
            new StringName("cutscene"));

        scenePlayer.SceneActive.ShouldBeFalse();
    }

    [Fact]
    public void AnimationFinished_ForOtherAnimation_ShouldNotEndScene()
    {
        var (scenePlayer, animationPlayer) = CreateScenePlayer();
        scenePlayer.StartScene(animationPlayer, "cutscene");

        animationPlayer.EmitSignal(
            AnimationPlayer.SignalName.AnimationFinished,
            new StringName("other_animation"));

        scenePlayer.SceneActive.ShouldBeTrue();
    }

    private (CutscenePlayer ScenePlayer, AnimationPlayer AnimationPlayer) CreateScenePlayer()
    {
        var scenePlayer = new CutscenePlayer();
        var animationPlayer = CreateAnimationPlayer();

        _host.AddChild(animationPlayer);
        _host.AddChild(scenePlayer);
        _godot.GodotInstance.Iteration(1);

        return (scenePlayer, animationPlayer);
    }

    private static AnimationPlayer CreateAnimationPlayer()
    {
        var animationPlayer = new AnimationPlayer();
        var library = new AnimationLibrary();
        var animation = new Animation
        {
            Length = 0.25f
        };

        library.AddAnimation("cutscene", animation);
        animationPlayer.AddAnimationLibrary("", library);
        return animationPlayer;
    }
}
