using Godot;

namespace CrossedDimensions.BoundingBoxes;

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
    /// Signal emitted when this hurtbox is hit by a hitbox.
    /// </summary>
    [Signal]
    public delegate void HurtboxHitEventHandler(Hitbox hitbox);

    /// <summary>
    /// Applies damage to the owning entity based on the given hitbox.
    /// </summary>
    public void Hit(Hitbox hitbox)
    {
        if (HealthComponent != null && hitbox.DamageComponent != null)
        {
            int damage = hitbox.DamageComponent.DamageAmount;
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

                float falloffFactor = Mathf.Clamp(1 - (distance / maxDistance), 0, 1);
                damage = (int)(damage * falloffFactor);
            }

            // characters can not hit themselves, but knockback will still
            // apply
            Vector2 force = direction.Normalized() * damage * knockback;
            OwnerCharacter.VelocityFromExternalForces += force;

            if (hitbox.OwnerCharacter == OwnerCharacter && OwnerCharacter is not null)
            {
                damage = 0;
            }

            EmitSignal(SignalName.HurtboxHit, hitbox);

            if (damage > 0)
            {
                HealthComponent.CurrentHealth -= damage;
            }
        }
    }
}
