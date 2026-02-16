using Godot;

namespace CrossedDimensions.Environment.Cutscene.Interactables;

/// <summary>
/// Base example for interactable objects
/// </summary>

public partial class Interactable : Area2D
{
    [Export]
    public Sprite2D Sprite { get; set; }

    [Export]
    public float HoldSecs { get; set; } = 1.0f;

    [Export]
    public bool InteractAllowed { get; private set; } = false;

    private float _holdTimer = 0f;

    [Export]
    public StringName InteractAction { get; set; } = "interact";

    [Export]
    public int InteractPriority { get; set; } = 0;

    [Signal]
    public delegate void InteractedEventHandler();

    internal void OnArea2DBodyEntered(Node body)
    {
        if (GetOverlappingBodies().Count > 0)
        {
            InteractAllowed = true;
        }
    }

    internal void OnArea2DBodyExited(Node body)
    {
        if (GetOverlappingBodies().Count == 0)
        {
            InteractAllowed = false;
        }
    }

    public override void _Process(double delta)
    {
        if (!InteractAllowed)
        {
            _holdTimer = 0f;
            return;
        }

        if (Input.IsActionPressed(InteractAction))
        {
            _holdTimer += (float)delta;

            if (_holdTimer >= HoldSecs)
            {
                _holdTimer = 0f;
                Interact();
            }
        }
        else
        {
            _holdTimer = 0f;
        }
    }

    protected virtual void Interact()
    {
        GD.Print($"Interacted with {Name}");
        EmitSignal(SignalName.Interacted);
    }
}
