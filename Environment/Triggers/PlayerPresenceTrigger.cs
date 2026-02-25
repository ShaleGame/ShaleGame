using Godot;

namespace CrossedDimensions.Environment.Triggers;

/// <summary>
/// Trigger that activates when a player (CharacterBody2D) enters an Area2D.
/// Pure logic component - visual components should listen to TriggerStateChanged signal.
/// </summary>
[GlobalClass]
public partial class PlayerPresenceTrigger : Trigger
{
    [Export]
    public Area2D DetectionArea { get; set; }

    private int _bodyCount = 0;

    /// <summary>
    /// The number of CharacterBody2D instances currently in the detection area.
    /// </summary>
    public int BodyCount
    {
        get => _bodyCount;
        private set
        {
            _bodyCount = value;

            // Activate if at least one body is present
            if (_bodyCount > 0)
            {
                SetActive(true);
            }
            // Deactivate only if no bodies present and StayActive is false
            else if (!StayActive)
            {
                SetActive(false);
            }
        }
    }

    public override void _Ready()
    {
        base._Ready();

        if (DetectionArea == null)
        {
            GD.PushError($"PlayerPresenceTrigger '{Name}' requires a DetectionArea to be set.");
            return;
        }

        DetectionArea.BodyEntered += OnBodyEntered;
        DetectionArea.BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is CharacterBody2D)
        {
            BodyCount++;
        }
    }

    private void OnBodyExited(Node body)
    {
        if (body is CharacterBody2D)
        {
            BodyCount--;
        }
    }
}
