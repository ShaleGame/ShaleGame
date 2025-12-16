using Godot;
using Godot.Collections;

namespace CrossedDimensions.Items;

public partial class Weapon : ItemInstance
{
    [Export]
    public States.StateMachine StateMachine { get; set; }

    public bool IsPrimaryUseHeld
    {
        get => OwnerCharacter?.Controller?.IsMouse1Held ?? false;
    }

    public bool IsSecondaryUseHeld
    {
        get => OwnerCharacter?.Controller?.IsMouse2Held ?? false;
    }

    public Vector2 Target
    {
        get => OwnerCharacter?.Controller?.Target ?? Vector2.Zero;
    }

    public override void _Ready()
    {
        StateMachine.Initialize(this);
    }

    public override void _Process(double delta)
    {
        StateMachine.Process(delta);
    }
}
