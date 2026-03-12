using Godot;

namespace CrossedDimensions.Components;

[GlobalClass]
public partial class DamageComponent : Node
{
    [Export]
    public int DamageAmount { get; set; } = 10;

    [Export]
    public float KnockbackMultiplier { get; set; } = 1f;

    [Export]
    public Vector2 KnockbackDirectionScale { get; set; } = Vector2.One;
}
