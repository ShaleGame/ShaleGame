using Godot;

namespace CrossedDimensions.Extensions;

public static class GpuParticles2DExtensions
{
    public static GpuParticles2D EmitOneShot(this GpuParticles2D particles)
    {
        var newParticles = particles.Duplicate() as GpuParticles2D;
        newParticles.Emitting = true;
        newParticles.OneShot = true;
        particles.GetTree().Root.AddChild(newParticles);
        newParticles.Finished += () => newParticles.QueueFree();
        newParticles.GlobalPosition = particles.GlobalPosition;
        return newParticles;
    }
}
