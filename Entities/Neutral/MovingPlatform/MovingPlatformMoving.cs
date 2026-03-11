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

    /// <summary>
    /// If true the platform will return to <see cref="PointA"/> after reaching <see cref="PointB"/>.
    /// If false the platform stops at <see cref="PointB"/> and remains there.
    /// </summary>
    [Export]
    public bool ReturnToInitialPoint { get; set; } = true;

    private bool _stopped;

    /// <summary>
    /// If not null, the platform will move towards <see cref="PointB"/> when
    /// the activator is active and towards <see cref="PointA"/> otherwise.
    /// </summary>
    private Environment.Triggers.ActivationLogicActivator _activator;

    [Export]
    public Environment.Triggers.ActivationLogicActivator Activator
    {
        get => _activator;
        set
        {
            if (_activator == value) return;

            if (_activator != null)
            {
                _activator.Activated -= OnActivatorActivated;
                _activator.Deactivated -= OnActivatorDeactivated;
            }

            _activator = value;

            if (_activator != null)
            {
                _activator.Activated += OnActivatorActivated;
                _activator.Deactivated += OnActivatorDeactivated;

                // Set initial target based on activator state: move to B when activated, A otherwise.
                _targetIndex = _activator.IsActivated ? 1 : 0;
            }
        }
    }

    private void OnActivatorActivated()
    {
        _stopped = false;
        _targetIndex = 1; // move toward PointB
    }

    private void OnActivatorDeactivated()
    {
        _stopped = false;
        _targetIndex = 0; // move toward PointA
    }

    public override State Enter(State previousState)
    {
        _platform = Context as Character;
        _freezable = _platform?.Freezable;

        _pointAPosition = PointA.GlobalPosition;
        _pointBPosition = PointB.GlobalPosition;

        // Reset stopped state when entering.
        _stopped = false;

        // If configured to teleport back and we're already at PointB, teleport to PointA immediately.
        if (!ReturnToInitialPoint && _platform != null)
        {
            var toB = _platform.GlobalPosition - _pointBPosition;
            if (toB.LengthSquared() <= 1f)
            {
                _platform.GlobalPosition = _pointAPosition;
                _platform.Velocity = Vector2.Zero;
                _platform.MoveAndSlide();
                _targetIndex = 1; // next target is B
            }
        }

        return base.Enter(previousState);
    }

    public override State PhysicsProcess(double delta)
    {
        if (_platform is null || PointA is null || PointB is null)
        {
            return base.PhysicsProcess(delta);
        }

        if (_platform.IsFrozen || _stopped)
        {
            return base.PhysicsProcess(delta);
        }

        // If an activator is assigned, the target is driven by the activator state.
        int effectiveTargetIndex = _activator != null ? (_activator.IsActivated ? 1 : 0) : _targetIndex;
        Vector2 target = effectiveTargetIndex == 0 ? _pointAPosition : _pointBPosition;
        Vector2 toTarget = target - _platform.GlobalPosition;

        if (toTarget.LengthSquared() <= (Speed * (float)delta) * (Speed * (float)delta))
        {
            _platform.GlobalPosition = target;
            _platform.Velocity = Vector2.Zero;
            _platform.MoveAndSlide();

            if (_activator != null)
            {
                // When controlled by an activator, stop at the target until the activator changes.
                _stopped = true;
                _targetIndex = effectiveTargetIndex; // keep internal index in sync
            }
            else
            {
                // original behavior: teleport back or reverse target
                if (effectiveTargetIndex == 1 && !ReturnToInitialPoint)
                {
                    _platform.GlobalPosition = _pointAPosition;
                    _platform.Velocity = Vector2.Zero;
                    _platform.MoveAndSlide();

                    // after teleporting, set target to B so it can travel to B again if desired
                    _targetIndex = 1;
                }
                else
                {
                    // otherwise reverse target
                    _targetIndex = effectiveTargetIndex == 0 ? 1 : 0;
                }
            }
        }
        else
        {
            _platform.Velocity = toTarget.Normalized() * Speed;
            _platform.MoveAndSlide();
        }

        return base.PhysicsProcess(delta);
    }

    public override void _ExitTree()
    {
        if (_activator != null)
        {
            _activator.Activated -= OnActivatorActivated;
            _activator.Deactivated -= OnActivatorDeactivated;
            _activator = null;
        }

        base._ExitTree();
    }
}
