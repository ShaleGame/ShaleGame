using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using CrossedDimensions.BoundingBoxes;
using System.Linq;

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

    [Export]
    public float LiftHeight { get; set; } = 250f;

    [Export]
    public float RiseSpeed { get; set; } = 520f;

    [Export]
    public float HorizontalTrackingSpeed { get; set; } = 720f;

    [Export]
    public float SlamSpeed { get; set; } = 1200f;

    [Export]
    public float GroundSnapTolerance { get; set; } = 4f;

    private Character _boss;
    private AnimationPlayer _anim;
    private State _attackIdle;

    private int _phase;
    private double _timer;
    private float _groundY;
    private float _targetX;

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

        _groundY = _boss?.GlobalPosition.Y ?? 0f;
        _targetX = GetTargetX();

        _anim?.Play("GroundSlamWindup");

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (_phase == 1)
        {
            return null;
        }

        _timer -= delta;
        if (_timer > 0)
        {
            return null;
        }

        if (_phase == 0)
        {
            _phase = 1;
            return null;
        }

        if (_phase == 2)
        {
            _phase = 3;
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

        if (_phase == 3)
        {
            return _attackIdle;
        }

        return null;
    }

    public override State PhysicsProcess(double delta)
    {
        if (_boss == null)
        {
            return null;
        }

        _boss.Velocity = Vector2.Zero;
        _boss.VelocityFromInput = Vector2.Zero;
        _boss.VelocityFromExternalForces = Vector2.Zero;

        var position = _boss.GlobalPosition;

        if (_phase == 0)
        {
            _targetX = GetTargetX();
            var airY = _groundY - LiftHeight;
            position.X = Mathf.MoveToward(position.X, _targetX, HorizontalTrackingSpeed * (float)delta);
            position.Y = Mathf.MoveToward(position.Y, airY, RiseSpeed * (float)delta);
            _boss.GlobalPosition = position;
        }
        else if (_phase == 1)
        {
            position.X = Mathf.MoveToward(position.X, _targetX, HorizontalTrackingSpeed * (float)delta);
            position.Y = Mathf.MoveToward(position.Y, _groundY, SlamSpeed * (float)delta);
            _boss.GlobalPosition = position;

            if (Mathf.Abs(position.Y - _groundY) <= GroundSnapTolerance)
            {
                _boss.GlobalPosition = new Vector2(position.X, _groundY);
                TriggerImpact();
            }
        }

        return null;
    }

    public override void Exit(State nextState)
    {
        if (GroundSlamHitbox != null)
        {
            GroundSlamHitbox.Monitoring = false;
            GroundSlamHitbox.Monitorable = false;
        }

        if (GroundSlamCollisionShape != null)
        {
            GroundSlamCollisionShape.Disabled = true;
        }

        base.Exit(nextState);
    }

    private float GetTargetX()
    {
        if (_boss == null)
        {
            return 0f;
        }

        var nearestPlayer = GetTree().GetNodesInGroup("Player")
            .OfType<Character>()
            .OrderBy(player => _boss.GlobalPosition.DistanceTo(player.GlobalPosition))
            .FirstOrDefault();

        return nearestPlayer?.GlobalPosition.X ?? _boss.GlobalPosition.X;
    }

    private void TriggerImpact()
    {
        if (_phase != 1)
        {
            return;
        }

        _phase = 2;
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
    }
}
