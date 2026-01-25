using Godot;

namespace CrossedDimensions.Entities.Enemies;

public abstract partial class AttackPattern : Node
{
    [Export]
    public int PatternId { get; set; }

    [Export]
    public bool Homing { get; set; }

    [Export]
    public float Speed { get; set; } = 300f;

    public abstract void ExecuteAttack(Vector2 origin, Vector2 target, float angle);
}
