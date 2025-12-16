namespace CrossedDimensions.Entities;

/// <summary>
/// Interface for handling projectile hit events, such as for custom logic.
/// </summary>
public interface IProjectileHitHandlerComponent
{
    public void OnProjectileHit(Projectile projectile);
}
