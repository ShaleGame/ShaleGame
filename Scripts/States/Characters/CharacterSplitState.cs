using Godot;

namespace CrossDimensions.States.Characters;

public sealed partial class CharacterSplitState : CharacterState
{
    [Export]
    public State IdleState { get; set; }

    private Vector2 _inputDirection = Vector2.Zero;

    private CrossDimensions.Characters.Character _clone;

    private double _timeLeft;

    public override State Enter(State previousState)
    {
        _inputDirection = CharacterContext.Controller
            .MovementInput
            .Normalized();

        _timeLeft = 0.1f;
        
        if (PerformSplit())
        {
            var clone = CharacterContext.Cloneable.Clone;
            clone.MovementStateMachine.InitialState = clone
                .MovementStateMachine
                .GetNode<State>("Split State");
        }

        return null;
    }

    public override State Process(double delta)
    {
        return null;
    }

    public override State PhysicsProcess(double delta)
    {
        CharacterContext.Velocity = _inputDirection * CharacterContext
            .Cloneable.SplitForce;
        CharacterContext.MoveAndSlide();

        if ((_timeLeft -= delta) <= 0)
        {
            return IdleState;
        }

        return null;
    }
}
