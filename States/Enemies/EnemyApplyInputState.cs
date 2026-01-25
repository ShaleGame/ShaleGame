using Godot;

namespace CrossedDimensions.States.Enemies;

public sealed partial class EnemyApplyInputState : EnemyState
{
    public override State Process(double delta)
    {
        if (EnemyComponent is null || EnemyController is null || CharacterContext is null)
        {
            return null;
        }

        if (!EnemyComponent.HasTarget)
        {
            EnemyController.SetMovementInput(Vector2.Zero);
            EnemyController.SetPrimaryAttack(false);
            return null;
        }

        var offset = EnemyComponent.TargetPosition - CharacterContext.GlobalPosition;
        var direction = offset.Normalized();
        EnemyController.SetMovementInput(new Vector2(direction.X, 0));
        EnemyController.SetTargetInput(offset);

        var distance = offset.Length();
        EnemyController.SetPrimaryAttack(distance <= EnemyComponent.AttackRange);

        return null;
    }
}
