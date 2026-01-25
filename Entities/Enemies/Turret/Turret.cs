using Godot;

namespace CrossedDimensions.Entities.Enemies.Turret;

public partial class Turret : CharacterBody2D
{
    [Export]
    public Node2D TurretAxis { get; set; }

    [Export]
    public Node2D BulletSpawnPoint { get; set; }

    [Export]
    public AttackPattern AttackPattern { get; set; }

    [Export]
    public float AggroRange { get; set; } = 300f;

    private bool _reachPlayer;
    private float _axisAngle;
    private Node2D _player;
    private RayCast2D _raycast;

    public override void _Ready()
    {
        _player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
        _raycast = new RayCast2D
        {
            CollisionMask = (1 << 0) | (1 << 1)
        };
        AddChild(_raycast);
    }

    public override void _Process(double delta)
    {
        if (_player is null || TurretAxis is null)
        {
            return;
        }

        _axisAngle = GetAngleTo(_player.GlobalPosition) - Mathf.DegToRad(90f);

        if (_axisAngle > Mathf.DegToRad(60f) || _axisAngle < Mathf.DegToRad(-60f))
        {
            _reachPlayer = false;
        }
        else
        {
            _reachPlayer = true;
        }

        _raycast.GlobalPosition = TurretAxis.GlobalPosition;
        var dir = (_player.GlobalPosition - TurretAxis.GlobalPosition).Normalized();
        _raycast.TargetPosition = dir * AggroRange;
        _raycast.Enabled = true;
        _raycast.ForceRaycastUpdate();

        if (_raycast.IsColliding())
        {
            var collider = _raycast.GetCollider() as Node;
            if (collider is not null && collider.IsInGroup("Player"))
            {
                _reachPlayer = true;
            }
            else
            {
                _reachPlayer = false;
            }
        }
        else
        {
            _reachPlayer = false;
        }

        _axisAngle = Mathf.Clamp(_axisAngle, Mathf.DegToRad(-60f), Mathf.DegToRad(60f));
        TurretAxis.Rotation = _axisAngle;
    }

    public void OnBulletTimerTimeout()
    {
        if (_reachPlayer && AttackPattern is not null && BulletSpawnPoint is not null && _player is not null)
        {
            AttackPattern.ExecuteAttack(BulletSpawnPoint.GlobalPosition, _player.GlobalPosition, _axisAngle);
        }
    }
}
