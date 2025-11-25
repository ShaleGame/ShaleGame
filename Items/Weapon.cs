using Godot;

namespace CrossDimensions.Items;

public partial class Weapon : ItemInstance
{
    [Export]
    public States.Weapons.WeaponStateMachine StateMachine { get; set; }

    public bool IsPrimaryUseHeld
    {
        get => OwnerCharacter?.Controller?.IsMouse1Held ?? false;
    }

    public bool IsSecondaryUseHeld
    {
        get => OwnerCharacter?.Controller?.IsMouse2Held ?? false;
    }

    public override void _Ready()
    {
        StateMachine.Initialize(this);
    }
}
