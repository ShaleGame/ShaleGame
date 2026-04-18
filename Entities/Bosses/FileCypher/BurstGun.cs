using Godot;
using System.Linq;
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
    private Character _waveTarget;
    private bool _targetClone;

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
        _targetClone = false;
        _waveTarget = ResolveWaveTarget();

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
            _targetClone = !_targetClone;
            _waveTarget = ResolveWaveTarget();
            _timer = DelayBetweenBursts;
            return null;
        }

        return _attackIdle;
    }

    private void SpawnBullet()
    {
        if (BulletScene == null || SpawnPoint == null || _boss == null)
        {
            return;
        }

        var bullet = BulletScene.Instantiate<SoftHomingBullet>();
        bullet.GlobalPosition = SpawnPoint.GlobalPosition;
        bullet.Direction = Vector2.Left;
        bullet.OwnerCharacter = _boss;
        bullet.TargetCharacter = _waveTarget;

        GetTree().CurrentScene.AddChild(bullet);
    }

    private Character ResolveWaveTarget()
    {
        var allPlayers = GetTree().GetNodesInGroup("Player").OfType<Character>();

        if (_targetClone)
        {
            return allPlayers.FirstOrDefault(player => player.Cloneable?.IsClone == true)
                ?? allPlayers.FirstOrDefault(player => player.Cloneable?.IsClone == false)
                ?? allPlayers.FirstOrDefault();
        }

        return allPlayers.FirstOrDefault(player => player.Cloneable?.IsClone == false)
            ?? allPlayers.FirstOrDefault();
    }
}
