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
    /// Physics layers checked before enabling the frozen collision layer.
    /// While any body on these layers overlaps the parent, enabling the
    /// collision is deferred so those bodies are not pushed out or stuck
    /// inside the ice block. Defaults to PlayerCollision | EnemyCollision.
    /// </summary>
    [Export(PropertyHint.Layers2DPhysics)]
    public uint ObstructionMask { get; set; } = 0b1010;

    private bool _collisionPending;

    /// <summary>
    /// The health component of the ice block, if any. This is used to apply
    /// damage to the ice block when it is hit while frozen.
    /// </summary>
    [Export]
    public HealthComponent Health { get; set; }

    public override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
    }

    public void Freeze(float duration)
    {
        GD.Print("frozen!");
        if (duration <= 0f)
        {
            return;
        }

        _timeLeft = Mathf.Max(_timeLeft, duration);
        SetProcess(true);
        EmitSignal(SignalName.Frozen, _timeLeft);

        if (ForceCollisionWhileFrozen && GetParent() is CollisionObject2D)
        {
            _collisionPending = true;
            SetPhysicsProcess(true);
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
            _collisionPending = false;
            SetPhysicsProcess(false);
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

    public override void _PhysicsProcess(double delta)
    {
        if (!_collisionPending || GetParent() is not CollisionObject2D parent)
        {
            SetPhysicsProcess(false);
            return;
        }

        if (IsObstructed(parent))
        {
            return;
        }

        parent.SetCollisionLayerValue(5, true);
        _collisionPending = false;
        SetPhysicsProcess(false);
    }

    private bool IsObstructed(CollisionObject2D parent)
    {
        var spaceState = parent.GetWorld2D().DirectSpaceState;
        var exclude = new Godot.Collections.Array<Rid> { parent.GetRid() };

        foreach (uint ownerId in parent.GetShapeOwners())
        {
            var transform = parent.GlobalTransform
                * parent.ShapeOwnerGetTransform(ownerId);
            int shapeCount = parent.ShapeOwnerGetShapeCount(ownerId);

            for (int i = 0; i < shapeCount; i++)
            {
                var query = new PhysicsShapeQueryParameters2D
                {
                    Shape = parent.ShapeOwnerGetShape(ownerId, i),
                    Transform = transform,
                    CollisionMask = ObstructionMask,
                    CollideWithAreas = false,
                    CollideWithBodies = true,
                    Exclude = exclude,
                };

                if (spaceState.IntersectShape(query, 1).Count > 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public override void _ExitTree()
    {
        if (IsFrozen)
        {
            Unfreeze();
        }
    }
}
