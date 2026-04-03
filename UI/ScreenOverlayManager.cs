using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace CrossedDimensions.UI;

/// <summary>
/// Global singleton that manages damage feedback effects: screen shake, overlay flash,
/// and timescale slowdown. This autoload ensures effects persist even when the source
/// entity (character, clone) is freed.
/// </summary>
[GlobalClass]
public partial class ScreenOverlayManager : CanvasLayer
{
    // -- Fade overlay ---------------------------------------------------------
    public static ScreenOverlayManager Instance { get; private set; }

    [Export]
    public ColorRect FadeOverlay { get; set; }

    [Export]
    public AnimationPlayer FadeAnimationPlayer { get; set; }

    [Export]
    public float FadeDuration { get; set; } = 0.4f;

    /// <summary>
    /// A full-screen ColorRect whose material uses DamageVignette.gdshader.
    /// The manager drives the shader's "strength" uniform directly.
    /// </summary>
    [Export]
    public ColorRect Overlay { get; set; }

    /// <summary>
    /// A Timer whose ProcessCallback is set to Always so it ticks even when
    /// Engine.TimeScale is 0. Wire this in the scene.
    /// </summary>
    [Export]
    public Timer FreezeTimer { get; set; }

    [ExportCategory("Feedback")]
    [Export]
    public float OverlayMaxStrength { get; set; } = 0.75f;

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

    /// <summary>
    /// The low-pass filter cutoff frequency (Hz) applied at full damage
    /// intensity.
    /// </summary>
    [Export]
    public float MuffleFilterCutoffHz { get; set; } = 800f;

    /// <summary>
    /// The maximum cutoff frequency (Hz) when any damage is applied. This
    /// allows some audible effect even at low intensities.
    [Export]
    public float MuffleFilterMaxCutoffHz { get; set; } = 2400f;

    /// <summary>
    /// How fast the cutoff frequency fades back to fully open (Hz per second).
    /// </summary>
    [Export]
    public float MuffleFilterFadeSpeed { get; set; } = 4000f;

    private AudioEffectLowPassFilter _lowPass;
    private const float FullCutoffHz = 20500f;
    private float _currentCutoffHz = FullCutoffHz;

    private float _overlayStrength;
    private ShaderMaterial _overlayMaterial;
    private float _shakeRemaining;
    private float _currentShakeIntensity;
    private Camera2D _activeCamera;
    private Vector2 _cameraBasePosition = Vector2.Zero;
    private readonly RandomNumberGenerator _rng = new();

    public override void _Ready()
    {
        Instance = this;
        _rng.Randomize();

        if (Overlay is not null)
        {
            _overlayMaterial = Overlay.Material as ShaderMaterial;
            _overlayMaterial?.SetShaderParameter("strength", 0f);
            Overlay.Visible = false;
        }

        if (FreezeTimer is not null)
        {
            FreezeTimer.OneShot = true;
            FreezeTimer.Timeout += OnFreezeTimerTimeout;
        }

        int busIdx = AudioServer.GetBusIndex("Master");
        _lowPass = Enumerable.Range(0, AudioServer.GetBusEffectCount(busIdx))
            .Select(i => AudioServer.GetBusEffect(busIdx, i))
            .OfType<AudioEffectLowPassFilter>()
            .FirstOrDefault();
    }

    /// <summary>Fades the screen to black. Call before changing scene.</summary>
    public async Task FadeIn()
    {
        FadeOverlay.Modulate = new Color(0, 0, 0, 0);
        FadeOverlay.Visible = true;
        FadeAnimationPlayer.Play("fade_in");
        await ToSignal(FadeAnimationPlayer, AnimationPlayer.SignalName.AnimationFinished);
    }

