using Godot;
using System.Linq;
using CrossedDimensions.Entities;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class SoftHomingBullet : Projectile
{
    [Export]
    public float HomingStrength { get; set; } = 1f / 128;

    // Maximum angular velocity (radians per second) the bullet can rotate.
    // This is scaled by the lifetime parameter `t` in _PhysicsProcess.
    [Export(PropertyHint.Range, "0,360,degrees")]
    public float MaxAngularVelocity { get; set; } = 90.0f;

    public Character TargetCharacter { get; set; }

    // Homing now scales with the projectile's lifetime timer (time left / wait time).

    public override void _PhysicsProcess(double delta)
    {
        var nearest = GetTargetCharacter();

        if (nearest != null)
        {
            var targetPos = nearest.GlobalPosition;
            var desired = GlobalPosition.DirectionTo(targetPos);

            // scale homing by lifetime timer (time left / wait time)
            // homing is strongest at spawn and weakens over time
            float t = 1f;
            if (LifetimeTimer is not null && LifetimeTimer.WaitTime > 0f)
            {
                float time = (float)(LifetimeTimer.TimeLeft / LifetimeTimer.WaitTime);
                t = Mathf.Clamp(time, 0f, 1f);
            }

            // Rotate the Direction vector toward the desired direction using
            // a maximum angular velocity. The maximum rotation allowed this
            // frame is MaxAngularVelocity * t * delta. Uses vector math
            // (AngleTo + Rotated) to avoid manual angle conversion.
            if (Direction.LengthSquared() == 0f)
            {
                // If direction isn't initialized for some reason, snap to
                // the desired direction.
                Direction = desired;
            }
            else
            {
                // Signed angle from current direction to desired (radians)
                float angleToTarget = Direction.AngleTo(desired);

                // Max rotation allowed this frame (radians)
                float maxRotationThisFrame = Mathf.DegToRad(MaxAngularVelocity) * t * (float)delta;

                // Clamp rotation and apply it
                float rotation = Mathf.Clamp(angleToTarget, -maxRotationThisFrame, maxRotationThisFrame);
                Direction = Direction.Rotated(rotation).Normalized();
            }

            _velocity = Direction * Speed;

            QueueRedraw();
        }

        base._PhysicsProcess(delta);
    }

    public override void _Draw()
    {
        // Draw a line indicating the current direction of the bullet for
        // debugging purposes.
        var lineLength = 16f;
        DrawLine(Vector2.Zero, Direction * lineLength, Colors.Red);
    }

    public override void _Ready()
    {
        // Set initial direction to nearest player at spawn time so the bullet
        // initially aims towards the player before soft-homing adjustments.
        var nearest = GetTargetCharacter();

        if (nearest != null)
        {
            Direction = GlobalPosition.DirectionTo(nearest.GlobalPosition);
        }

        base._Ready();
    }

    private Character GetTargetCharacter()
    {
        if (TargetCharacter != null && IsInstanceValid(TargetCharacter))
        {
            return TargetCharacter;
        }

        return TargetCharacter = GetTree()
            .GetNodesInGroup("Player")
            .OfType<Character>()
            .OrderBy(player => GlobalPosition.DistanceSquaredTo(player.GlobalPosition))
            .FirstOrDefault();
    }
}
