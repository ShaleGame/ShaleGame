using Godot;

namespace CrossedDimensions.Environment.Triggers;

/// <summary>
/// Abstract base class for all triggers. Triggers are pure logic components that
/// determine their own active state and emit signals when state changes.
/// Visual components should listen to the TriggerStateChanged signal.
/// Inherits from Node2D to allow positioning in the scene tree.
/// </summary>
public abstract partial class Trigger : Node2D
{
    /// <summary>
    /// Optional key for persisting trigger state in the save system.
    /// If empty, trigger state will not be saved.
    /// </summary>
    [Export]
    public string SaveKey { get; set; } = "";

    /// <summary>
    /// If true, the trigger will remain active once activated and never deactivate.
    /// This is useful for one-time puzzle solutions or permanent progress.
    /// </summary>
    [Export]
    public bool StayActive { get; set; } = false;

    /// <summary>
    /// Current active state of the trigger.
    /// </summary>
    public bool IsActive { get; private set; } = false;

    /// <summary>
    /// Emitted when the trigger's active state changes.
    /// </summary>
    [Signal]
    public delegate void TriggerStateChangedEventHandler(bool active);

    public override void _Ready()
    {
        // Load saved state only for stay-active triggers that specify a key
        if (StayActive && !string.IsNullOrEmpty(SaveKey) && Saves.SaveManager.Instance != null)
        {
            if (Saves.SaveManager.Instance.TryGetKey<bool>(SaveKey, out var savedState))
            {
                SetActive(savedState, saveToFile: false);
            }
        }
    }

    /// <summary>
    /// Sets the active state of the trigger and emits the TriggerStateChanged signal.
    /// </summary>
    /// <param name="active">The new active state.</param>
    /// <param name="saveToFile">Whether to persist this state to the save file (if SaveKey is set).</param>
    public void SetActive(bool active, bool saveToFile = true)
    {
        // Don't change state if already at target state
        if (IsActive == active)
        {
            return;
        }

        // Don't deactivate if StayActive is true
        if (IsActive && !active && StayActive)
        {
            return;
        }

        IsActive = active;
        EmitSignal(SignalName.TriggerStateChanged, active);

        // Save to file only for stay-active triggers
        if (saveToFile && StayActive && !string.IsNullOrEmpty(SaveKey) && Saves.SaveManager.Instance != null)
        {
            Saves.SaveManager.Instance.SetKey(SaveKey, IsActive);
        }
    }
}
