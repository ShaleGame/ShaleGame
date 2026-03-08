using Godot;
using System;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Characters.Controllers;

[GlobalClass]
public abstract partial class CharacterController : Node2D
{
    /// <summary>
    /// Gets or sets the scale factor for the X axis input. Useful for inverting controls
    /// such as when a cloned character has mirrored movement.
    /// </summary>
    [Export]
    public float XScale { get; set; } = 1.0f;

    /// <summary>
    /// When <c>false</c>, all input properties return their neutral/zero values
    /// and no input signals are emitted. Set to <c>false</c> to disable player
    /// control (e.g. on death).
    /// </summary>
    public bool IsActive { get; set; } = true;

    public bool IsMoving => !MovementInput.IsZeroApprox();

    public abstract Vector2 MovementInput { get; }

    public abstract Vector2 Target { get; }

    public abstract bool IsJumping { get; }

    public abstract bool IsJumpHeld { get; }

    public abstract bool IsJumpReleased { get; }

    public abstract bool IsMouse1Held { get; }

    public abstract bool IsMouse2Held { get; }

    public abstract bool IsSplitting { get; }

    public abstract bool IsSplitReleased { get; }

    public abstract bool IsSplitHeld { get; }

    public abstract bool IsInteractHeld { get; }

    public Character OwnerCharacter { get; set; }

    protected bool IsBlocked => OwnerCharacter?.IsFrozen ?? false;

    [Signal]
    public delegate void WeaponNextRequestedEventHandler();

    [Signal]
    public delegate void WeaponPreviousRequestedEventHandler();

    /// <summary>
    /// Emitted when an input slot key is pressed so listeners can bind a
    /// specific weapon slot to the event stream.
    /// </summary>
    /// <param name="index">Zero-based index of the requested weapon slot.</param>
    [Signal]
    public delegate void WeaponSlotRequestedEventHandler(int index);
}
