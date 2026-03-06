using CrossedDimensions.BoundingBoxes;
using Godot;

namespace CrossedDimensions.Entities;

public partial class Projectile : Node2D
{
    /// <summary>
    /// Signal emitted when the projectile hits something. Use this to handle
    /// custom hit logic.
    /// </summary>
    [Signal]
    public delegate void ProjectileHitEventHandler(Projectile projectile, Hurtbox hurtbox);

    /// <summary>
    /// The direction the projectile is moving in. If set in the editor,
    /// it should be a normalized vector.
    /// </summary>
    [Export]
    public Vector2 Direction { get; set; }

    /// <summary>
    /// The speed of the projectile in pixels per second.
    /// </summary>
    [Export]
    public float Speed { get; set; }

    /// <summary>
    /// The hitbox associated with the projectile.
    /// </summary>
    [Export]
    public BoundingBoxes.Hitbox Hitbox { get; set; }

    /// <summary>
    /// The gravity applied to the projectile in pixels per second squared.
    /// </summary>
    [Export]
    public Vector2 Gravity { get; set; } = Vector2.Zero;

    /// <summary>
    /// Determines if the projectile should be freed on hit. Set to false if
    /// you want to handle projectile hit logic in a custom way using the
    /// ProjectileHit event.
    /// </summary>
    [Export]
    public bool FreeOnHit { get; set; } = true;

    /// <summary>
    /// A timer that determines the lifetime of the projectile. If null,
    /// the projectile will not expire. Note that the timer should have
    /// auto-start enabled in the editor.
    /// </summary>
    [Export]
    public Timer LifetimeTimer { get; set; }

    private Characters.Character _ownerCharacter;
    private Vector2 _velocity;

    /// <summary>
    /// The character that owns this projectile, if any.
    /// </summary>
    public Characters.Character OwnerCharacter
    {
        get => _ownerCharacter;
        set
        {
            _ownerCharacter = value;
            if (Hitbox is not null)
            {
                Hitbox.OwnerCharacter = value;
            }
        }
    }

    /// <summary>
    /// The weapon that owns this projectile, if any.
    /// </summary>
    public Items.Weapon OwnerWeapon { get; set; }

    public override void _Ready()
    {
        if (LifetimeTimer is not null)
        {
            LifetimeTimer.Timeout += () => QueueFree();
        }

        _velocity = Direction.Rotated(Rotation) * Speed;

        Hitbox.Hit += OnHitboxHit;
    }

    public override void _PhysicsProcess(double delta)
    {
        var deltaSeconds = (float)delta;
        _velocity += Gravity * deltaSeconds;
        Position += _velocity * deltaSeconds;
    }

    public void OnHitboxHit(Hitbox hitbox, Hurtbox hurtbox)
    {
        EmitSignal(SignalName.ProjectileHit, this, hurtbox);

        if (FreeOnHit)
        {
            QueueFree();
        }
    }
}
