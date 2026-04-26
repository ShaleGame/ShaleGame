using Godot;
using CrossedDimensions.Entities;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class BulletHell : State
{
    [Export]
    public PackedScene ProjectileScene { get; set; }

    [Export]
    public Node2D SpawnPoint { get; set; }

    [Export]
    public int ProjectilesPerRing { get; set; } = 16;

    [Export]
    public int RingCount { get; set; } = 3;

    [Export]
    public double DelayBetweenRings { get; set; } = 0.25;

    [Export]
    public float RingAngleOffsetDegrees { get; set; } = 12f;

    private Character _boss;
    private State _attackIdle;

    private int _ringIndex;
    private double _timer;

    public override State Enter(State previousState)
    {
        _boss = Context as Character;
        _attackIdle = GetParent().GetNode<State>("AttackIdle");

        _ringIndex = 0;
        _timer = 0;

        var anim = _boss?.GetNode<AnimationPlayer>("AnimationPlayer");
        anim?.Play("BulletHell");

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        _timer -= delta;
        if (_timer > 0)
        {
            return null;
        }

        FireRing();
        _ringIndex++;

        if (_ringIndex >= RingCount)
        {
            return _attackIdle;
        }

        _timer = DelayBetweenRings;

        return null;
    }

    private void FireRing()
    {
        if (_boss == null || ProjectileScene == null || SpawnPoint == null || ProjectilesPerRing <= 0)
        {
            return;
        }

        float offset = Mathf.DegToRad(_ringIndex * RingAngleOffsetDegrees);

        for (int i = 0; i < ProjectilesPerRing; i++)
        {
            float angle = offset + (Mathf.Tau / ProjectilesPerRing) * i;
            Vector2 direction = Vector2.Right.Rotated(angle);

            var projectile = ProjectileScene.Instantiate<Projectile>();
            projectile.GlobalPosition = SpawnPoint.GlobalPosition;
            projectile.Direction = direction;
            projectile.Rotation = direction.Angle() - Mathf.Pi / 2f;
            projectile.OwnerCharacter = _boss;

            GetTree().CurrentScene.AddChild(projectile);
        }
    }
}
