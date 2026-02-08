using Godot;

namespace CrossedDimensions.Environment.Cutscene;

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
    private bool InteractAllowed = false;
    private float _holdTimer = 0f;
    [Export]
    public StringName InteractAction { get; set; } = "interact";
    [Export]
    public int InteractPriority { get; set; } = 0;
    
    private void OnArea2DBodyEntered(Node body)
    {
        if (body is CharacterBody2D)
        {
            InteractAllowed = true;
        }
    }
    private void OnArea2DBodyExited(Node body)
    {
        if (body is CharacterBody2D)
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
    }
}