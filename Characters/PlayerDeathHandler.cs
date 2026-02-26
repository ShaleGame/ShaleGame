using Godot;
using CrossedDimensions.UI;

namespace CrossedDimensions.Characters;

public partial class PlayerDeathHandler : Node
{
    [Export]
    public Character Character { get; set; }

    /// <summary>
    /// Optional reference to the DamageFeedbackController. When provided, the
    /// death screen transition is deferred until <see cref="DamageFeedbackController.DeathFadeCompleted"/>
    /// fires or <see cref="MaxTransitionDelay"/> elapses, whichever comes first.
    /// </summary>
    [Export]
    public DamageFeedbackController DamageFeedback { get; set; }

    /// <summary>
    /// Maximum real-time seconds to wait for the death fade before forcing the
    /// scene transition. Only used when <see cref="DamageFeedback"/> is set.
    /// </summary>
    [Export]
    public float MaxTransitionDelay { get; set; } = 3f;

    private bool _transitionScheduled;
    private Timer _fallbackTimer;

    public override void _Ready()
    {
        Character.Health.HealthChanged += OnHealthChanged;

        if (DamageFeedback is not null)
        {
            DamageFeedback.DeathFadeCompleted += OnDeathFadeCompleted;

            _fallbackTimer = new Timer();
            _fallbackTimer.WaitTime = MaxTransitionDelay;
            _fallbackTimer.OneShot = true;
            _fallbackTimer.ProcessCallback = Timer.TimerProcessCallback.Idle;
            _fallbackTimer.Timeout += OnFallbackTimeout;
            AddChild(_fallbackTimer);
        }
    }

    private void OnHealthChanged(int oldHealth)
    {
        if (Character.Health.IsAlive)
        {
            return;
        }

        if (Character.Cloneable?.IsClone ?? false)
        {
            OnCloneCharacterDeath();
        }
        else
        {
            OnOriginalCharacterDeath();
        }
    }

    private void OnOriginalCharacterDeath()
    {
        if (DamageFeedback is not null)
        {
            // Wait for the feedback controller to finish the fade, with fallback.
            _fallbackTimer?.Start();
            return;
        }

        GoToDeathScreen();
    }

    private void OnCloneCharacterDeath()
    {
        Character.Cloneable?.Original?.Cloneable?.Merge();
    }

    private void OnDeathFadeCompleted()
    {
        if (_transitionScheduled)
        {
            return;
        }

        _fallbackTimer?.Stop();
        GoToDeathScreen();
    }

    private void OnFallbackTimeout()
    {
        GoToDeathScreen();
    }

    private void GoToDeathScreen()
    {
        if (_transitionScheduled)
        {
            return;
        }

        _transitionScheduled = true;
        const string ScenePath = "res://Scenes/DeathScreen.tscn";
        GetTree().CallDeferred("change_scene_to_file", ScenePath);
    }
}
