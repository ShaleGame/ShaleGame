using Godot;

namespace CrossedDimensions.Entities.Enemies;

[GlobalClass]
public partial class EnemyComponent : Node
{
    [Export]
    public int Id { get; set; }

    [Export]
    public string EnemyName { get; set; } = "Enemy";

    public enum EnemyClass
    {
        Melee,
        Ranged,
        Support,
        Boss
    }

    [Export]
    public EnemyClass Class { get; set; } = EnemyClass.Melee;

    [Export]
    public AttackPattern AttackPattern { get; set; }

    [Export]
    public float AggroRange { get; set; } = 300f;

    [Export]
    public float AttackRange { get; set; } = 200f;

    public bool HasTarget { get; set; }

    public Node2D TargetNode { get; set; }

    public Vector2 TargetPosition { get; set; } = Vector2.Zero;
}
