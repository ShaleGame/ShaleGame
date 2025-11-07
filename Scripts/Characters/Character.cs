using Godot;

namespace CrossDimensions.Characters;

public partial class Character : CharacterBody2D
{
    [Export]
    public Controllers.CharacterController Controller { get; set; }

    [Export]
    public States.StateMachine MovementStateMachine { get; set; }

    [Export]
    public float Speed { get; set; } = 16f;

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
