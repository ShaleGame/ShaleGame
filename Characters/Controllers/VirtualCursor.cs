using Godot;

namespace CrossedDimensions.Characters.Controllers;

/// <summary>
/// How the aim stick positions the shared cursor.
/// </summary>
public enum AimMode
{
    /// <summary>
    /// Absolute offset (<c>stick * CombatRadius</c>). Snappy and stateless — angle
    /// from the stick direction, distance from its magnitude. Returns toward the
    /// anchor when the stick is released.
    /// </summary>
    Combat,

    /// <summary>
    /// Velocity-driven free placement — the stick drives the cursor's velocity and
    /// it holds position on neutral. Gives mouse-grade fine placement.
    /// </summary>
    Precision,
}

/// <summary>
/// Pure (Node-free) aim model shared by the player and clone on controller. Holds
/// the cursor <see cref="Offset"/> from the camera-followed anchor; both bodies
/// aim at the same world point (<see cref="CursorWorld"/>), so split fire
/// converges exactly like it does at the mouse cursor.
/// </summary>
/// <remarks>
/// Deliberately not a Node so the math stays unit-testable in isolation. A thin
/// component is expected to wrap this, feeding the real aim-stick vector and the
/// anchor position each frame and reading <see cref="CursorWorld"/> back into the
/// controllers' aim target.
/// </remarks>
public sealed class VirtualCursor
{
    /// <summary>Combat-mode reach at full stick deflection, in world units.</summary>
    public float CombatRadius { get; set; } = 96f;

    /// <summary>Precision-mode cursor speed, in world units per second.</summary>
    public float PrecisionSpeed { get; set; } = 160f;

    /// <summary>Maximum cursor distance from the anchor (viewport clamp).</summary>
    public float MaxRange { get; set; } = 200f;

    /// <summary>Current cursor offset from the anchor.</summary>
    public Vector2 Offset { get; private set; }

    /// <summary>How the stick currently positions the cursor.</summary>
    public AimMode Mode { get; private set; } = AimMode.Combat;

    /// <summary>
    /// Advances the cursor one frame from the aim-stick vector (magnitude 0..1)
    /// and the frame delta in seconds. Combat overwrites the offset absolutely;
    /// precision integrates velocity and clamps to <see cref="MaxRange"/>.
    /// </summary>
    public void Process(Vector2 stick, float delta)
    {
        Offset = Mode == AimMode.Combat
            ? stick * CombatRadius
            : ClampToRange(Offset + (stick * PrecisionSpeed * delta), MaxRange);
    }

    /// <summary>
    /// Toggles between combat and precision. Entering precision keeps the current
    /// <see cref="Offset"/> so the cursor does not jump (it seeds from wherever you
    /// were aiming); the next combat update reclaims absolute control.
    /// </summary>
    public void ToggleMode()
    {
        Mode = Mode == AimMode.Combat ? AimMode.Precision : AimMode.Combat;
    }

    /// <summary>The shared world-space cursor point for a given anchor.</summary>
    public Vector2 CursorWorld(Vector2 anchor) => anchor + Offset;

    /// <summary>
    /// The aim vector a body at <paramref name="bodyPosition"/> uses to aim at the
    /// shared cursor (<c>cursorWorld - bodyPosition</c>). Identical in shape to the
    /// existing mouse math, so the player and clone converge on one point.
    /// </summary>
    public static Vector2 AimVector(Vector2 cursorWorld, Vector2 bodyPosition)
        => cursorWorld - bodyPosition;

    /// <summary>Clamps a vector's magnitude to <paramref name="maxRange"/>.</summary>
    public static Vector2 ClampToRange(Vector2 offset, float maxRange)
    {
        if (offset.LengthSquared() <= maxRange * maxRange)
        {
            return offset;
        }

        return offset.Normalized() * maxRange;
    }
}
