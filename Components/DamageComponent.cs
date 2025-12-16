using Godot;

namespace CrossedDimensions.Components;

public partial class DamageComponent : Node
{
    [Export]
    public int DamageAmount { get; set; } = 10;

    [Export]
    public float KnockbackMultiplier { get; set; } = 1f;
}
