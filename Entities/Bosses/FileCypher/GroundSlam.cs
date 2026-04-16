using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using CrossedDimensions.BoundingBoxes;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class GroundSlam : State
{
    [Export]
    public Hitbox GroundSlamHitbox { get; set; }

    [Export]
    public CollisionShape2D GroundSlamCollisionShape { get; set; }

    [Export]
    public double WindupTime { get; set; } = 0.8;

    [Export]
    public double ActiveTime { get; set; } = 0.1;

    [Export]
    public double RecoveryTime { get; set; } = 0.5;

    private Character _boss;
    private AnimationPlayer _anim;
    private State _attackIdle;

    private int _phase;
    private double _timer;

    public override State Enter(State previousState)
    {
        _boss = Context as Character;
        _anim = _boss?.GetNode<AnimationPlayer>("AnimationPlayer");
        _attackIdle = GetParent().GetNode<State>("AttackIdle");

        if (GroundSlamHitbox != null)
        {
            GroundSlamHitbox.OwnerCharacter = _boss;
            GroundSlamHitbox.Monitoring = false;
            GroundSlamHitbox.Monitorable = false;
        }

        if (GroundSlamCollisionShape != null)
        {
            GroundSlamCollisionShape.Disabled = true;
        }

        _phase = 0;
        _timer = WindupTime;

        _anim?.Play("GroundSlamWindup");

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        _timer -= delta;
        if (_timer > 0)
        {
            return null;
        }

        if (_phase == 0)
        {
            _phase = 1;
            _timer = ActiveTime;
            _anim?.Play("GroundSlamImpact");

            if (GroundSlamHitbox != null)
            {
                GroundSlamHitbox.Monitoring = true;
                GroundSlamHitbox.Monitorable = true;
            }

            if (GroundSlamCollisionShape != null)
            {
                GroundSlamCollisionShape.Disabled = false;
            }

            return null;
        }

        if (_phase == 1)
        {
            _phase = 2;
            _timer = RecoveryTime;

            if (GroundSlamHitbox != null)
            {
                GroundSlamHitbox.Monitoring = false;
                GroundSlamHitbox.Monitorable = false;
            }

            if (GroundSlamCollisionShape != null)
            {
                GroundSlamCollisionShape.Disabled = true;
            }

            _anim?.Play("Phase1Idle");

            return null;
        }

        return _attackIdle;
    }
}
