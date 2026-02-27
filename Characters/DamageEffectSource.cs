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
    /// <summary>
    /// Defines the method used to display damage flash effects on sprites.
    /// </summary>
    public enum FlashMode
    {
        Modulate,
        Shader
    }

    /// <summary>
    /// The health component to monitor for damage events.
    /// </summary>
    [Export]
    public HealthComponent Health { get; set; }

    /// <summary>
    /// The camera to apply shake effects to when damage occurs.
    /// </summary>
    [Export]
    public Camera2D Camera { get; set; }

    /// <summary>
    /// The sprite node to apply flash effects to.
    /// </summary>
    [Export]
    public Node2D SpriteAnchor { get; set; }

    /// <summary>
    /// Whether to trigger global effects (screen shake, overlay, timescale) through DamageEffectsManager.
    /// If false, only local sprite flash effects are applied.
    /// </summary>
    [Export]
    public bool TriggerGlobalEffects { get; set; } = true;

    /// <summary>
    /// Reference to the character entity. Used to determine if this is a
    /// clone and should use the original's camera.
    /// </summary>
    [Export]
    public Character Character { get; set; }

    [ExportCategory("Feedback")]
    /// <summary>
    /// Multiplier for converting damage amount to effect intensity.
    /// Higher values make damage feel more impactful.
    /// </summary>
    [Export]
    public float DamageToIntensity { get; set; } = 1.5f;

    /// <summary>
    /// The minimum intensity for damage effects, ensuring even small damage is
    /// visible.
    /// </summary>
    [Export]
    public float MinimumIntensity { get; set; } = 0.15f;

    /// <summary>
    /// The amount of brightness gain applied to the sprite during flash in
    /// Modulate mode.
    /// </summary>
    [Export]
    public float SpriteHighlightGain { get; set; } = 0.6f;

    /// <summary>
    /// The method used to apply sprite flash effects (Modulate or Shader).
    /// </summary>
    [Export]
    public FlashMode SpriteFlashMode { get; set; } = FlashMode.Modulate;

    private DamageEffectsManager _effectsManager;
    private Color _spriteBaseModulate = new Color(1, 1, 1, 1);
    private float _spriteFlashRemaining;
    private float _spriteFlashDuration = 0.5f;
    private float _lastIntensity = 0f;

    /// <summary>
    /// Initializes the component by subscribing to health changes, setting up
    /// sprite flash effects, getting the DamageEffectsManager reference, and
    /// determining the appropriate camera for clones.
    /// </summary>
    public override void _Ready()
    {
        if (Health is not null)
        {
            Health.HealthChanged += OnHealthChanged;
        }

        if (SpriteAnchor is not null)
        {
            _spriteBaseModulate = SpriteAnchor.Modulate;

            if (SpriteFlashMode == FlashMode.Shader)
            {
                AddShaderToSprite();
            }
        }

        if (GetTree().Root.HasNode("/root/DamageEffectsManager"))
        {
            _effectsManager = GetNode<DamageEffectsManager>("/root/DamageEffectsManager");
        }

        // if this character is a clone, prefer using the original's camera for shake
        if (Character?.Cloneable is not null && Character.Cloneable.IsClone)
        {
            var original = Character.Cloneable.Original;
            if (original is not null)
            {
                var originalCamera = original.GetNodeOrNull<Camera2D>("Camera2D");
                if (originalCamera is not null)
                {
                    Camera = originalCamera;
                }
            }
        }
    }

    /// <summary>
    /// Applies the damage flash shader to the sprite anchor.
    /// </summary>
    private void AddShaderToSprite()
    {
        var shader = GD.Load<Shader>("res://Shaders/DamageFlash.gdshader");
        SpriteAnchor.Material = new ShaderMaterial { Shader = shader };
    }

    public override void _Process(double delta)
    {
        if (TriggerGlobalEffects)
        {
            if (SpriteAnchor is not null && IsEffectsManagerActivate(_effectsManager))
            {
                var timer = _effectsManager.FreezeTimer;
                if (timer is not null && timer.WaitTime > 0f && !timer.IsStopped())
                {
                    float progress = (float)timer.TimeLeft / (float)timer.WaitTime;
                    progress = Mathf.Clamp(progress, 0f, 1f);
                    float intensity = _lastIntensity * progress;
                    ApplySpriteFlash(intensity);
                    return;
                }
            }
        }
        else
        {
            if (SpriteAnchor is not null && _spriteFlashRemaining > 0f)
            {
                _spriteFlashRemaining -= (float)delta;
                ApplySpriteFlash(1.0f);
            }
            else
            {
                ResetSpriteFlash();
            }
        }
    }

    /// <summary>
    /// Applies sprite flash effect with the specified intensity using the
    /// configured flash mode.
    /// </summary>
    /// <param name="intensity">The intensity of the flash effect (0-1).</param>
    private void ApplySpriteFlash(float intensity)
    {
        if (SpriteAnchor is null)
        {
            return;
        }

        if (SpriteFlashMode == FlashMode.Modulate)
        {
            ApplyModulateFlash(intensity);
        }
        else if (SpriteFlashMode == FlashMode.Shader)
        {
            ApplyShaderFlash(intensity);
        }
    }

    /// <summary>
    /// Resets the sprite flash effect to its normal appearance.
    /// </summary>
    private void ResetSpriteFlash()
    {
        if (SpriteAnchor is null)
        {
            return;
        }

        if (SpriteFlashMode == FlashMode.Modulate)
        {
            SpriteAnchor.Modulate = _spriteBaseModulate;
        }
        else if (SpriteFlashMode == FlashMode.Shader)
        {
            SpriteAnchor.SetInstanceShaderParameter("flash_intensity", 0.0f);
        }
    }

    /// <summary>
    /// Applies a flash effect by modulating the sprite's color to make it brighter.
    /// </summary>
    /// <param name="intensity">The intensity of the flash effect (0-1).</param>
    private void ApplyModulateFlash(float intensity)
    {
        float boost = 1f + intensity * SpriteHighlightGain;
        SpriteAnchor.Modulate = _spriteBaseModulate * boost;
    }

    /// <summary>
    /// Applies a flash effect using a shader parameter.
    /// </summary>
    /// <param name="intensity">The intensity of the flash effect (0-1).</param>
    private void ApplyShaderFlash(float intensity)
    {
        SpriteAnchor.SetInstanceShaderParameter("flash_intensity", intensity);
    }

    /// <summary>
    /// Checks if the DamageEffectsManager is properly initialized and active.
    /// </summary>
    /// <param name="mgr">The manager to check.</param>
    /// <returns>True if the manager and its freeze timer are available.</returns>
    private static bool IsEffectsManagerActivate(DamageEffectsManager mgr)
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
        {
            return;
        }

        float intensity;

        if (newHealth <= 0)
        {
            intensity = 1f;
        }
        else
        {
            // calculate intensity based on damage
            intensity = Mathf.Clamp(
                damage / (Health.MaxHealth * DamageToIntensity),
                MinimumIntensity,
                1f);
        }

        _lastIntensity = intensity;

        // if using local sprite flash, set the flash timer based on intensity
        if (!TriggerGlobalEffects && SpriteAnchor is not null)
        {
            _spriteFlashRemaining = _spriteFlashDuration * intensity;
        }

        // trigger global effects through the manager
        if (TriggerGlobalEffects && _effectsManager is not null)
        {
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
