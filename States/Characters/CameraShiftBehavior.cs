using Godot;
using CrossedDimensions.Characters;

namespace CrossedDimensions.States.Characters;

/// <summary>
/// Idle-only camera offset behavior for the main player.
/// </summary>
public partial class CameraShiftBehavior : State
{
    private const float ShiftDistance = 120f;
    private const float ShiftDuration = 0.4f;

    private Character _character;
    private CloneableComponent _cloneable;
    private Node2D _cameraOffset;
    private Tween _offsetTween;
    private Vector2 _targetOffset = Vector2.Zero;

    public override State Enter(State previousState)
    {
        CacheReferences();

        if (IsClone())
        {
            return null;
        }

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (IsClone())
        {
            return null;
        }

        if (_cameraOffset is null)
        {
            return null;
        }

        Vector2 desiredOffset = GetDesiredOffset();

        if (!AreEqualApprox(_targetOffset, desiredOffset))
        {
            TweenToOffset(desiredOffset);
        }

        return null;
    }

    public override void Exit(State nextState)
    {
        if (IsClone())
        {
            return;
        }

        if (_cameraOffset is null)
        {
            return;
        }

        TweenToOffset(Vector2.Zero);
    }

    private void CacheReferences()
    {
        _character = Context as Character;
        _cloneable = _character?.Cloneable;
        _cameraOffset = _character?.GetNodeOrNull<Node2D>("CameraOffset");
        _targetOffset = _cameraOffset?.Position ?? Vector2.Zero;
    }

    private bool IsClone()
    {
        return _cloneable?.IsClone ?? false;
    }

    private Vector2 GetDesiredOffset()
    {
        float horizontal = Godot.Input.GetAxis("move_left", "move_right");

        if (!Mathf.IsZeroApprox(horizontal))
        {
            return Vector2.Zero;
        }

        if (Godot.Input.IsActionPressed("move_up"))
        {
            return new Vector2(0f, -ShiftDistance);
        }

        if (Godot.Input.IsActionPressed("move_down"))
        {
            return new Vector2(0f, ShiftDistance);
        }

        return Vector2.Zero;
    }

    private void TweenToOffset(Vector2 targetOffset)
    {
        _offsetTween?.Kill();
        _offsetTween = CreateTween();
        _offsetTween
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Sine)
            .TweenProperty(_cameraOffset, "position", targetOffset, ShiftDuration);

        _targetOffset = targetOffset;
    }

    private static bool AreEqualApprox(Vector2 a, Vector2 b)
    {
        return Mathf.IsEqualApprox(a.X, b.X) && Mathf.IsEqualApprox(a.Y, b.Y);
    }
}
