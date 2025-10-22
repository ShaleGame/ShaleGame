using Godot;

namespace CrossDimensions.Characters.Controllers;

public abstract partial class CharacterController : Node2D
{
    public abstract Vector2 MovementInput { get; }

    public abstract Vector2 Target { get; }

    public abstract bool IsJumping { get; }

    public abstract bool IsMouse1Held { get; }

    public abstract bool IsMouse2Held { get; }
}
