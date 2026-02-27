using System;
using Godot;

namespace CrossedDimensions.Characters;

/// <summary>
/// Handles player character death, triggering the death fade animation and
/// transitioning to the death screen. Uses an unscaled, manual cubic-in fade
/// to ensure the animation runs in real time regardless of Engine.TimeScale.
/// </summary>
public partial class PlayerDeathHandler : Node
{
    [Export]
    public Character Character { get; set; }

    [Export]
    public ColorRect DeathOverlay { get; set; }

    [Export]
    public float DeathFadeSpeed { get; set; } = 1.5f;

    private bool _transitionScheduled;

    // Manual unscaled fade state
    private double _deathStartTime;
    private double _deathDuration;
    private bool _playingDeathFade;

    public override void _Ready()
    {
        Character.Health.HealthChanged += OnHealthChanged;

        if (DeathOverlay is not null)
        {
            DeathOverlay.Color = DeathOverlay.Color with { A = 0f };
            DeathOverlay.Visible = false;
        }
    }

    public override void _Process(double delta)
    {
        if (!_playingDeathFade || DeathOverlay is null)
        {
            return;
        }

        double now = Time.GetTicksMsec() / 1000.0;
        double progress = 0.0;

        if (_deathDuration > 0.0)
        {
            progress = (now - _deathStartTime) / _deathDuration;
        }

        progress = Math.Clamp(progress, 0.0, 1.0);

        // Cubic-in easing
        double eased = progress * progress * progress;

        DeathOverlay.Color = DeathOverlay.Color with { A = (float)eased };

        if (progress >= 1.0)
        {
            _playingDeathFade = false;
            GoToDeathScreen();
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
        if (DeathOverlay is not null)
        {
            DeathOverlay.Visible = true;
            DeathOverlay.Color = DeathOverlay.Color with { A = 0f };

            double duration = Math.Max(0.001, 1.0 / DeathFadeSpeed);

            _deathStartTime = Time.GetTicksMsec() / 1000.0;
            _deathDuration = duration;
            _playingDeathFade = true;

            return;
        }

        GoToDeathScreen();
    }

    private void OnCloneCharacterDeath()
    {
        Character.Cloneable?.Original?.Cloneable?.Merge();
    }

    private void GoToDeathScreen()
    {
        if (_transitionScheduled)
            return;

        _transitionScheduled = true;
        const string ScenePath = "res://Scenes/DeathScreen.tscn";
        GetTree().CallDeferred("change_scene_to_file", ScenePath);
    }
}
