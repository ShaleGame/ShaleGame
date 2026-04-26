using Godot;
using CrossedDimensions.Entities;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using CrossedDimensions.Environment.Triggers;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class SwitchPressure : State
{
    [Export]
    public PackedScene ProjectileScene { get; set; }

    [Export]
    public Marker2D TopTarget { get; set; }

    [Export]
    public Marker2D BottomTarget { get; set; }

    [Export]
    public Trigger TopSwitch { get; set; }

    [Export]
    public Trigger BottomSwitch { get; set; }

    [Export]
    public float Duration { get; set; } = 4f;

    [Export]
    public double VolleyInterval { get; set; } = 0.2;

    [Export]
    public int ShotsPerTargetPerVolley { get; set; } = 2;

    [Export]
    public float SpreadAngleDegrees { get; set; } = 16f;

    [Export]
    public float BulletSpeed { get; set; } = 260f;

    private Character _boss;
    private State _attackIdle;
    private float _timeLeft;
    private double _volleyTimer;

    public override State Enter(State previousState)
    {
        _boss = Context as Character;
        _attackIdle = GetParent().GetNode<State>("AttackIdle");
        _timeLeft = Duration;
        _volleyTimer = 0d;

        var anim = _boss?.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        anim?.Play("SwitchPressure");

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (_boss == null || ProjectileScene == null)
        {
            return _attackIdle;
        }

        _timeLeft -= (float)delta;
        if (_timeLeft <= 0f)
        {
            return _attackIdle;
        }

        double interval = VolleyInterval;
        if (TopSwitch?.IsActive == true || BottomSwitch?.IsActive == true)
        {
            interval = VolleyInterval / 0.45f;
        }

        _volleyTimer -= delta;
        while (_volleyTimer <= 0d)
        {
            FireAtTarget(TopTarget);
            FireAtTarget(BottomTarget);
            _volleyTimer += interval;
        }

        return null;
    }

    private void FireAtTarget(Marker2D target)
    {
        if (target == null || ShotsPerTargetPerVolley <= 0)
        {
            return;
        }

        Vector2 toTarget = _boss.GlobalPosition.DirectionTo(target.GlobalPosition);
        float spreadRad = Mathf.DegToRad(SpreadAngleDegrees);

        for (int i = 0; i < ShotsPerTargetPerVolley; i++)
        {
            float t = ShotsPerTargetPerVolley == 1
                ? 0f
                : (float)i / (ShotsPerTargetPerVolley - 1);
            float offset = Mathf.Lerp(-spreadRad * 0.5f, spreadRad * 0.5f, t);
            Vector2 dir = toTarget.Rotated(offset).Normalized();

            var projectile = ProjectileScene.Instantiate<Projectile>();
            projectile.GlobalPosition = _boss.GlobalPosition;
            projectile.Direction = dir;
            projectile.Speed = BulletSpeed;
            projectile.Rotation = dir.Angle() - Mathf.Pi / 2f;
            projectile.OwnerCharacter = _boss;
            GetTree().CurrentScene.AddChild(projectile);
        }
    }
}
