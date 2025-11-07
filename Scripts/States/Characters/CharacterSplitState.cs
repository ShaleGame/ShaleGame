using Godot;

namespace CrossDimensions.States.Characters;

public sealed partial class CharacterSplitState : CharacterState
{
    [Export]
    public State IdleState { get; set; }

    [Export]
    public float SplitForce { get; set; } = 24f;

    private Vector2 _inputDirection = Vector2.Zero;

    public override State Enter(State previousState)
    {
        _inputDirection = CharacterContext.Controller.MovementInput;
        return null;
    }

    public override State Process(double delta)
    {
        return null;
    }
}
