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
    private Character _ownerCharacter;
    private Color _spriteBaseModulate = new Color(1, 1, 1, 1);
    private float _spriteFlashRemaining;
    private float _spriteFlashDuration = 0.25f;
    private float _lastIntensity = 0f;

    public override void _Ready()
    {
        _ownerCharacter = GetParent<Character>();

        if (Health is not null)
        {
            Health.HealthChanged += OnHealthChanged;
        }

        if (SpriteAnchor is not null)
        {
            _spriteBaseModulate = SpriteAnchor.Modulate;
        }

        // Get reference to autoload manager (may be null in tests)
        if (GetTree().Root.HasNode("/root/DamageEffectsManager"))
        {
            _effectsManager = GetNode<DamageEffectsManager>("/root/DamageEffectsManager");
        }

        // If this character is a clone, prefer using the original's camera for shake
        if (_ownerCharacter?.Cloneable is not null && _ownerCharacter.Cloneable.IsClone)
        {
            var original = _ownerCharacter.Cloneable.Original;
            if (original is not null)
            {
                var originalCamera = original.GetNodeOrNull<Camera2D>("Camera2D");
                if (originalCamera is not null)
                {
                    Camera = originalCamera;
                }
            }
        }

        // If this is the original, hook split events so we can update the clone's source
        if (_ownerCharacter?.Cloneable is not null && !_ownerCharacter.Cloneable.IsClone)
        {
            _ownerCharacter.Cloneable.CharacterSplitPost += OnCharacterSplitPost;
        }
    }

    private void OnCharacterSplitPost(Character original, Character clone)
    {
        if (original != _ownerCharacter)
            return;

        // The clone scene has a DamageEffectSource node named "DamageEffectSource" as a child.
        var cloneSource = clone.GetNodeOrNull<DamageEffectSource>("DamageEffectSource");
        if (cloneSource is not null)
        {
            var originalCamera = original.GetNodeOrNull<Camera2D>("Camera2D");
            if (originalCamera is not null)
            {
                cloneSource.Camera = originalCamera;
            }
        }
    }

    public override void _Process(double delta)
    {
        // If the manager has an active freeze timer, base sprite modulation on it
        if (SpriteAnchor is not null && EffectsManagerIsActive(_effectsManager))
        {
            var timer = _effectsManager.FreezeTimer;
            if (timer is not null && timer.WaitTime > 0f && !timer.IsStopped())
            {
                float progress = Mathf.Clamp((float)timer.TimeLeft / (float)timer.WaitTime, 0f, 1f);
                float boost = 1f + _lastIntensity * progress * SpriteHighlightGain;
                SpriteAnchor.Modulate = _spriteBaseModulate * boost;
                return;
            }
        }

        // Fallback: local timer-based flash
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

    private static bool EffectsManagerIsActive(DamageEffectsManager mgr)
    {
        return mgr is not null && mgr.FreezeTimer is not null;
    }

    private void OnHealthChanged(int oldHealth)
    {
        if (Health is null)
            return;

        int newHealth = Health.CurrentHealth;
        int damage = oldHealth - newHealth;

        if (damage <= 0)
            return;

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

        _lastIntensity = intensity;

        // Trigger local sprite flash (fallback if manager not present)
        if (SpriteAnchor is not null)
        {
            _spriteFlashRemaining = _spriteFlashDuration * intensity;
        }

        // Trigger global effects via autoload
        if (TriggerGlobalEffects && _effectsManager is not null)
        {
            // Camera is already set to original's camera for clones in _Ready, or via split hook
            Camera2D effectiveCamera = Camera;

            if (newHealth <= 0)
            {
                _effectsManager.TriggerDeathFeedback(effectiveCamera);
            }
            else
            {
                _effectsManager.TriggerDamage(
                    damage,
                    Health.MaxHealth,
                    effectiveCamera,
                    DamageToIntensity,
                    MinimumIntensity);
            }
        }
    }
}
