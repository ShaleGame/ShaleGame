using System;
using CrossedDimensions.Characters;
using CrossedDimensions.Environment.Cutscene;
using CrossedDimensions.Environment.Cutscene.Interactables;
using CrossedDimensions.UI.UIDialogueBox;
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
        using var animationName = new StringName("cutscene");

        animationPlayer.EmitSignal(
            AnimationPlayer.SignalName.AnimationFinished,
            animationName);

        scenePlayer.SceneActive.ShouldBeFalse();
    }

    [Fact]
    public void AnimationFinished_ForOtherAnimation_ShouldNotEndScene()
    {
        var (scenePlayer, animationPlayer) = CreateScenePlayer();
        scenePlayer.StartScene(animationPlayer, "cutscene");
        using var animationName = new StringName("other_animation");

        animationPlayer.EmitSignal(
            AnimationPlayer.SignalName.AnimationFinished,
            animationName);

        scenePlayer.SceneActive.ShouldBeTrue();
    }

    [Fact]
    public void AnimationFinished_WithSequentialStepQueue_ShouldPlayNextAnimation()
    {
        var (scenePlayer, animationPlayer) = CreateScenePlayer();
        scenePlayer.StepQueue = new[]
        {
            CreateAnimationStep("cutscene"),
            CreateAnimationStep("cutscene_second")
        };

        scenePlayer.StartScene(animationPlayer);
        _godot.GodotInstance.Iteration(1);
        animationPlayer.CurrentAnimation.ToString().ShouldBe("cutscene");

        using var animationName = new StringName("cutscene");
        animationPlayer.EmitSignal(
            AnimationPlayer.SignalName.AnimationFinished,
            animationName);

        _godot.GodotInstance.Iteration(1);

        scenePlayer.SceneActive.ShouldBeTrue();
        animationPlayer.CurrentAnimation.ToString().ShouldBe("cutscene_second");
    }

    [Fact]
    public void AnimationFinished_ShouldStartLaterDialogueStepInsteadOfEndingScene()
    {
        var (scenePlayer, animationPlayer) = CreateScenePlayer();
        var dialoguePlayer = new DialoguePlayer();

        _host.AddChild(dialoguePlayer);
        _godot.GodotInstance.Iteration(1);

        scenePlayer.DialoguePlayer = dialoguePlayer;
        scenePlayer.StepQueue = new[]
        {
            CreateAnimationStep("cutscene"),
            CreateDialogueStep("First line.")
        };

        scenePlayer.StartScene(animationPlayer, "cutscene");
        _godot.GodotInstance.Iteration(1);
        dialoguePlayer.CurrentReel.ShouldBeNull();
        using var animationName = new StringName("cutscene");

        animationPlayer.EmitSignal(
            AnimationPlayer.SignalName.AnimationFinished,
            animationName);

        dialoguePlayer.CurrentReel.ShouldNotBeNull();
        scenePlayer.SceneActive.ShouldBeTrue();

        dialoguePlayer.EndDialogue();

        scenePlayer.SceneActive.ShouldBeFalse();
    }

    [Fact]
    public void DialogueEnding_ShouldAdvanceToNextStepInSequence()
    {
        var (scenePlayer, animationPlayer) = CreateScenePlayer();
        var dialoguePlayer = new DialoguePlayer();

        _host.AddChild(dialoguePlayer);
        _godot.GodotInstance.Iteration(1);

        var firstReel = CreateDialogueReel("First line.");
        var secondReel = CreateDialogueReel("Second line.");
        scenePlayer.DialoguePlayer = dialoguePlayer;
        scenePlayer.StepQueue = new[]
        {
            CreateDialogueStep(firstReel),
            CreateAnimationStep("cutscene"),
            CreateDialogueStep(secondReel)
        };

        scenePlayer.StartScene(animationPlayer);
        dialoguePlayer.CurrentReel.ShouldBe(firstReel);
        animationPlayer.IsPlaying().ShouldBeFalse();

        dialoguePlayer.EndDialogue();

        animationPlayer.IsPlaying().ShouldBeTrue();
        animationPlayer.CurrentAnimation.ToString().ShouldBe("cutscene");

        using var animationName = new StringName("cutscene");
        animationPlayer.EmitSignal(
            AnimationPlayer.SignalName.AnimationFinished,
            animationName);

        dialoguePlayer.CurrentReel.ShouldBe(secondReel);
        scenePlayer.SceneActive.ShouldBeTrue();
    }

    [Fact]
    public void DialogueEnding_FromDialogueBox_ShouldAdvanceToNextStep()
    {
        var (scenePlayer, animationPlayer) = CreateScenePlayer();
        var dialogueBox = CreateDialogueBox();
        var dialogueListener = new DialogueListener();

        _host.AddChild(dialogueListener);
        _godot.GodotInstance.Iteration(1);

        scenePlayer.DialogueBox = dialogueBox;
        scenePlayer.DialogueListener = dialogueListener;
        scenePlayer.StepQueue = new[]
        {
            CreateDialogueStep("First line."),
            CreateAnimationStep("cutscene")
        };

        scenePlayer.StartScene(animationPlayer);

        dialogueBox.Visible.ShouldBeTrue();
        animationPlayer.IsPlaying().ShouldBeFalse();

        dialogueBox.GetNode<DialoguePlayer>("DialoguePlayer").EndDialogue(dialogueBox);
        _godot.GodotInstance
            .IterateUntil(() => animationPlayer.IsPlaying(), 10)
            .ShouldBeTrue();

        animationPlayer.IsPlaying().ShouldBeTrue();
        animationPlayer.CurrentAnimation.ToString().ShouldBe("cutscene");
        scenePlayer.SceneActive.ShouldBeTrue();
    }

    [Fact]
    public void DialogueRequested_WithDialogueListener_ShouldOpenAssignedDialogueBox()
    {
        var (scenePlayer, animationPlayer) = CreateScenePlayer();
        var dialogueListener = new DialogueListener();
        var dialogueBox = CreateDialogueBox();

        _host.AddChild(dialogueListener);
        _godot.GodotInstance.Iteration(1);

        scenePlayer.DialogueBox = dialogueBox;
        scenePlayer.DialogueListener = dialogueListener;
        scenePlayer.StepQueue = new[]
        {
            CreateDialogueStep("Scene line.")
        };

        scenePlayer.StartScene(animationPlayer);

        dialogueListener.DialogueReel.ShouldNotBeNull();
        dialogueListener.DialogueReel.Frames[0].Text.ShouldBe("Scene line.");
        dialogueBox.Visible.ShouldBeTrue();

        dialogueBox.GetNode<DialoguePlayer>("DialoguePlayer").EndDialogue(dialogueBox);
    }

    [Fact]
    public void DialogueListener_InSavePointScene_ShouldOpenPlayerHudDialogueBox()
    {
        var player = ResourceLoader.Load<PackedScene>("res://Characters/Character.tscn")
            .Instantiate<Character>();
        var savePoint = ResourceLoader.Load<PackedScene>("res://Saves/SavePoint.tscn")
            .Instantiate<Node2D>();

        _host.AddChild(player);
        _host.AddChild(savePoint);
        _godot.GodotInstance.Iteration(1);

        var interactable = savePoint.GetNode<Interactable>("Interactable");
        var dialogueListener = savePoint.GetNode<DialogueListener>("DialogueListener");
        var playerHud = player.GetNode<Node>("%PlayerHud");
        var dialogueBox = playerHud.GetNode<DialogueBox>("DialogueBox");

        dialogueListener.Interactable.ShouldBeSameAs(interactable);

        interactable.EmitSignal(Interactable.SignalName.Interacted);
        _godot.GodotInstance.Iteration(1);

        dialogueBox.Visible.ShouldBeTrue();

        dialogueBox.GetNode<DialoguePlayer>("DialoguePlayer").EndDialogue(dialogueBox);
    }

    [Fact]
    public void DialogueListener_InSavePointScene_ShouldUseOriginalPlayerHudWhenCloneExists()
    {
        var player = ResourceLoader.Load<PackedScene>("res://Characters/Character.tscn")
            .Instantiate<Character>();
        var clone = ResourceLoader.Load<PackedScene>("res://Characters/CloneCharacter.tscn")
            .Instantiate<Character>();
        var savePoint = ResourceLoader.Load<PackedScene>("res://Saves/SavePoint.tscn")
            .Instantiate<Node2D>();

        clone.Cloneable.Original = player;
        player.Cloneable.Clone = clone;

        _host.AddChild(player);
        _host.AddChild(clone);
        _host.AddChild(savePoint);
        _godot.GodotInstance.Iteration(1);

        var interactable = savePoint.GetNode<Interactable>("Interactable");
        var playerHud = player.GetNode<Node>("%PlayerHud");
        var dialogueBox = playerHud.GetNode<DialogueBox>("DialogueBox");

        interactable.EmitSignal(Interactable.SignalName.Interacted);
        _godot.GodotInstance.Iteration(1);

        dialogueBox.Visible.ShouldBeTrue();

        dialogueBox.GetNode<DialoguePlayer>("DialoguePlayer").EndDialogue(dialogueBox);
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
        using var library = new AnimationLibrary();
        using var animation = new Animation
        {
            Length = 0.25f
        };
        using var secondAnimation = new Animation
        {
            Length = 0.25f
        };

        library.AddAnimation("cutscene", animation);
        library.AddAnimation("cutscene_second", secondAnimation);
        animationPlayer.AddAnimationLibrary("", library);
        return animationPlayer;
    }

    private DialogueBox CreateDialogueBox()
    {
        var packedScene = ResourceLoader.Load<PackedScene>(
            "res://UI/UIDialogueBox/DialogueBox.tscn");
        var dialogueBox = packedScene.Instantiate<DialogueBox>();

        _host.AddChild(dialogueBox);
        _godot.GodotInstance.Iteration(1);

        return dialogueBox;
    }

    private static DialogueReel CreateDialogueReel(string text)
    {
        return new DialogueReel
        {
            Frames = new[]
            {
                new DialogueFrame
                {
                    Speaker = "Tester",
                    Text = text,
                    Portrait = Array.Empty<Texture2D>(),
                    PortraitPosition = Array.Empty<Vector2>()
                }
            }
        };
    }

    private static CutsceneStep CreateAnimationStep(string animationName)
    {
        return new CutsceneStep
        {
            Kind = CutsceneStep.StepKind.Animation,
            AnimationName = animationName
        };
    }

    private static CutsceneStep CreateDialogueStep(string text)
    {
        return CreateDialogueStep(CreateDialogueReel(text));
    }

    private static CutsceneStep CreateDialogueStep(DialogueReel dialogueReel)
    {
        return new CutsceneStep
        {
            Kind = CutsceneStep.StepKind.Dialogue,
            DialogueReel = dialogueReel
        };
    }
}
