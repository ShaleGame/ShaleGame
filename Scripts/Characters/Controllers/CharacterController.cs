using Godot;

namespace CrossDimensions.Characters.Controllers;

public abstract partial class CharacterController : Node2D
{
    /// <summary>
    /// Gets or sets the scale factor for the X axis input. Useful for inverting controls
    /// such as when a cloned character has mirrored movement.
    /// </summary>
    [Export]
    public float XScale { get; set; } = 1.0f;

    public bool IsMoving => !MovementInput.IsZeroApprox();

    public abstract Vector2 MovementInput { get; }

    public abstract Vector2 Target { get; }

    public abstract bool IsJumping { get; }

    public abstract bool IsMouse1Held { get; }

    public abstract bool IsMouse2Held { get; }

    public abstract bool IsSplitting { get; }
}
