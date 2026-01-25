using Godot;

namespace CrossedDimensions.Entities.Enemies.Turret;

public partial class TurretAttackPattern : AttackPattern
{
    [Export(PropertyHint.File, "*.tscn")]
    public string BulletScenePath { get; set; } = "res://Entities/Enemies/Turret/Bullet.tscn";

    private PackedScene _bullet;

    public override void _Ready()
    {
        if (!string.IsNullOrWhiteSpace(BulletScenePath))
        {
            _bullet = GD.Load<PackedScene>(BulletScenePath);
        }
    }

    public override void ExecuteAttack(Vector2 origin, Vector2 target, float angle)
    {
        if (_bullet is null)
        {
            return;
        }

        var bullet = _bullet.Instantiate<Node2D>();
        bullet.Rotation = angle;
        AddChild(bullet);
        bullet.GlobalPosition = origin;
    }
}
