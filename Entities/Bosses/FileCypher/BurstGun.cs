using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class BurstGun : State
{
    [Export]
    public PackedScene BulletScene { get; set; }

    [Export]
    public Node2D SpawnPoint { get; set; }

    [Export]
    public int BurstsPerAttack { get; set; } = 3;

    [Export]
    public int ShotsPerBurst { get; set; } = 3;

    [Export]
    public double DelayBetweenShots { get; set; } = 0.15;

    [Export]
    public double DelayBetweenBursts { get; set; } = 0.6;

    private Character _boss;
    private AnimationPlayer _anim;
    private State _attackIdle;

    private int _burstIndex;
    private int _shotIndex;
    private double _timer;

    public override State Enter(State previousState)
    {
        _boss = Context as Character;
        _anim = _boss?.GetNode<AnimationPlayer>("AnimationPlayer");
        _attackIdle = GetParent().GetNode<State>("AttackIdle");

        _burstIndex = 0;
        _shotIndex = 0;
        _timer = 0;

        _anim?.Play("BurstGun");

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        _timer -= delta;
        if (_timer > 0)
        {
            return null;
        }

        SpawnBullet();

        _shotIndex++;
        if (_shotIndex < ShotsPerBurst)
        {
            _timer = DelayBetweenShots;
            return null;
        }

        _shotIndex = 0;
        _burstIndex++;
        if (_burstIndex < BurstsPerAttack)
        {
            _timer = DelayBetweenBursts;
            return null;
        }

        return _attackIdle;
    }

    private void SpawnBullet()
    {
        GD.Print("Attempting to spawn bullet... bullet scene: " + (BulletScene != null) + ", spawn point: " + (SpawnPoint != null) + ", boss: " + (_boss != null));
        if (BulletScene == null || SpawnPoint == null || _boss == null)
        {
            return;
        }

        var bullet = BulletScene.Instantiate<SoftHomingBullet>();
        bullet.GlobalPosition = SpawnPoint.GlobalPosition;
        bullet.Direction = Vector2.Left;
        bullet.OwnerCharacter = _boss;

        GD.Print($"Spawning bullet at {bullet.GlobalPosition}");

        GetTree().CurrentScene.AddChild(bullet);
    }
}
