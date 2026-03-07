using CrossedDimensions.Components;
using CrossedDimensions.Entities;
using Godot;

namespace CrossedDimensions.States.Weapons.Behaviors;

public partial class FireProjectileBehavior : WeaponState
{
    [Export]
    public PackedScene ProjectileScene { get; set; }

    [Export]
    public Vector2 Offset { get; set; } = new(0, -4);

    /// <summary>
    /// Optional tracker to assign to the spawned projectile's
    /// <see cref="IceCrystalHitHandler"/>, so that frozen components can be
    /// tracked and released later.
    /// </summary>
    [Export]
    public FreezeTrackerComponent FreezeTracker { get; set; }

    public override State Enter(State previousState)
    {
        var projectile = ProjectileScene.Instantiate<Entities.Projectile>();
        projectile.OwnerCharacter = Weapon.OwnerCharacter;
        projectile.GlobalPosition = Weapon.GlobalPosition
            + Offset;
        projectile.Rotation = Weapon.Target.Angle();

        if (FreezeTracker is not null
            && projectile.GetNodeOrNull<IceCrystalHitHandler>("HitHandler") is { } handler)
        {
            handler.FreezeTracker = FreezeTracker;
        }

        // TODO: use a World class to add the projectile to the scene
        projectile.OwnerCharacter.GetParent().AddChild(projectile);

        return null;
    }
}
