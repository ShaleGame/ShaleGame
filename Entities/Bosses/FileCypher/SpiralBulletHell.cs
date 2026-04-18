using Godot;
using CrossedDimensions.Entities;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using CrossedDimensions.Environment.Triggers;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class SpiralBulletHell : State
{
    [Export]
    public PackedScene ProjectileScene { get; set; }

    [Export]
    public Trigger TopSwitch { get; set; }

    [Export]
    public Trigger BottomSwitch { get; set; }

    [Export]
    public float BaseRotationSpeed { get; set; } = 1.2f;

    [Export]
    public float BaseFireInterval { get; set; } = 0.08f;

    [Export]
    public int Arms { get; set; } = 3;

    [Export]
    public float BulletSpeed { get; set; } = 180f;

    [Export]
    public float DamageThreshold { get; set; } = 750f;

    private Character _boss;
    private AnimationPlayer _anim;
    private State _attackIdle;
    private float _angle;
    private double _fireTimer;
    private float _damageTaken;

    public override State Enter(State previousState)
    {
        _boss = Context as Character;
        _anim = _boss?.GetNode<AnimationPlayer>("AnimationPlayer");
        _attackIdle = GetParent().GetNode<State>("AttackIdle");

        _angle = 0f;
        _fireTimer = 0d;
        _damageTaken = 0f;

        _anim?.Play("SpiralBulletHell");

        if (_boss?.Health != null)
        {
            _boss.Health.HealthChanged += OnHealthChanged;
        }

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (_boss == null || ProjectileScene == null || Arms <= 0)
        {
            return _attackIdle;
        }

        if (_damageTaken >= DamageThreshold)
        {
            return _attackIdle;
        }

        float rotationSpeed = BaseRotationSpeed;
        double fireInterval = BaseFireInterval;

        if (TopSwitch?.IsActive == true || BottomSwitch?.IsActive == true)
        {
            fireInterval = BaseFireInterval / 0.45f;
        }

        _fireTimer -= delta;
        while (_fireTimer <= 0d)
        {
            FireTick();
            _fireTimer += fireInterval;
        }

        _angle += rotationSpeed * (float)delta;

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
        for (int i = 0; i < Arms; i++)
        {
            float angle = _angle + (Mathf.Tau / Arms) * i;
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
            SpawnBullet(direction);
        }
    }

    private void SpawnBullet(Vector2 direction)
    {
        var projectile = ProjectileScene.Instantiate<Projectile>();
        projectile.GlobalPosition = _boss.GlobalPosition;
        projectile.Direction = direction;
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
