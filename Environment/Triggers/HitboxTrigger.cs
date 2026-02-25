using Godot;

namespace CrossedDimensions.Environment.Triggers;

/// <summary>
/// Trigger that activates when hit by a Hitbox (e.g., projectile, explosion).
/// Uses a Hurtbox to detect hits and can temporarily activate based on a timer.
/// Pure logic component - visual components should listen to TriggerStateChanged signal.
/// </summary>
[GlobalClass]
public partial class HitboxTrigger : Trigger
{
    /// <summary>
    /// The hurtbox that detects hits from hitboxes.
    /// </summary>
    [Export]
    public BoundingBoxes.Hurtbox Hurtbox { get; set; }

    /// <summary>
    /// Duration in seconds that the trigger stays active after being hit.
    /// Only used when StayActive is false.
    /// </summary>
    [Export]
    public float ActiveDuration { get; set; } = 1.0f;

    private Timer _deactivationTimer;

    public override void _Ready()
    {
        base._Ready();

        if (Hurtbox == null)
        {
            GD.PushError($"HitboxTrigger '{Name}' requires a Hurtbox to be set.");
            return;
        }

        Hurtbox.HurtboxHit += OnHurtboxHit;

        // Create deactivation timer if needed
        if (!StayActive)
        {
            _deactivationTimer = new Timer
            {
                OneShot = true,
                WaitTime = ActiveDuration
            };
            _deactivationTimer.Timeout += OnDeactivationTimerTimeout;
            AddChild(_deactivationTimer);
        }
    }

    private void OnHurtboxHit(BoundingBoxes.Hitbox hitbox, float damage)
    {
        // Activate the trigger when hit
        SetActive(true);

        // Start deactivation timer if not staying active permanently
        if (!StayActive && _deactivationTimer != null)
        {
            _deactivationTimer.Start();
        }
    }

    private void OnDeactivationTimerTimeout()
    {
        // Deactivate after the timer expires
        SetActive(false);
    }

    public override void _ExitTree()
    {
        if (Hurtbox != null)
        {
            Hurtbox.HurtboxHit -= OnHurtboxHit;
        }

        if (_deactivationTimer != null)
        {
            _deactivationTimer.Timeout -= OnDeactivationTimerTimeout;
        }

        base._ExitTree();
    }
}
