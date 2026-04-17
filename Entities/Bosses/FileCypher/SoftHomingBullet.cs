using Godot;
using System.Linq;
using CrossedDimensions.Entities;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class SoftHomingBullet : Projectile
{
    [Export]
    public float HomingStrength { get; set; } = 512f;

    // Homing now scales with the projectile's lifetime timer (time left / wait time).

    public override void _PhysicsProcess(double delta)
    {
        var nearest = GetTree()
            .GetNodesInGroup("Player")
            .OfType<Character>()
            .OrderBy(p => GlobalPosition.DistanceSquaredTo(p.GlobalPosition))
            .FirstOrDefault();

        if (nearest != null)
        {
            Vector2 targetDirection = GlobalPosition.DirectionTo(nearest.GlobalPosition);

            // If a LifetimeTimer is present and has a positive WaitTime, scale homing
            // strength by the proportion of time left. Otherwise use full strength.
            float timeScale = 1f;
            if (LifetimeTimer is not null && LifetimeTimer.WaitTime > 0f)
            {
                timeScale = Mathf.Clamp((float)LifetimeTimer.TimeLeft / (float)LifetimeTimer.WaitTime, 0f, 1f);
            }

            float effectiveHoming = HomingStrength * timeScale;
            _velocity = _velocity.MoveToward(targetDirection * Speed, effectiveHoming * (float)delta);
        }

        base._PhysicsProcess(delta);
    }

    public override void _Ready()
    {
        // Set initial direction to nearest player at spawn time so the bullet
        // initially aims towards the player before soft-homing adjustments.
        var nearest = GetTree()
            .GetNodesInGroup("Player")
            .OfType<Character>()
            .OrderBy(p => GlobalPosition.DistanceSquaredTo(p.GlobalPosition))
            .FirstOrDefault();

        if (nearest != null)
        {
            Rotation = GlobalPosition.AngleTo(nearest.GlobalPosition);
        }

        base._Ready();
    }
}