    /// <summary>Fades the screen from black. Call after scene is ready.</summary>
    public async Task FadeOut()
    {
        FadeAnimationPlayer.Play("fade_out");
        await ToSignal(FadeAnimationPlayer, AnimationPlayer.SignalName.AnimationFinished);
        FadeOverlay.Visible = false;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        // Update overlay fade
        if (Overlay is not null && _overlayStrength > 0f)
        {
            _overlayStrength = Mathf.Max(0f, _overlayStrength - OverlayFadeSpeed * dt);
            _overlayMaterial?.SetShaderParameter("strength", _overlayStrength);

            if (_overlayStrength <= 0.01f)
            {
                _overlayStrength = 0f;
                _overlayMaterial?.SetShaderParameter("strength", 0f);
                Overlay.Visible = false;
            }
        }

        if (_lowPass is not null && _currentCutoffHz < FullCutoffHz)
        {
            _currentCutoffHz = Mathf.Min(
                FullCutoffHz,
                _currentCutoffHz + MuffleFilterFadeSpeed * dt);
            _lowPass.CutoffHz = _currentCutoffHz;
        }

        // Update camera shake
        if (_activeCamera is not null && IsInstanceValid(_activeCamera))
        {
            if (_shakeRemaining > 0f)
            {
                _shakeRemaining = Mathf.Max(0f, _shakeRemaining - dt);

                // Attenuate by how much freeze time is left so the shake dies
                // down as the hold elapses, even when TimeScale is 0.
                float freezeProgress = FreezeTimer is not null
                    ? Mathf.Clamp((float)FreezeTimer.TimeLeft / (float)FreezeTimer.WaitTime, 0f, 1f)
                    : Mathf.Clamp(_shakeRemaining / (ShakeDuration + 0.0001f), 0f, 1f);

                Vector2 jitter = new Vector2(
                    _rng.RandfRange(-1f, 1f),
                    _rng.RandfRange(-1f, 1f)) * ShakeStrength * _currentShakeIntensity * freezeProgress;
                _activeCamera.Position = _cameraBasePosition + jitter;
            }
            else
            {
                _activeCamera.Position = _cameraBasePosition;
                _activeCamera = null; // Release camera reference
            }
        }
        else if (_shakeRemaining > 0f)
        {
            // Camera became invalid, stop shake
            _shakeRemaining = 0f;
            _activeCamera = null;
        }
    }

    /// <summary>
    /// Triggers damage feedback effects based on damage intensity.
    /// </summary>
    /// <param name="damage">Amount of damage taken</param>
    /// <param name="maxHealth">Maximum health of the entity</param>
    /// <param name="camera">Camera to shake (optional)</param>
    /// <param name="damageToIntensity">Multiplier to convert damage to intensity</param>
    /// <param name="minimumIntensity">Minimum intensity threshold</param>
    public void TriggerDamage(
        int damage,
        int maxHealth,
        Camera2D camera = null,
        float damageToIntensity = 1.5f,
        float minimumIntensity = 0.15f)
    {
        if (damage <= 0)
        {
            return;
        }

        float intensity = Mathf.Clamp(
            damage / (maxHealth * damageToIntensity),
            minimumIntensity,
            1f);

        TriggerFeedback(intensity, camera);
    }

    /// <summary>
    /// Triggers damage feedback at death intensity (1.0).
    /// </summary>
    /// <param name="camera">Camera to shake (optional)</param>
    public void TriggerDeathFeedback(Camera2D camera = null)
    {
        TriggerFeedback(1f, camera);
    }

    /// <summary>
    /// Applies visual and time-scale feedback at the given intensity (0-1).
    /// </summary>
    private void TriggerFeedback(float intensity, Camera2D camera)
    {
        // Update overlay
        if (Overlay is not null)
        {
            Overlay.Visible = true;
            _overlayStrength = Mathf.Max(_overlayStrength, OverlayMaxStrength * intensity);
            _overlayMaterial?.SetShaderParameter("strength", _overlayStrength);
        }

        if (_lowPass is not null)
        {
            // scale cutoff between max and min based on intensity, so even
            // low damage has some effect
            float target = Mathf.Lerp(MuffleFilterMaxCutoffHz, MuffleFilterCutoffHz, intensity);
            // take the lower cutoff so stacking hits do not reduce the effect
            _currentCutoffHz = Mathf.Min(_currentCutoffHz, target);
            _lowPass.CutoffHz = _currentCutoffHz;
        }

        // Update timescale (use minimum to handle stacking)
        Engine.TimeScale = Mathf.Min(Engine.TimeScale, MinTimeScale);

        // Update camera shake (refresh if stronger)
        if (camera is not null && IsInstanceValid(camera))
        {
            if (_activeCamera != camera)
            {
                // Store base position when we start shaking a new camera
                _activeCamera = camera;
                _cameraBasePosition = camera.Position;
            }

            if (intensity > _currentShakeIntensity)
            {
                _currentShakeIntensity = intensity;
            }

            // Extend shake duration
            float newShakeDuration = ShakeDuration * intensity;
            _shakeRemaining = Mathf.Max(_shakeRemaining, newShakeDuration);
        }

        // Update freeze timer
        float hold = FreezeHoldDuration * intensity;

        if (FreezeTimer is not null)
        {
            // Restart timer if new freeze would be longer
            if (!FreezeTimer.IsStopped() && FreezeTimer.TimeLeft < hold)
            {
                FreezeTimer.Start(hold);
            }
            else if (FreezeTimer.IsStopped())
            {
                FreezeTimer.Start(hold);
            }
        }
        else
        {
            // No timer: snap back immediately
            Engine.TimeScale = 1f;
        }
    }

    private void OnFreezeTimerTimeout()
    {
        Engine.TimeScale = 1f;
        _currentShakeIntensity = 0f;
    }
}
