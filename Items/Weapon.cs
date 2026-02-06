using Godot;

namespace CrossedDimensions.Items;

/// <summary>
/// A weapon instance. Weapons encapsulate behavior driven by a state machine
/// and can read character input when active.
/// </summary>
[GlobalClass]
public partial class Weapon : ItemInstance
{
    /// <summary>
    /// The state machine that drives this weapon's states and behaviour.
    /// </summary>
    [Export]
    public States.StateMachine StateMachine { get; set; }

    /// <summary>
    /// Whether the primary-use input is currently held and the weapon is allowed
    /// to act on it.
    /// </summary>
    public bool IsPrimaryUseHeld
    {
        get => IsActive && (OwnerCharacter?.Controller?.IsMouse1Held ?? false);
    }

    /// <summary>
    /// Whether the secondary-use input is currently held and the weapon is allowed
    /// to act on it.
    /// </summary>
    public bool IsSecondaryUseHeld
    {
        get => IsActive && (OwnerCharacter?.Controller?.IsMouse2Held ?? false);
    }

    /// <summary>
    /// Current input target from the owner character's controller, or
    /// <see cref="Vector2.Zero"/> when no controller is present.
    /// </summary>
    public Vector2 Target
    {
        get => OwnerCharacter?.Controller?.Target ?? Vector2.Zero;
    }

    /// <summary>
    /// When <c>true</c> the weapon is active and may receive input and be used.
    /// Defaults to <c>true</c> so weapons remain usable when no <see cref="Inventory"/>
    /// component is present on the owning character (optional inventory).
    /// The <see cref="Inventory"/> is expected to manage this property (setting it
    /// to <c>false</c> for stored/unequipped weapons and to <c>true</c> when equipping),
    /// but other selection or multiplexing systems that replace or complement an
    /// inventory may also control this property to determine which weapon is active.
    /// </summary>
    [Export]
    public bool IsActive { get; set; } = true;

    public override void _Ready()
    {
        StateMachine.Initialize(this);
    }

    public override void _Process(double delta)
    {
        StateMachine.Process(delta);
    }
}
