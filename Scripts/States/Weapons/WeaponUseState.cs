using Godot;

namespace CrossDimensions.States.Weapons;

public partial class WeaponUseState : WeaponState
{
    public WeaponState NextState { get; set; }

    public override State Enter(State previousState)
    {
        return base.Enter(previousState);
    }

    public override State PrimaryUseEnd()
    {
        return base.PrimaryUseEnd();
    }
}
