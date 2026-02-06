using CrossedDimensions.Components;
using CrossedDimensions.Extensions;
using Godot;

namespace CrossedDimensions.Entities;

public partial class IceCrystalHitHandler : Node, IProjectileHitHandlerComponent
{
    public void OnProjectileHit(Projectile projectile, BoundingBoxes.Hurtbox hurtbox)
    {
        if (hurtbox?.OwnerCharacter is Characters.Character chr)
        {
            if (chr.HasNode<FreezableComponent>("FreezableComponent", out var f))
            {
                f.Freeze(15f);
            }
        }
    }
}
