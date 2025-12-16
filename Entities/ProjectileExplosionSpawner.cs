using Godot;

namespace CrossedDimensions.Entities;

public partial class ProjectileExplosionSpawner : Node, IProjectileHitHandlerComponent
{
    [Export]
    public PackedScene ExplosionScene { get; set; }

    public void OnProjectileHit(Projectile projectile)
    {
        if (ExplosionScene is null)
        {
            return;
        }

        var explosionInstance = ExplosionScene.Instantiate<Projectile>();

        // TODO: use a World class to add the explosion instance
        explosionInstance.GlobalPosition = projectile.GlobalPosition;
        explosionInstance.OwnerCharacter = projectile.OwnerCharacter;

        // defer since the projectile has monitoring set
        projectile.GetParent().CallDeferred("add_child", explosionInstance);
    }
}
