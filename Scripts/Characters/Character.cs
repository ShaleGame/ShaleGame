using Godot;

namespace CrossDimensions.Characters;

public partial class Character : CharacterBody2D
{
    [Export]
    public Controllers.CharacterController Controller { get; set; }

    [Export]
    public States.StateMachine MovementStateMachine { get; set; }

    [Export]
    public float Speed { get; set; } = 192f;

    [Export]
    public float JumpForce { get; set; } = 384f;

    [Export]
    public CloneableComponent Cloneable { get; set; } = null;

    public Vector2 VelocityFromInput { get; set; } = Vector2.Zero;

    public Vector2 VelocityFromExternalForces { get; set; } = Vector2.Zero;

    public int AvailableJumps { get; set; } = 1;

    /// <summary>
    /// Indicates whether a jump input has been buffered. This is used to allow
    /// the character to hold the jump button slightly before landing and jump
    /// as soon as they touch the ground, only if the player has exhausted their
    /// available jumps.
    /// </summary>
    public bool IsJumpBuffered { get; set; } = false;

    public float AirAcceleration { get; set; } = 128f;

    public override void _Ready()
    {
        MovementStateMachine.Initialize(this);
    }

    public override void _Process(double delta)
    {
        MovementStateMachine.Process(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        MovementStateMachine.PhysicsProcess(delta);
    }
}
