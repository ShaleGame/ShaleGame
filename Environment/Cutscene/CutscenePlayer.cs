using System;
using System.Collections.Generic;
using System.Linq;
using CrossedDimensions.UI.UIDialogueBox;
using Godot;

namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// Plays a cutscene by driving a queued sequence of cutscene steps.
/// </summary>
public partial class CutscenePlayer : Node
{
    private AnimationPlayer _animationPlayer;
    private DialogueBox _dialogueBox;
    private DialoguePlayer _dialoguePlayer;
    private DialoguePlayer _subscribedDialoguePlayer;
    private DialogueListener _dialogueListener;
    private readonly Queue<CutsceneStep> _pendingSteps = new();
    private string _currentAnimationName = "";

    [Signal]
    public delegate void StartingSceneEventHandler();

    [Signal]
    public delegate void EndingSceneEventHandler();

    [Signal]
    public delegate void DialogueRequestedEventHandler();

    public bool SceneActive { get; set; }

    public DialogueReel RequestedDialogueReel { get; private set; }

    [Export]
    public CutsceneStep[] StepQueue { get; set; } = Array.Empty<CutsceneStep>();

    public double PlaybackSpeed { get; set; } = 1.0;

    [Export]
    public AnimationPlayer AnimationPlayer
    {
        get => _animationPlayer;
        set
        {
            if (ReferenceEquals(_animationPlayer, value))
            {
                return;
            }

            if (_animationPlayer is not null)
            {
                _animationPlayer.AnimationFinished -= OnAnimationFinished;
            }

            _animationPlayer = value;

            if (_animationPlayer is not null)
            {
                _animationPlayer.AnimationFinished += OnAnimationFinished;
            }
        }
    }

    [Export]
    public DialoguePlayer DialoguePlayer
    {
        get => _dialoguePlayer;
        set
        {
            if (ReferenceEquals(_dialoguePlayer, value))
            {
                return;
            }

            _dialoguePlayer = value;
            UpdateDialogueEndingSubscription();
        }
    }

    [Export]
    public DialogueBox DialogueBox
    {
        get => _dialogueBox;
        set
        {
            if (ReferenceEquals(_dialogueBox, value))
            {
                return;
            }

            _dialogueBox = value;

            if (_dialogueListener is not null
                && _dialogueListener.DialogueBox is null)
            {
                _dialogueListener.DialogueBox = value;
            }

            UpdateDialogueEndingSubscription();
        }
    }

    [Export]
    public DialogueListener DialogueListener
    {
        get => _dialogueListener;
        set
        {
            if (ReferenceEquals(_dialogueListener, value))
            {
                return;
            }

            if (_dialogueListener is not null)
            {
                _dialogueListener.CutscenePlayer = null;
            }

            _dialogueListener = value;

            if (_dialogueListener is not null)
            {
                if (_dialogueListener.DialogueBox is null)
                {
                    _dialogueListener.DialogueBox = _dialogueBox;
                }

                _dialogueListener.CutscenePlayer = this;
            }
        }
    }

    [Export]
    public string AnimationName { get; set; } = "";

    public override void _ExitTree()
    {
        if (_animationPlayer is not null)
        {
            _animationPlayer.AnimationFinished -= OnAnimationFinished;
        }

        if (_dialogueListener is not null)
        {
            _dialogueListener.CutscenePlayer = null;
        }

        if (_subscribedDialoguePlayer is not null)
        {
            _subscribedDialoguePlayer.Ending -= OnDialogueEnded;
        }
    }

    public void StartScene(AnimationPlayer animationPlayer = null, string animationName = "")
    {
        if (animationPlayer is not null)
        {
            AnimationPlayer = animationPlayer;
        }

        if (!string.IsNullOrEmpty(animationName))
        {
            AnimationName = animationName;
        }

        UpdateDialogueEndingSubscription();
        BuildPlaybackQueues();
        SceneActive = true;

        EmitSignal(SignalName.StartingScene);
        StartNextStep();
    }

    public void EndScene()
    {
        if (!SceneActive)
        {
            return;
        }

        SceneActive = false;
        _pendingSteps.Clear();
        _currentAnimationName = "";
        RequestedDialogueReel = null;

        if (AnimationPlayer?.IsPlaying() ?? false)
        {
            AnimationPlayer.Stop();
        }

        if (AnimationPlayer is not null)
        {
            AnimationPlayer.SpeedScale = 1.0f;
        }

        var dialoguePlayer = ResolveDialoguePlayer();
        if (dialoguePlayer is not null)
        {
            dialoguePlayer.EndDialogue(ResolveDialogueBox());
        }

        EmitSignal(SignalName.EndingScene);
    }

