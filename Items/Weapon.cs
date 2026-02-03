using Godot;

namespace CrossedDimensions.Items;

public partial class Weapon : ItemInstance
{
    [Export]
    public States.StateMachine StateMachine { get; set; }

    public bool IsPrimaryUseHeld
    {
        get => IsEquipped && (OwnerCharacter?.Controller?.IsMouse1Held ?? false);
    }

    public bool IsSecondaryUseHeld
    {
        get => IsEquipped && (OwnerCharacter?.Controller?.IsMouse2Held ?? false);
    }

    public Vector2 Target
    {
        get => OwnerCharacter?.Controller?.Target ?? Vector2.Zero;
    }

    public bool IsEquipped
        => OwnerCharacter?.Inventory?.IsWeaponEquipped(this) ?? true;

    public override void _Ready()
    {
        StateMachine.Initialize(this);
    }

    public override void _Process(double delta)
    {
        StateMachine.Process(delta);
    }
}
