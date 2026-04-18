using Godot;
using CrossedDimensions.Extensions;

namespace CrossedDimensions.Environment.Triggers;

/// <summary>
/// AnimationPlayer that responds to trigger state changes.
/// Attach this script directly to an AnimationPlayer node and configure the trigger reference.
/// Queues animations based on trigger state transitions.
/// </summary>
[GlobalClass]
public partial class TriggerVisualListener : AnimationPlayer
{
    /// <summary>
    /// The trigger to listen to for state changes.
    /// </summary>
    [Export]
    public Trigger Trigger { get; set; }

    /// <summary>
    /// Animation to queue when trigger transitions from inactive to active.
    /// Default: "activate"
    /// </summary>
    [Export]
    public StringName ActivateAnimationName { get; set; } = "activate";

    /// <summary>
    /// Animation to queue when trigger transitions from active to inactive.
    /// Default: "deactivate"
    /// </summary>
    [Export]
    public StringName DeactivateAnimationName { get; set; } = "deactivate";

    /// <summary>
    /// Animation to queue when trigger is already active (looping state animation).
    /// Default: "active"
    /// </summary>
    [Export]
    public StringName ActiveAnimationName { get; set; } = "active";

    /// <summary>
    /// Animation to queue when trigger is already inactive (looping state animation).
    /// Default: "inactive"
    /// </summary>
    [Export]
    public StringName InactiveAnimationName { get; set; } = "inactive";

    private bool _previousState;

    public override void _Ready()
    {
        base._Ready();

        if (Trigger == null)
        {
            GD.PushError($"TriggerVisualListener '{Name}' requires a Trigger to be set.");
            return;
        }

        Trigger.TriggerStateChanged += OnTriggerStateChanged;

        // Set initial state
        _previousState = Trigger.IsActive;
        UpdateState(Trigger.IsActive, true);
    }

    private void OnTriggerStateChanged(bool active)
    {
        UpdateState(active, false);
    }

    private void UpdateState(bool isActivated, bool isInitial)
    {
        Stop();
        ClearQueue();

        if (isActivated)
        {
            if (!isInitial)
            {
                this.SafeQueueAnimation(ActivateAnimationName);
            }
            this.SafeQueueAnimation(ActiveAnimationName);
        }
        else
        {
            if (!isInitial)
            {
                this.SafeQueueAnimation(DeactivateAnimationName);
            }
            this.SafeQueueAnimation(InactiveAnimationName);
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete && Trigger != null)
        {
            Trigger.TriggerStateChanged -= OnTriggerStateChanged;
        }
    }
}
