using Godot;

namespace CrossedDimensions.Characters;

/// <summary>
/// Handles player character death, triggering the death fade animation and
/// transitioning to the death screen.
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
    private bool _playingDeathFade;
    private float _deathOverlayAlpha;

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
        if (!_playingDeathFade)
        {
            return;
        }

        if (DeathOverlay is null)
        {
            return;
        }

        // Fade runs at real-time (unscaled delta)
        float dt = (float)delta;
        _deathOverlayAlpha = Mathf.Min(1f, _deathOverlayAlpha + DeathFadeSpeed * dt);
        DeathOverlay.Color = DeathOverlay.Color with { A = _deathOverlayAlpha };

        if (_deathOverlayAlpha >= 1f)
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
        // Start death fade animation
        // The fade is naturally delayed by the timescale freeze from DamageEffectsManager
        if (DeathOverlay is not null)
        {
            _playingDeathFade = true;
            _deathOverlayAlpha = 0f;
            DeathOverlay.Visible = true;
            DeathOverlay.Color = DeathOverlay.Color with { A = 0f };
        }
        else
        {
            // No overlay, go to death screen immediately
            GoToDeathScreen();
        }
    }

    private void OnCloneCharacterDeath()
    {
        Character.Cloneable?.Original?.Cloneable?.Merge();
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
