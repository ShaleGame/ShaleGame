using Godot;

namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// Plays a cutscene by driving a configured animation.
/// </summary>
public partial class CutscenePlayer : Node, ICutsceneHandler
{
    private AnimationPlayer _animationPlayer;

    [Signal]
    public delegate void StartingSceneEventHandler();

    [Signal]
    public delegate void EndingSceneEventHandler();

    public bool SceneActive { get; set; }

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
    public string AnimationName { get; set; } = "";

    public override void _ExitTree()
    {
        if (_animationPlayer is not null)
        {
            _animationPlayer.AnimationFinished -= OnAnimationFinished;
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

        SceneActive = true;

        if (AnimationPlayer is not null && !string.IsNullOrEmpty(AnimationName))
        {
            AnimationPlayer.Play(AnimationName);
        }

        EmitSignal(SignalName.StartingScene);
    }

    public void EndScene()
    {
        if (AnimationPlayer?.IsPlaying() ?? false)
        {
            AnimationPlayer.Stop();
        }

        SceneActive = false;
        EmitSignal(SignalName.EndingScene);
    }

    private void OnAnimationFinished(StringName animationName)
    {
        if (!SceneActive)
        {
            return;
        }

        if (!string.IsNullOrEmpty(AnimationName) && animationName != AnimationName)
        {
            return;
        }

        EndScene();
    }
}
