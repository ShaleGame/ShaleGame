using Godot;

namespace CrossedDimensions.BoundingBoxes;

public partial class Hitbox : BoundingBox
{
    [Export]
    public Characters.Character OwnerCharacter { get; set; }

    [Export]
    public Components.DamageComponent DamageComponent { get; set; }

    /// <summary>
    /// A timer that can be used to disable the hitbox after a certain
    /// duration. This is useful for objects such as explosions that
    /// should only be able to hit things for a brief moment.
    /// </summary>
    [Export]
    public Timer LifetimeTimer { get; set; }

    [Signal]
    public delegate void HitEventHandler();

    /// <summary>
    /// Determines if the hitbox can hit the entity that owns it.
    /// </summary>
    [Export]
    public bool CanHitSelf { get; set; } = false;

    /// <summary>
    /// Determines if the damage dealt by this hitbox falls off with
    /// distance from the center of the hitbox.
    /// </summary>
    [Export]
    public bool FalloffWithDistance { get; set; } = false;

    public override void _Ready()
    {
        AreaEntered += OnHitboxAreaEntered;
        BodyEntered += OnHitboxBodyEntered;

        if (LifetimeTimer is not null)
        {
            LifetimeTimer.Timeout += () => QueueFree();
        }

        base._Ready();
    }

    public void OnHitboxAreaEntered(Area2D area)
    {
        GD.Print("hit");
        if (area is Hurtbox hurtbox)
        {
            if (!CanHitSelf && hurtbox.OwnerCharacter == OwnerCharacter)
            {
                return;
            }

            if (hurtbox.Hit(this))
            {
                EmitSignal(SignalName.Hit);
            }
        }
    }

    public void OnHitboxBodyEntered(Node body)
    {
        if (!CanHitSelf && body == OwnerCharacter)
        {
            return;
        }

        EmitSignal(SignalName.Hit);
    }
}
