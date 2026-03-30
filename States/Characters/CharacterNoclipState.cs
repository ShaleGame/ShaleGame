using Godot;

namespace CrossedDimensions.States.Characters;

public sealed partial class CharacterNoclipState : CharacterState
{
    [Export]
    public float NoclipSpeed { get; set; } = 800f;

    public override State Enter(State previousState)
    {
        CharacterContext.Velocity = Vector2.Zero;
        CharacterContext.VelocityFromInput = Vector2.Zero;
        CharacterContext.VelocityFromExternalForces = Vector2.Zero;
        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        return null;
    }

    public override State PhysicsProcess(double delta)
    {
        Vector2 direction = CharacterContext.Controller?.MovementInput ?? Vector2.Zero;
        Vector2 movement = direction * NoclipSpeed;

        CharacterContext.VelocityFromInput = movement;
        CharacterContext.VelocityFromExternalForces = Vector2.Zero;
        CharacterContext.Velocity = Vector2.Zero;
        CharacterContext.GlobalPosition += movement * (float)delta;

        return null;
    }
}
