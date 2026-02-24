using Godot;
using CrossedDimensions.Components;

namespace CrossedDimensions.UI;

[GlobalClass]
public partial class DamageFeedbackController : Node
{
    [Export]
    public HealthComponent HealthComponent { get; set; }

    [Export]
    public Camera2D Camera { get; set; }

    [Export]
    public ColorRect Overlay { get; set; }

    [Export]
    public Node2D SpriteAnchor { get; set; }

    [ExportCategory("Feedback")]
    [Export]
    public float OverlayMaxAlpha { get; set; } = 0.75f;

    [Export]
    public float OverlayFadeSpeed { get; set; } = 2.5f;

    [Export]
    public float MinTimeScale { get; set; } = 0.6f;

    [Export]
    public float TimeScaleHoldDuration { get; set; } = 0.25f;

    [Export]
    public float ShakeDuration { get; set; } = 0.25f;

    [Export]
    public float ShakeStrength { get; set; } = 12.0f;

    [Export]
    public float DamageToIntensity { get; set; } = 1.5f;

    [Export]
    public float MinimumIntensity { get; set; } = 0.15f;

    [Export]
    public float SpriteHighlightGain { get; set; } = 0.6f;

    [Export]
    public float SpriteHighlightRecovery { get; set; } = 0.3f;

    private float _overlayAlpha;
    private float _shakeRemaining;
    private float _shakeStrength;
    private float _spriteHighlightTimer;
    private float _timeScaleHoldRemaining;
    private float _targetTimeScale = 1f;
    private Vector2 _cameraBasePosition = Vector2.Zero;
    private Color _overlayBaseColor = new Color(0, 0, 0, 1);
    private Color _spriteBaseModulate = new Color(1, 1, 1, 1);
    private readonly RandomNumberGenerator _rng = new();

    public override void _Ready()
    {
        _rng.Randomize();

        if (HealthComponent is not null)
        {
            HealthComponent.HealthChanged += OnHealthChanged;
        }

        if (Camera is not null)
        {
            _cameraBasePosition = Camera.Position;
        }

        if (Overlay is not null)
        {
            _overlayBaseColor = Overlay.Color;
            Overlay.Color = _overlayBaseColor with { A = 0f };
            Overlay.Visible = false;
        }

        if (SpriteAnchor is not null)
        {
            _spriteBaseModulate = SpriteAnchor.Modulate;
        }
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        if (Overlay is not null && _overlayAlpha > 0f)
        {
            _overlayAlpha = Mathf.Max(0f, _overlayAlpha - OverlayFadeSpeed * dt);
            Overlay.Color = _overlayBaseColor with { A = _overlayAlpha };

            if (_overlayAlpha <= 0.01f)
            {
                Overlay.Visible = false;
            }
        }

        if (_timeScaleHoldRemaining > 0f)
        {
            _timeScaleHoldRemaining = Mathf.Max(0f, _timeScaleHoldRemaining - dt);
        }
        else if (Engine.TimeScale < 1f)
        {
            Engine.TimeScale = 1f;
        }

        if (Camera is not null)
        {
            if (_shakeRemaining > 0f)
            {
                _shakeRemaining = Mathf.Max(0f, _shakeRemaining - dt);
                float progress = Mathf.Clamp(_shakeRemaining / (ShakeDuration + 0.0001f), 0f, 1f);
                Vector2 jitter = new Vector2(
                    _rng.RandfRange(-1f, 1f),
                    _rng.RandfRange(-1f, 1f)) * _shakeStrength * progress;
                Camera.Position = _cameraBasePosition + jitter;
            }
            else
            {
                Camera.Position = _cameraBasePosition;
            }
        }

        if (SpriteAnchor is not null)
        {
            if (_spriteHighlightTimer > 0f)
            {
                _spriteHighlightTimer = Mathf.Max(0f, _spriteHighlightTimer - dt);
                float highlightProgress = Mathf.Clamp(_spriteHighlightTimer / SpriteHighlightRecovery, 0f, 1f);
                float boost = 1f + highlightProgress * SpriteHighlightGain;
                SpriteAnchor.Modulate = _spriteBaseModulate * boost;
            }
            else
            {
                SpriteAnchor.Modulate = _spriteBaseModulate;
            }
        }
    }

    private void OnHealthChanged(int oldHealth)
    {
        if (HealthComponent is null)
        {
            return;
        }

        int newHealth = HealthComponent.CurrentHealth;
        if (newHealth <= 0)
        {
            return;
        }

        int damage = oldHealth - newHealth;
        if (damage <= 0)
        {
            return;
        }

        float normalized = Mathf.Clamp(damage / (HealthComponent.MaxHealth * DamageToIntensity), MinimumIntensity, 1f);
        TriggerFeedback(normalized);
    }

    private void TriggerFeedback(float intensity)
    {
        if (Overlay is not null)
        {
            Overlay.Visible = true;
            _overlayAlpha = OverlayMaxAlpha * intensity;
            Overlay.Color = _overlayBaseColor with { A = _overlayAlpha };
        }

        float targetScale = Mathf.Clamp(1f - intensity * (1f - MinTimeScale), MinTimeScale, 1f);
        Engine.TimeScale = Mathf.Min(Engine.TimeScale, targetScale);
        _timeScaleHoldRemaining = TimeScaleHoldDuration * intensity;

        _shakeRemaining = ShakeDuration * intensity;
        _shakeStrength = ShakeStrength * intensity;

        if (SpriteAnchor is not null)
        {
            _spriteHighlightTimer = SpriteHighlightRecovery * intensity;
        }
    }
}
