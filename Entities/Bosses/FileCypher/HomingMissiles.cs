using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class HomingMissiles : State
{
    [Export]
    public PackedScene MissileScene { get; set; }

    [Export]
    public Marker2D BombRunStart { get; set; }

    [Export]
    public Marker2D BombRunEnd { get; set; }

    [Export]
    public Marker2D BombRunMarker { get; set; }

    [Export]
    public double BombRunDuration { get; set; } = 2.0;

    [Export]
    public double SpawnInterval { get; set; } = 0.16;

    [Export]
    public double RecoveryTime { get; set; } = 0.35;

    private Character _boss;
    private AnimationPlayer _anim;
    private State _attackIdle;

    private double _elapsed;
    private double _spawnTimer;
    private bool _runFinished;
    private double _recoveryRemaining;

    public override State Enter(State previousState)
    {
        _boss = Context as Character;
        _anim = _boss?.GetNode<AnimationPlayer>("AnimationPlayer");
        _attackIdle = GetParent().GetNode<State>("AttackIdle");

        _elapsed = 0;
        _spawnTimer = 0;
        _runFinished = false;
        _recoveryRemaining = RecoveryTime;

        _anim?.Play("HomingMissiles");

        if (BombRunStart != null && BombRunMarker != null)
        {
            BombRunMarker.GlobalPosition = BombRunStart.GlobalPosition;
        }

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (MissileScene == null || BombRunStart == null || BombRunEnd == null || BombRunMarker == null)
        {
            return _attackIdle;
        }

        if (!_runFinished)
        {
            _elapsed += delta;

            float t = 1.0f;
            if (BombRunDuration > 0)
            {
                t = Mathf.Clamp((float)(_elapsed / BombRunDuration), 0f, 1f);
            }

            BombRunMarker.GlobalPosition = BombRunStart.GlobalPosition.Lerp(BombRunEnd.GlobalPosition, t);

            _spawnTimer += delta;
            while (_spawnTimer >= SpawnInterval)
            {
                _spawnTimer -= SpawnInterval;
                SpawnBombMissile();
            }

            if (_elapsed >= BombRunDuration)
            {
                _runFinished = true;
            }

            return null;
        }

        _recoveryRemaining -= delta;
        if (_recoveryRemaining <= 0)
        {
            return _attackIdle;
        }

        return null;
    }

    private void SpawnBombMissile()
    {
        if (_boss == null || MissileScene == null || BombRunMarker == null)
        {
            return;
        }

        var direction = Vector2.Down;
        var missile = MissileScene.Instantiate<StripeMissile>();
        missile.GlobalPosition = BombRunMarker.GlobalPosition;
        missile.Direction = direction;
        missile.OwnerCharacter = _boss;
        missile.Rotation = direction.Angle() - Mathf.Pi / 2f;

        GetTree().CurrentScene.AddChild(missile);
    }
}
