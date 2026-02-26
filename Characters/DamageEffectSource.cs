using Godot;
using CrossedDimensions.Components;
using CrossedDimensions.UI;

namespace CrossedDimensions.Characters;

/// <summary>
/// Component that triggers damage visual feedback effects when the entity takes damage.
/// Handles local effects (sprite flash) and delegates global effects (screen shake,
/// overlay, timescale) to the DamageEffectsManager autoload.
/// </summary>
[GlobalClass]
public partial class DamageEffectSource : Node
{
    [Export]
    public HealthComponent Health { get; set; }

    [Export]
    public Camera2D Camera { get; set; }

    [Export]
    public Node2D SpriteAnchor { get; set; }

    [Export]
    public bool TriggerGlobalEffects { get; set; } = true;

    [ExportCategory("Feedback")]
    [Export]
    public float DamageToIntensity { get; set; } = 1.5f;

    [Export]
    public float MinimumIntensity { get; set; } = 0.15f;

    [Export]
    public float SpriteHighlightGain { get; set; } = 0.6f;

    private DamageEffectsManager _effectsManager;
    private Color _spriteBaseModulate = new Color(1, 1, 1, 1);
    private float _spriteFlashRemaining;
    private float _spriteFlashDuration = 0.25f;

    public override void _Ready()
    {
        if (Health is not null)
        {
            Health.HealthChanged += OnHealthChanged;
        }

        if (SpriteAnchor is not null)
        {
            _spriteBaseModulate = SpriteAnchor.Modulate;
        }

        // Get reference to autoload
        _effectsManager = GetNode<DamageEffectsManager>("/root/DamageEffectsManager");
    }

    public override void _Process(double delta)
    {
        // Update sprite flash
        if (SpriteAnchor is not null && _spriteFlashRemaining > 0f)
        {
            float dt = (float)delta;
            _spriteFlashRemaining = Mathf.Max(0f, _spriteFlashRemaining - dt);

            float progress = Mathf.Clamp(_spriteFlashRemaining / _spriteFlashDuration, 0f, 1f);
            float boost = 1f + progress * SpriteHighlightGain;
            SpriteAnchor.Modulate = _spriteBaseModulate * boost;

            if (_spriteFlashRemaining <= 0f)
            {
                SpriteAnchor.Modulate = _spriteBaseModulate;
            }
        }
    }

    private void OnHealthChanged(int oldHealth)
    {
        if (Health is null)
        {
            return;
        }

        int newHealth = Health.CurrentHealth;
        int damage = oldHealth - newHealth;

        if (damage <= 0)
        {
            return;
        }

        float intensity;

        if (newHealth <= 0)
        {
            // Death - max intensity
            intensity = 1f;
        }
        else
        {
            // Regular damage - calculate intensity
            intensity = Mathf.Clamp(
                damage / (Health.MaxHealth * DamageToIntensity),
                MinimumIntensity,
                1f);
        }

        // Trigger local sprite flash
        if (SpriteAnchor is not null)
        {
            _spriteFlashRemaining = _spriteFlashDuration * intensity;
        }

        // Trigger global effects via autoload
        if (TriggerGlobalEffects && _effectsManager is not null)
        {
            if (newHealth <= 0)
            {
                _effectsManager.TriggerDeathFeedback(Camera);
            }
            else
            {
                _effectsManager.TriggerDamage(
                    damage,
                    Health.MaxHealth,
                    Camera,
                    DamageToIntensity,
                    MinimumIntensity);
            }
        }
    }
}
