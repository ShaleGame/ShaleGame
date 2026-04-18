using Godot;
using CrossedDimensions.Extensions;

namespace CrossedDimensions.Environment.Triggers;

/// <summary>
/// AnimationPlayer that responds to activator state changes.
/// Attach this script directly to an AnimationPlayer node and configure the activator reference.
/// Queues animations based on activator state transitions.
/// Can optionally disable collision shapes when activated.
/// </summary>
[GlobalClass]
public partial class ActivatorVisualListener : AnimationPlayer
{
    /// <summary>
    /// The activator to listen to for state changes.
    /// </summary>
    [Export]
    public Activator Activator { get; set; }

    /// <summary>
    /// Animation to queue when activator transitions from deactivated to activated.
    /// Default: "activate"
    /// </summary>
    [Export]
    public StringName ActivateAnimationName { get; set; } = "activate";

    /// <summary>
    /// Animation to queue when activator transitions from activated to deactivated.
    /// Default: "deactivate"
    /// </summary>
    [Export]
    public StringName DeactivateAnimationName { get; set; } = "deactivate";

    /// <summary>
    /// Animation to queue when activator is already activated (looping state animation).
    /// Default: "active"
    /// </summary>
    [Export]
    public StringName ActiveAnimationName { get; set; } = "active";

    /// <summary>
    /// Animation to queue when activator is already deactivated (looping state animation).
    /// Default: "inactive"
    /// </summary>
    [Export]
    public StringName InactiveAnimationName { get; set; } = "inactive";

    private bool _previousState;

    public override void _Ready()
    {
        base._Ready();

        if (Activator == null)
        {
            GD.PushError($"ActivatorVisualListener '{Name}' requires an Activator to be set.");
            return;
        }

        Activator.Activated += OnActivated;
        Activator.Deactivated += OnDeactivated;

        // Set initial state
        _previousState = Activator.IsActivated;
        UpdateState(Activator.IsActivated, isInitial: true);
    }

    private void OnActivated()
    {
        UpdateState(true, isInitial: false);
    }

    private void OnDeactivated()
    {
        UpdateState(false, isInitial: false);
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
        if (what == NotificationPredelete && Activator != null)
        {
            Activator.Activated -= OnActivated;
            Activator.Deactivated -= OnDeactivated;
        }
    }
}
