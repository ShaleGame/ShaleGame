using Godot;

namespace CrossedDimensions.States.Enemies;

public sealed partial class EnemySenseTargetState : EnemyState
{
    public override State Process(double delta)
    {
        if (CharacterContext is null || EnemyComponent is null)
        {
            return null;
        }

        var player = CharacterContext.GetTree().GetFirstNodeInGroup("Player") as Node2D;
        if (player is null)
        {
            EnemyComponent.HasTarget = false;
            EnemyComponent.TargetNode = null;
            EnemyComponent.TargetPosition = Vector2.Zero;
            return null;
        }

        var distance = CharacterContext.GlobalPosition.DistanceTo(player.GlobalPosition);
        EnemyComponent.HasTarget = distance <= EnemyComponent.AggroRange;
        EnemyComponent.TargetNode = player;
        EnemyComponent.TargetPosition = player.GlobalPosition;

        return null;
    }
}
