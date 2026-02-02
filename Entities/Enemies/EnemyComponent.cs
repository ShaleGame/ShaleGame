using Godot;

namespace CrossedDimensions.Entities.Enemies;

[GlobalClass]
public partial class EnemyComponent : Node
{
    [Export]
    public string EnemyName { get; set; } = "Enemy";

    public enum EnemyClass
    {
        Melee,
        Ranged,
        Support,
        Boss
    }

    public bool HasTarget { get; set; }

    public Node2D TargetNode { get; set; }

    public Vector2 TargetPosition { get; set; } = Vector2.Zero;
}
