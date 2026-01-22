using Godot;

namespace CrossedDimensions.BoundingBoxes;

/// <summary>
/// A hurtbox that can receive damage from hitboxes. Can be attached to
/// entities with health and apply damage and knockback when hit.
/// </summary>
public partial class Hurtbox : BoundingBox
{
    /// <summary>
    /// The health component of the entity that owns this hurtbox.
    /// </summary>
    [Export]
    public Components.HealthComponent HealthComponent { get; set; }

    /// <summary>
    /// The character that owns this hurtbox, if any.
    /// </summary>
    [Export]
    public Characters.Character OwnerCharacter { get; set; }

    /// <summary>
    /// Optional timer used to provide invulnerability frames after being hit.
    /// When running, subsequent hits are ignored until the timer expires.
    /// </summary>
    [Export]
    public Timer IFrameTimer { get; set; }

    /// <summary>
    /// Signal emitted when this hurtbox is hit by a hitbox.
    /// </summary>
    [Signal]
    public delegate void HurtboxHitEventHandler(Hitbox hitbox, float damage);

    /// <summary>
    /// Calculate the damage falloff factor based on distance and a maximum
    /// distance. Returns a value clamped between 0 and 1.
    /// Extracted to allow unit testing of falloff logic without needing
    /// collision shapes or Area integration.
    /// </summary>
    public static float CalculateFalloffFactor(float distance, float maxDistance)
    {
        if (maxDistance <= 0f)
        {
            return 0f;
        }

        return Mathf.Clamp(1f - (distance / maxDistance), 0f, 1f);
    }

    /// <summary>
    /// Applies damage to the owning entity based on the given hitbox.
    /// Returns true if the hit registered (i.e. damage or knockback was applied),
    /// false if the hit was ignored (for example during invulnerability frames).
    /// </summary>
    public bool Hit(Hitbox hitbox)
    {
        // if an iframe timer is provided and running, ignore the hit
        if (IFrameTimer is not null && !IFrameTimer.IsStopped())
        {
            return false;
        }

        int damage = hitbox.DamageComponent.DamageAmount;

        if (OwnerCharacter is not null)
        {
            float knockback = hitbox.DamageComponent.KnockbackMultiplier;
            Vector2 hurtboxCenter = GlobalPosition;
            Vector2 hitboxCenter = hitbox.GlobalPosition;
            Vector2 direction = hurtboxCenter - hitboxCenter;

            if (hitbox.FalloffWithDistance)
            {
                float distance = direction.Length();
                float maxDistance = hitbox.GetNode<CollisionShape2D>("CollisionShape2D")
                    .Shape
                    .GetRect()
                    .Size
                    .Length() / 2;

                float falloffFactor = CalculateFalloffFactor(distance, maxDistance);
                damage = (int)(damage * falloffFactor);
            }

            // characters can not hit themselves, but knockback will still
            // apply
            Vector2 force = direction.Normalized() * damage * knockback;

            // apply knockback if non-zero force
            if (force != Vector2.Zero)
            {
                OwnerCharacter.VelocityFromExternalForces += force;
            }

            // do not apply damage if the hitbox belongs to the same character
            if (hitbox.OwnerCharacter == OwnerCharacter && OwnerCharacter is not null)
            {
                damage = 0;
            }
        }

        // apply damage to health component
        if (damage > 0 && HealthComponent is not null)
        {
            HealthComponent.CurrentHealth -= damage;
        }

        // if an iframe timer is provided, (re)start it to grant temporary invulnerability
        if (IFrameTimer is not null)
        {
            IFrameTimer.Start();
        }

        EmitSignal(SignalName.HurtboxHit, hitbox, damage);
        return true;
    }
}
