using Godot;

namespace CrossedDimensions.Components;

[GlobalClass]
public partial class FreezableComponent : Node
{
    [Signal]
    public delegate void FrozenEventHandler(float timeLeft);

    [Signal]
    public delegate void UnfrozenEventHandler();

    private float _timeLeft;

    public bool IsFrozen => _timeLeft > 0f;

    public float TimeLeft => _timeLeft;

    /// <summary>
    /// Determines if the parent CollisionObject2D should force collision
    /// responses while frozen. This is useful for implementing a feature
    /// where the player can be frozen solid and used as a platform for other
    /// characters to stand on. Only use for collision objects that are not
    /// in the world collision layer.
    /// </summary>
    [Export]
    public bool ForceCollisionWhileFrozen { get; set; } = false;

    /// <summary>
    /// The health component of the ice block, if any. This is used to apply
    /// damage to the ice block when it is hit while frozen.
    /// </summary>
    [Export]
    public HealthComponent Health { get; set; }

    public override void _Ready()
    {
        SetProcess(false);
    }

    public void Freeze(float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        _timeLeft = Mathf.Max(_timeLeft, duration);
        SetProcess(true);
        EmitSignal(SignalName.Frozen, _timeLeft);

        if (ForceCollisionWhileFrozen && GetParent() is CollisionObject2D parent)
        {
            parent.SetCollisionLayerValue(5, true);
        }

        Health.CurrentHealth = Health.MaxHealth;
        Health.HealthChanged += OnHealthChanged;
    }

    private void OnHealthChanged(int health)
    {
        if (Health.CurrentHealth <= 0)
        {
            Unfreeze();
        }
    }

    public void Unfreeze()
    {
        if (!IsFrozen)
        {
            return;
        }

        _timeLeft = 0f;
        SetProcess(false);
        EmitSignal(SignalName.Unfrozen);

        if (ForceCollisionWhileFrozen && GetParent() is CollisionObject2D parent)
        {
            parent.SetCollisionLayerValue(5, false);
        }

        Health.HealthChanged -= OnHealthChanged;
        Health.CurrentHealth = 0;
    }

    public override void _Process(double delta)
    {
        if (_timeLeft <= 0f)
        {
            return;
        }

        float remaining = _timeLeft - (float)delta;
        if (remaining <= 0f)
        {
            Unfreeze();
        }
        else
        {
            _timeLeft = remaining;
        }
    }
}
