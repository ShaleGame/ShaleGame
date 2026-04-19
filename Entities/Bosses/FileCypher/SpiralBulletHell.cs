using Godot;
using CrossedDimensions.Entities;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class SpiralBulletHell : State
{
    [Export]
    public PackedScene ProjectileScene { get; set; }

    [Export]
    public Marker2D LeftSweepStart { get; set; }

    [Export]
    public Marker2D LeftSweepEnd { get; set; }

    [Export]
    public Marker2D LeftSweepMarker { get; set; }

    [Export]
    public Marker2D RightSweepStart { get; set; }

    [Export]
    public Marker2D RightSweepEnd { get; set; }

    [Export]
    public Marker2D RightSweepMarker { get; set; }

    [Export]
    public double SweepDuration { get; set; } = 2.0;

    [Export]
    public double DelayBetweenSweeps { get; set; } = 0.0;

    [Export]
    public double SpawnInterval { get; set; } = 0.125;

    [Export]
    public float BulletSpeed { get; set; } = 180f;

    [Export]
    public float DamageThreshold { get; set; } = 750f;

    private Character _boss;
    private AnimationPlayer _anim;
    private State _attackIdle;
    private double _elapsed;
    private double _spawnTimer;
    private float _damageTaken;

    public override State Enter(State previousState)
    {
        _boss = Context as Character;
        _anim = _boss?.GetNode<AnimationPlayer>("AnimationPlayer");
        _attackIdle = GetParent().GetNode<State>("AttackIdle");

        _elapsed = 0d;
        _spawnTimer = 0d;
        _damageTaken = 0f;

        _anim?.Play("SpiralBulletHell");

        if (LeftSweepStart != null && LeftSweepMarker != null)
        {
            LeftSweepMarker.GlobalPosition = LeftSweepStart.GlobalPosition;
        }

        if (RightSweepStart != null && RightSweepMarker != null)
        {
            RightSweepMarker.GlobalPosition = RightSweepStart.GlobalPosition;
        }

        if (_boss?.Health != null)
        {
            _boss.Health.HealthChanged += OnHealthChanged;
        }

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (_boss == null || ProjectileScene == null)
        {
            return _attackIdle;
        }

        if (LeftSweepStart == null || LeftSweepEnd == null || LeftSweepMarker == null)
        {
            return _attackIdle;
        }

        if (RightSweepStart == null || RightSweepEnd == null || RightSweepMarker == null)
        {
            return _attackIdle;
        }

        if (_damageTaken >= DamageThreshold)
        {
            return _attackIdle;
        }

        _elapsed += delta;

        double cycleDuration = SweepDuration + Mathf.Max(0.0, DelayBetweenSweeps);
        double cycleElapsed = _elapsed;
        if (cycleDuration > 0d)
        {
            cycleElapsed %= cycleDuration;
        }

        bool isSweeping = SweepDuration <= 0d || cycleElapsed < SweepDuration;
        float t = 1f;

        if (isSweeping && SweepDuration > 0d)
        {
            t = (float)(cycleElapsed / SweepDuration);
        }

        LeftSweepMarker.GlobalPosition = LeftSweepStart.GlobalPosition.Lerp(LeftSweepEnd.GlobalPosition, t);
        RightSweepMarker.GlobalPosition = RightSweepStart.GlobalPosition.Lerp(RightSweepEnd.GlobalPosition, t);

        if (!isSweeping)
        {
            _spawnTimer = 0d;
            return null;
        }

        _spawnTimer += delta;
        while (_spawnTimer >= SpawnInterval)
        {
            FireTick();
            _spawnTimer -= SpawnInterval;
        }

        return null;
    }

    public override void Exit(State nextState)
    {
        if (_boss?.Health != null)
        {
            _boss.Health.HealthChanged -= OnHealthChanged;
        }

        base.Exit(nextState);
    }

    private void FireTick()
    {
        SpawnBullet(LeftSweepMarker.GlobalPosition, Vector2.Left);
        SpawnBullet(RightSweepMarker.GlobalPosition, Vector2.Right);
    }

    private void SpawnBullet(Vector2 position, Vector2 direction)
    {
        var projectile = ProjectileScene.Instantiate<Projectile>();
        projectile.GlobalPosition = position;
        projectile.Direction = Vector2.Up;
        projectile.Speed = BulletSpeed;
        projectile.Rotation = direction.Angle() - Mathf.Pi / 2f;
        projectile.OwnerCharacter = _boss;

        GetTree().CurrentScene.AddChild(projectile);
    }

    private void OnHealthChanged(int oldHealth)
    {
        if (_boss?.Health == null)
        {
            return;
        }

        int delta = oldHealth - _boss.Health.CurrentHealth;
        if (delta > 0)
        {
            _damageTaken += delta;
        }
    }
}
