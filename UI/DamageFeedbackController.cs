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

    [Export]
    public ColorRect DeathOverlay { get; set; }

    /// <summary>
    /// A Timer whose ProcessCallback is set to Always so it ticks even when
    /// Engine.TimeScale is 0. Wire this in the scene.
    /// </summary>
    [Export]
    public Timer FreezeTimer { get; set; }

    [ExportCategory("Feedback")]
    [Export]
    public float OverlayMaxAlpha { get; set; } = 0.75f;

    [Export]
    public float OverlayFadeSpeed { get; set; } = 2.5f;

    [Export]
    public float MinTimeScale { get; set; } = 0.6f;

    /// <summary>
    /// Real-time seconds to hold the slowed timescale before snapping back.
    /// Scales with intensity.
    /// </summary>
    [Export]
    public float FreezeHoldDuration { get; set; } = 0.25f;

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

    [ExportCategory("Death Feedback")]
    [Export]
    public float DeathFreezeDuration { get; set; } = 0.4f;

    [Export]
    public float DeathFadeInSpeed { get; set; } = 1.5f;

    /// <summary>
    /// Emitted once the death overlay has fully faded to black, signalling that
    /// it is safe to transition to the death screen.
    /// </summary>
    [Signal]
    public delegate void DeathFadeCompletedEventHandler();

    private float _overlayAlpha;
    private float _shakeRemaining;
    private float _shakeStrength;
    private float _spriteHighlightTimer;
    private Vector2 _cameraBasePosition = Vector2.Zero;
    private Color _overlayBaseColor = new Color(0, 0, 0, 1);
    private Color _spriteBaseModulate = new Color(1, 1, 1, 1);
    private readonly RandomNumberGenerator _rng = new();

    // True while the death-fade overlay is animating.
    private bool _dying;
    private float _deathOverlayAlpha;

    // What to do when the freeze timer fires: null = snap back to normal speed,
    // non-null = snap back then run the action (used for death fade).
    private System.Action _onFreezeComplete;


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

        if (DeathOverlay is not null)
        {
            DeathOverlay.Color = DeathOverlay.Color with { A = 0f };
            DeathOverlay.Visible = false;
        }

        if (SpriteAnchor is not null)
        {
            _spriteBaseModulate = SpriteAnchor.Modulate;
        }

        if (FreezeTimer is not null)
        {
            FreezeTimer.OneShot = true;
            FreezeTimer.Timeout += OnFreezeTimerTimeout;
        }
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        // Death fade runs at real speed after freeze ends; delta is unscaled
        // because timescale is already restored to 1 before _dying is set.
        if (_dying)
        {
            ProcessDeathFade(dt);
            return;
        }

        if (Overlay is not null && _overlayAlpha > 0f)
        {
            _overlayAlpha = Mathf.Max(0f, _overlayAlpha - OverlayFadeSpeed * dt);
            Overlay.Color = _overlayBaseColor with { A = _overlayAlpha };

            if (_overlayAlpha <= 0.01f)
            {
                Overlay.Visible = false;
            }
        }

        if (Camera is not null)
        {
            if (_shakeRemaining > 0f)
            {
                _shakeRemaining = Mathf.Max(0f, _shakeRemaining - dt);

                // Attenuate by how much freeze time is left so the shake dies
                // down as the hold elapses, even when TimeScale is 0.
                float freezeProgress = FreezeTimer is not null
                    ? Mathf.Clamp((float)FreezeTimer.TimeLeft, 0f, 1f)
                    : Mathf.Clamp(_shakeRemaining / (ShakeDuration + 0.0001f), 0f, 1f);

                Vector2 jitter = new Vector2(
                    _rng.RandfRange(-1f, 1f),
                    _rng.RandfRange(-1f, 1f)) * _shakeStrength * freezeProgress;
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

    private void ProcessDeathFade(float dt)
    {
        if (DeathOverlay is null)
        {
            return;
        }

        _deathOverlayAlpha = Mathf.Min(1f, _deathOverlayAlpha + DeathFadeInSpeed * dt);
        DeathOverlay.Color = DeathOverlay.Color with { A = _deathOverlayAlpha };

        if (_deathOverlayAlpha >= 1f)
        {
            _dying = false;
            EmitSignal(SignalName.DeathFadeCompleted);
        }
    }

    private void OnHealthChanged(int oldHealth)
    {
        if (HealthComponent is null)
        {
            return;
        }

        int newHealth = HealthComponent.CurrentHealth;
        int damage = oldHealth - newHealth;
        if (damage <= 0)
        {
            return;
        }

        if (newHealth <= 0)
        {
            TriggerFeedback(1f, freezeDuration: DeathFreezeDuration, afterFreeze: BeginDeathFade);
        }
        else
        {
            float normalized = Mathf.Clamp(
                damage / (HealthComponent.MaxHealth * DamageToIntensity),
                MinimumIntensity, 1f);
            TriggerFeedback(normalized);
        }
    }

    /// <summary>
    /// Applies visual and time-scale feedback at the given intensity (0–1).
    /// Optionally runs <paramref name="afterFreeze"/> when the hold timer expires.
    /// </summary>
    private void TriggerFeedback(
        float intensity,
        float? freezeDuration = null,
        System.Action afterFreeze = null
    )
    {
        if (Overlay is not null)
        {
            Overlay.Visible = true;
            _overlayAlpha = OverlayMaxAlpha * intensity;
            Overlay.Color = _overlayBaseColor with { A = _overlayAlpha };
        }

        //float targetScale = Mathf.Clamp(1f - intensity * (1f - MinTimeScale), MinTimeScale, 1f);
        float targetScale = MinTimeScale;
        Engine.TimeScale = Mathf.Min(Engine.TimeScale, targetScale);

        _shakeRemaining = ShakeDuration * intensity;
        _shakeStrength = ShakeStrength * intensity;

        if (SpriteAnchor is not null)
        {
            _spriteHighlightTimer = SpriteHighlightRecovery * intensity;
        }

        _onFreezeComplete = afterFreeze;

        float hold = freezeDuration ?? FreezeHoldDuration * intensity;

        if (FreezeTimer is not null)
        {
            FreezeTimer.Start(hold);
        }
        else
        {
            // No timer: snap back immediately and run callback if present.
            Engine.TimeScale = 1f;
            afterFreeze?.Invoke();
        }
    }

    private void OnFreezeTimerTimeout()
    {
        Engine.TimeScale = 1f;
        _onFreezeComplete?.Invoke();
        _onFreezeComplete = null;
    }

    private void BeginDeathFade()
    {
        _dying = true;
        _deathOverlayAlpha = 0f;

        if (DeathOverlay is not null)
        {
            DeathOverlay.Visible = true;
            DeathOverlay.Color = DeathOverlay.Color with { A = 0f };
        }
    }
}
