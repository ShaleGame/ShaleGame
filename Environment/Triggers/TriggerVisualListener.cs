using Godot;

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
        UpdateVisuals(Trigger.IsActive, isInitial: true);
    }

    private void OnTriggerStateChanged(bool active)
    {
        UpdateVisuals(active, isInitial: false);
    }

    private void UpdateVisuals(bool active, bool isInitial)
    {
        StringName animationToQueue;

        if (isInitial)
        {
            // On initial state, play the looping state animation
            animationToQueue = active ? ActiveAnimationName : InactiveAnimationName;
        }
        else
        {
            // On state change, play the transition animation
            if (active && !_previousState)
            {
                // Transitioning from inactive to active
                animationToQueue = ActivateAnimationName;
            }
            else if (!active && _previousState)
            {
                // Transitioning from active to inactive
                animationToQueue = DeactivateAnimationName;
            }
            else
            {
                // State didn't actually change, use looping animation
                animationToQueue = active ? ActiveAnimationName : InactiveAnimationName;
            }
        }

        _previousState = active;

        // Queue the animation if it exists
        if (HasAnimation(animationToQueue))
        {
            Queue(animationToQueue);
        }
        else
        {
            GD.PushWarning($"TriggerVisualListener '{Name}': Does not have animation '{animationToQueue}'");
        }
    }

    public override void _ExitTree()
    {
        if (Trigger != null)
        {
            Trigger.TriggerStateChanged -= OnTriggerStateChanged;
        }

        base._ExitTree();
    }
}
