using Godot;

namespace CrossedDimensions.BoundingBoxes;

[GlobalClass]
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
    public delegate void HitEventHandler(Hitbox hitbox, Hurtbox hurtbox);

    [Signal]
    public delegate void HitCharacterEventHandler(Hitbox hitbox, Characters.Character character);

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
        if (area is Hurtbox hurtbox)
        {
            if (!CanHitSelf && hurtbox.OwnerCharacter == OwnerCharacter)
            {
                return;
            }

            if (hurtbox.Hit(this))
            {
                EmitSignal(SignalName.Hit, this, hurtbox);

                if (hurtbox.OwnerCharacter is not null)
                {
                    EmitSignal(SignalName.HitCharacter, this, hurtbox.OwnerCharacter);
                }
            }
        }
    }

    public void OnHitboxBodyEntered(Node body)
    {
        if (!CanHitSelf && body == OwnerCharacter)
        {
            return;
        }

        // if the body is not a hurtbox, we still want to emit a hit signal so that
        // projectiles can interact with the environment (e.g. explode on
        // contact with a wall)
        EmitSignal(SignalName.Hit, this, new Variant());
    }
}
