using Godot;

namespace CrossedDimensions.Entities;

[GlobalClass]
public partial class ProjectileEmitter : Node2D
{
    [Export]
    public PackedScene ProjectileScene { get; set; }

    [Export]
    public Marker2D PointA { get; set; }

    [Export]
    public Marker2D PointB { get; set; }

    [Export]
    public Marker2D EmitPoint { get; set; }

    [Export]
    public Timer EmitTimer { get; set; }

    [Export]
    public float MoveSpeed { get; set; } = 64f;

    [Export]
    public Vector2 EmitDirection { get; set; } = Vector2.Down;

    [Export]
    public Node2D AimTarget { get; set; }

    [Export]
    public AnimationPlayer EmitAnimationPlayer { get; set; }

    [Export]
    public StringName EmitAnimationName { get; set; } = "emit";

    [Export]
    public AudioStreamPlayer2D EmitSound { get; set; }

    [Export]
    public Characters.Character OwnerCharacter { get; set; }

    private int _targetIndex = 1;

    public override void _Ready()
    {
        if (PointA != null)
        {
            GlobalPosition = PointA.GlobalPosition;
        }

        if (EmitTimer != null)
        {
            EmitTimer.Timeout += OnEmitTimerTimeout;
            if (EmitTimer.IsStopped())
            {
                EmitTimer.Start();
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (PointA == null || PointB == null || MoveSpeed <= 0f)
        {
            return;
        }

        var target = _targetIndex == 0 ? PointA.GlobalPosition : PointB.GlobalPosition;
        var toTarget = target - GlobalPosition;
        var maxDistance = MoveSpeed * (float)delta;

        if (toTarget.LengthSquared() <= maxDistance * maxDistance)
        {
            GlobalPosition = target;
            _targetIndex = _targetIndex == 0 ? 1 : 0;
            return;
        }

        GlobalPosition += toTarget.Normalized() * maxDistance;
    }

    private void OnEmitTimerTimeout()
    {
        EmitProjectile();
    }

    public void EmitProjectile()
    {
        if (ProjectileScene == null)
        {
            return;
        }

        var projectile = ProjectileScene.Instantiate<Projectile>();
        var spawnPosition = EmitPoint?.GlobalPosition ?? GlobalPosition;
        var direction = GetEmitDirection(spawnPosition);

        projectile.GlobalPosition = spawnPosition;
        projectile.Direction = direction;
        projectile.OwnerCharacter = OwnerCharacter;

        GetTree().CurrentScene.AddChild(projectile);

        if (EmitAnimationPlayer != null && EmitAnimationPlayer.HasAnimation(EmitAnimationName))
        {
            EmitAnimationPlayer.Play(EmitAnimationName);
        }

        EmitSound?.Play();
    }

    private Vector2 GetEmitDirection(Vector2 spawnPosition)
    {
        if (AimTarget != null)
        {
            return spawnPosition.DirectionTo(AimTarget.GlobalPosition).Normalized();
        }

        return EmitDirection.Rotated(GlobalRotation).Normalized();
    }
}