    private void OnAnimationFinished(StringName animationName)
    {
        if (!SceneActive)
        {
            return;
        }

        if (!string.IsNullOrEmpty(_currentAnimationName)
            && animationName != _currentAnimationName)
        {
            return;
        }

        StartNextStep();
    }

    private void OnDialogueEnded()
    {
        if (!SceneActive)
        {
            return;
        }

        StartNextStep();
    }

    private void BuildPlaybackQueues()
    {
        _pendingSteps.Clear();
        _currentAnimationName = "";

        if (StepQueue.Length > 0)
        {
            foreach (var step in StepQueue.Where(step => step is not null))
            {
                _pendingSteps.Enqueue(step);
            }
        }
        else if (!string.IsNullOrWhiteSpace(AnimationName))
        {
            _pendingSteps.Enqueue(new CutsceneStep
            {
                Kind = CutsceneStep.StepKind.Animation,
                AnimationName = AnimationName
            });
        }
    }

    private void StartNextStep()
    {
        _currentAnimationName = "";
        RequestedDialogueReel = null;

        while (_pendingSteps.Count > 0)
        {
            var step = _pendingSteps.Dequeue();
            if (step is null)
            {
                continue;
            }

            if (step.Kind == CutsceneStep.StepKind.Animation && TryStartAnimationStep(step))
            {
                return;
            }
            else if (step.Kind == CutsceneStep.StepKind.Dialogue && TryStartDialogueStep(step))
            {
                return;
            }
        }

        EndScene();
    }

    private bool TryStartAnimationStep(CutsceneStep step)
    {
        if (AnimationPlayer is null)
        {
            GD.PushWarning(
                "CutscenePlayer.TryStartAnimationStep: " +
                "animation step was queued without an AnimationPlayer.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(step.AnimationName))
        {
            GD.PushWarning(
                "CutscenePlayer.TryStartAnimationStep: " +
                "animation step was missing an animation name.");
            return false;
        }

        if (!AnimationPlayer.HasAnimation(step.AnimationName))
        {
            GD.PushWarning(
                $"CutscenePlayer.TryStartAnimationStep: animation " +
                $"'{step.AnimationName}' was not found.");
            return false;
        }

        AnimationPlayer.SpeedScale = (float)PlaybackSpeed;
        _currentAnimationName = step.AnimationName;
        AnimationPlayer.Play(step.AnimationName);
        return true;
    }

    private bool TryStartDialogueStep(CutsceneStep step)
    {
        if (step.DialogueReel is null)
        {
            GD.PushWarning(
                "CutscenePlayer.TryStartDialogueStep: " +
                "dialogue step was missing a DialogueReel.");
            return false;
        }

        var dialoguePlayer = ResolveDialoguePlayer();
        if (dialoguePlayer is null)
        {
            GD.PushWarning(
                "CutscenePlayer.TryStartDialogueStep: " +
                "dialogue step was queued without a DialoguePlayer.");
            return false;
        }

        RequestedDialogueReel = step.DialogueReel;
        var targetDialogueBox = ResolveDialogueBox();
        if (targetDialogueBox is not null)
        {
            if (_dialogueListener is not null)
            {
                EmitSignal(SignalName.DialogueRequested);
            }

            targetDialogueBox.OpenDialogue(step.DialogueReel);
            return true;
        }

        if (_dialogueListener is not null)
        {
            GD.PushWarning(
                "CutscenePlayer.TryStartDialogueStep: " +
                "DialogueListener was assigned without a DialogueBox.");
            return false;
        }

        return dialoguePlayer.StartDialogue(step.DialogueReel);
    }

    private DialoguePlayer ResolveDialoguePlayer()
    {
        if (_dialoguePlayer is not null)
        {
            return _dialoguePlayer;
        }

        return ResolveDialogueBox()?.GetNodeOrNull<DialoguePlayer>("DialoguePlayer");
    }

    private DialogueBox ResolveDialogueBox()
    {
        if (_dialogueListener?.DialogueBox is not null)
        {
            return _dialogueListener.DialogueBox;
        }

        return _dialogueBox;
    }

    private void UpdateDialogueEndingSubscription()
    {
        var dialoguePlayer = ResolveDialoguePlayer();
        if (ReferenceEquals(_subscribedDialoguePlayer, dialoguePlayer))
        {
            return;
        }

        if (_subscribedDialoguePlayer is not null)
        {
            _subscribedDialoguePlayer.Ending -= OnDialogueEnded;
        }

        _subscribedDialoguePlayer = dialoguePlayer;

        if (_subscribedDialoguePlayer is not null)
        {
            _subscribedDialoguePlayer.Ending += OnDialogueEnded;
        }
    }
}
