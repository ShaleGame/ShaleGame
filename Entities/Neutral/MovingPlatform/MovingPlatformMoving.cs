using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using CrossedDimensions.Components;

namespace CrossedDimensions.Entities.Neutral.MovingPlatform;

/// <summary>
/// The default movement state for the moving platform.
/// The platform travels between two <see cref="Marker2D"/> nodes injected via
/// <see cref="PointA"/> and <see cref="PointB"/>. Move those nodes in the editor
/// to set the patrol path. The platform starts toward <see cref="PointB"/> and
/// bounces back and forth.
/// When the <see cref="FreezableComponent"/> emits <c>Frozen</c> the state
/// machine transitions to <see cref="MovingPlatformFrozen"/>.
/// </summary>
public partial class MovingPlatformMoving : State
{
    /// <summary>
    /// The first waypoint of the patrol path.
    /// </summary>
    [Export]
    public Marker2D PointA { get; set; }

    /// <summary>
    /// The second waypoint of the patrol path.
    /// </summary>
    [Export]
    public Marker2D PointB { get; set; }

    private Vector2 _pointAPosition;

    private Vector2 _pointBPosition;

    /// <summary>
    /// Movement speed in pixels per second.
    /// </summary>
    [Export]
    public float Speed { get; set; } = 64f;

    private Character _platform;
    private FreezableComponent _freezable;

    // Index of the waypoint we are currently heading toward (0 = A, 1 = B).
    private int _targetIndex = 1;

    public override State Enter(State previousState)
    {
        _platform = Context as Character;
        _freezable = _platform?.Freezable;

        _pointAPosition = PointA.GlobalPosition;
        _pointBPosition = PointB.GlobalPosition;

        return base.Enter(previousState);
    }

    public override State PhysicsProcess(double delta)
    {
        if (_platform is null || PointA is null || PointB is null)
        {
            return base.PhysicsProcess(delta);
        }

        if (_platform.IsFrozen)
        {
            return base.PhysicsProcess(delta);
        }

        Vector2 target = _targetIndex == 0 ? _pointAPosition : _pointBPosition;
        Vector2 toTarget = target - _platform.GlobalPosition;

        if (toTarget.LengthSquared() <= (Speed * (float)delta) * (Speed * (float)delta))
        {
            // Arrived — snap and reverse.
            _platform.GlobalPosition = target;
            _platform.Velocity = Vector2.Zero;
            _platform.MoveAndSlide();
            _targetIndex = _targetIndex == 0 ? 1 : 0;
        }
        else
        {
            _platform.Velocity = toTarget.Normalized() * Speed;
            _platform.MoveAndSlide();
        }

        return base.PhysicsProcess(delta);
    }
}
