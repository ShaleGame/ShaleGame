using CrossedDimensions.Components;
using CrossedDimensions.Extensions;
using Godot;

namespace CrossedDimensions.Entities;

public partial class IceCrystalHitHandler : Node, IProjectileHitHandlerComponent
{
    [Export]
    public FreezeTrackerComponent FreezeTracker { get; set; }

    public void OnProjectileHit(Projectile projectile, BoundingBoxes.Hurtbox hurtbox)
    {
        if (hurtbox?.Freezable is not null)
        {
            hurtbox.Freezable.Freeze(15f);
            FreezeTracker?.TrackFrozen(hurtbox.Freezable);
        }
    }
}
