using Godot;
using System.Collections;

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

    private void 
}