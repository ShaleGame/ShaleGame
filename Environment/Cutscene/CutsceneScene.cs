using System;
using CrossedDimensions.UI.UIDialogueBox;
using Godot;

namespace CrossedDimensions.Environment.Cutscene;

[GlobalClass]
public partial class CutsceneScene : Node2D
{
    private CutscenePlayer _scenePlayer;

    [Signal]
    public delegate void StartingSceneEventHandler();

    [Signal]
    public delegate void EndingSceneEventHandler();

    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    [Export]
    public string StartAnimation { get; set; } = "";

    [Export]
    public DialogueListener DialogueListener { get; set; }

    [Export]
    public DialogueBox DialogueBox { get; set; }

    [Export]
    public CutsceneStep[] StepQueue { get; set; } = Array.Empty<CutsceneStep>();

    public bool IsStarted { get; set; }

    public bool IsFinished { get; set; }

    public override void _Ready()
    {
        EnsureScenePlayer();
    }

    public override void _ExitTree()
    {
        if (_scenePlayer is null)
        {
            return;
        }

        _scenePlayer.StartingScene -= OnScenePlaybackStarted;
        _scenePlayer.EndingScene -= OnScenePlaybackEnded;
    }

    public void StartScene(double playbackSpeed = 1.0)
    {
        EnsureScenePlayer();

        _scenePlayer.AnimationPlayer = AnimationPlayer;
        _scenePlayer.AnimationName = StartAnimation;
        _scenePlayer.DialogueBox = DialogueBox;
        _scenePlayer.DialogueListener = DialogueListener;
        _scenePlayer.StepQueue = StepQueue;
        _scenePlayer.PlaybackSpeed = playbackSpeed;

        IsStarted = true;
        IsFinished = false;
        _scenePlayer.StartScene();
    }

    public void EndScene()
    {
        if (_scenePlayer is null)
        {
            return;
        }

        _scenePlayer.EndScene();
    }

    private void EnsureScenePlayer()
    {
        if (_scenePlayer is not null)
        {
            return;
        }

        _scenePlayer = new CutscenePlayer();
        _scenePlayer.StartingScene += OnScenePlaybackStarted;
        _scenePlayer.EndingScene += OnScenePlaybackEnded;
        AddChild(_scenePlayer);
    }

    private void OnScenePlaybackStarted()
    {
        EmitSignal(SignalName.StartingScene);
    }

    private void OnScenePlaybackEnded()
    {
        IsFinished = true;
        EmitSignal(SignalName.EndingScene);
    }
}
