using Godot;
using CrossedDimensions.Characters;
using CrossedDimensions.Components;
using CrossedDimensions.BoundingBoxes;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class AnchorClone : Character
{
    [Signal]
    public delegate void CloneDiedEventHandler();

    [Export]
    public HealthComponent CloneHealth { get; set; }

    [Export]
    public Hurtbox CloneHurtbox { get; set; }

    [Export]
    public Hitbox LaserHitbox { get; set; }

    [Export]
    public CollisionShape2D LaserShape { get; set; }

    [Export]
    public Node2D LaserPivot { get; set; }

    [Export]
    public float SweepArcDegrees { get; set; } = 100f;

    [Export]
    public float SweepSpeedDegrees { get; set; } = 40f;

    [Export]
    public double WarmupTime { get; set; } = 0.5;

    private bool _sweepingForward = true;
    private double _warmupRemaining;

    public void Initialize(Character boss, int health)
    {
        CloneHealth?.SetStats(health, health);
    }

    public override void _Ready()
    {
        base._Ready();

        if (CloneHealth != null)
        {
            CloneHealth.HealthChanged += OnHealthChanged;
        }

        if (LaserHitbox != null)
        {
            LaserHitbox.OwnerCharacter = this;
            LaserHitbox.Monitoring = false;
            LaserHitbox.Monitorable = false;
        }

        if (LaserShape != null)
        {
            LaserShape.Disabled = true;
        }

        _warmupRemaining = WarmupTime;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (_warmupRemaining > 0)
        {
            _warmupRemaining -= delta;

            if (_warmupRemaining <= 0)
            {
                if (LaserHitbox != null)
                {
                    LaserHitbox.Monitoring = true;
                    LaserHitbox.Monitorable = true;
                }

                if (LaserShape != null)
                {
                    LaserShape.Disabled = false;
                }
            }

            return;
        }

        if (LaserPivot == null)
        {
            return;
        }

        float halfArc = SweepArcDegrees * 0.5f;
        float nextRotation = LaserPivot.RotationDegrees + SweepSpeedDegrees * (float)delta * (_sweepingForward ? 1f : -1f);

        if (nextRotation >= halfArc)
        {
            nextRotation = halfArc;
            _sweepingForward = false;
        }
        else if (nextRotation <= -halfArc)
        {
            nextRotation = -halfArc;
            _sweepingForward = true;
        }

        LaserPivot.RotationDegrees = nextRotation;
    }

    private void OnHealthChanged(int oldHealth)
    {
        if (CloneHealth == null || CloneHealth.CurrentHealth > 0)
        {
            return;
        }

        EmitSignal(SignalName.CloneDied);
        QueueFree();
    }
}
