using CrossedDimensions.Extensions;
using CrossedDimensions.BoundingBoxes;
using Godot;

namespace CrossedDimensions.Entities;

public partial class ProjectileEffectsPlayer : Node, IProjectileHitHandlerComponent
{
    [Export]
    public AudioStreamPlayer2D Sound { get; set; }

    [Export]
    public GpuParticles2D Particles { get; set; }

    public void OnProjectileHit(Projectile projectile, Hurtbox hurtbox)
    {
        if (Sound != null)
        {
            Sound.PlayOneShot();
        }

        if (Particles != null)
        {
            Particles.EmitOneShot();
        }
    }
}
