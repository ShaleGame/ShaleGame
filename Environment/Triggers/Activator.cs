using System.Linq;
using Godot;

namespace CrossedDimensions.Environment.Triggers;

/// <summary>
/// Abstract base class for activators. Activators are pure logic components that
/// listen to triggers and determine when to activate based on trigger states.
/// Visual/physics components should listen to Activated/Deactivated signals.
/// Inherits from Node2D to allow positioning in the scene tree.
/// </summary>
public abstract partial class Activator : Node2D
{
    /// <summary>
    /// Array of triggers to listen to. The activator will subscribe to their
    /// TriggerStateChanged signals automatically.
    /// </summary>
    [Export]
    public Godot.Collections.Array<Trigger> Triggers { get; set; } = [];

    /// <summary>
    /// If true, the activator will remain activated once activated and never deactivate.
    /// </summary>
    [Export]
    public bool StayActivated { get; set; } = false;

    /// <summary>
    /// Current activation state.
    /// </summary>
    public bool IsActivated { get; private set; } = false;

    /// <summary>
    /// Emitted when the activator becomes activated.
    /// </summary>
    [Signal]
    public delegate void ActivatedEventHandler();

    /// <summary>
    /// Emitted when the activator becomes deactivated.
    /// </summary>
    [Signal]
    public delegate void DeactivatedEventHandler();

    public override void _Ready()
    {
        // Subscribe to all trigger signals
        foreach (var trigger in Triggers)
        {
            if (trigger != null)
            {
                trigger.TriggerStateChanged += OnTriggerStateChanged;
            }
        }

        // Evaluate initial state
        EvaluateActivation();
    }

    private void OnTriggerStateChanged(bool active)
    {
        EvaluateActivation();
    }

    private void EvaluateActivation()
    {
        bool shouldActivate = ShouldActivate();

        if (shouldActivate && !IsActivated)
        {
            IsActivated = true;
            EmitSignal(SignalName.Activated);
        }
        else if (!shouldActivate && IsActivated && !StayActivated)
        {
            IsActivated = false;
            EmitSignal(SignalName.Deactivated);
        }
    }

    /// <summary>
    /// Override this method to implement custom activation logic.
    /// Return true if the activator should be activated based on current trigger states.
    /// </summary>
    protected abstract bool ShouldActivate();
}
