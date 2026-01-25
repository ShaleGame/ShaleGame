using Godot;

namespace CrossedDimensions.Characters;

[GlobalClass]
public partial class Character : CharacterBody2D
{
    /// <summary>
    /// The movement speed of the character in units per second.
    /// </summary>
    [Export]
    public float Speed { get; set; } = 192f;

    /// <summary>
    /// The number of units the player can jump at max held duration
    /// </summary>
    [Export]
    public float JumpHeight { get; set; } = 72.0f;

    /// <summary>
    /// The time at which jumping began
    /// Used with JumpTime to determine when to stop extending jump height
    /// </summary>
    public float JumpHeldAtTime = 0.0f;

    /// <summary>
    /// The time at which jumping ended
    /// Used to calculate JumpGravBoostTime to force jump height to be lowered
    /// </summary>
    public float JumpReleasedAtTime = 0.0f;

    /// <summary>
    /// The amount of time that extra gravity is applied to JumpInitialVelocity
    /// Used to restrict jump height when jump is not held for the full JumpTime
    /// </summary>
    public float JumpGravBoostTime = 0.0f;

    /// <summary>
    /// A bool for whether the jump input is allowed
    /// Used to turn off jump inputs after jump key released or time exceeded
    /// </summary>
    public bool AllowJumpInput = true;

    /// <summary>
    /// The controller component that grabs input for this character.
    /// </summary>
    [ExportCategory("Components")]
    [Export]
    public Controllers.CharacterController Controller { get; set; }

    /// <summary>
    /// The state machine that controls the movement states of the character.
    /// </summary>
    [Export]
    public States.StateMachine MovementStateMachine { get; set; }

    /// <summary>
    /// The health component that manages the health of this character.
    /// </summary>
    [Export]
    public Components.HealthComponent Health { get; set; }

    /// <summary>
    /// The state machine that controls the brain (AI or player) of the character.
    /// </summary>
    [Export]
    [ExportGroup("AI")]
    public States.StateMachine BrainStateMachine { get; set; }

    /// <summary>
    /// The cloneable component that allows this character to be cloned or
    /// merged. If null, this character cannot be cloned.
    /// </summary>
    [Export]
    [ExportGroup("Cloning")]
    public CloneableComponent Cloneable { get; set; } = null;


    /// <summary>
    /// The velocity of the character from input controls. This is used by the
    /// movement states to compute <c>Velocity</c>.
    /// </summary>
    public Vector2 VelocityFromInput { get; set; } = Vector2.Zero;

    /// <summary>
    /// The velocity of the character from external forces (e.g. knockback,
    /// gravity). This is used by the movement states to compute
    /// <c>Velocity</c>.
    /// </summary>
    public Vector2 VelocityFromExternalForces { get; set; } = Vector2.Zero;

    public override void _Ready()
    {
        BrainStateMachine?.Initialize(this);
        MovementStateMachine.Initialize(this);
    }

    public override void _Process(double delta)
    {
        BrainStateMachine?.Process(delta);
        MovementStateMachine.Process(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        BrainStateMachine?.PhysicsProcess(delta);
        MovementStateMachine.PhysicsProcess(delta);
    }

    public override void _Input(InputEvent @event)
    {
        BrainStateMachine?.Input(@event);
        MovementStateMachine.Input(@event);
    }
}
