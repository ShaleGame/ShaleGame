using Godot;
using CrossedDimensions.States;
using System;
using System.ComponentModel;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Neutral.PathPlatform;

public partial class PathTrace : State
{

    [Export]
    PathFollow2D pathFollow;

    [Export]
    Path2D path;

    private Character _platform;

    private bool _curveClosed = true;

    private int _direction = 1;

    public override State Enter(State previousState)
    {
        _platform = Context as Character;

        _platform.GlobalPosition = path.Curve.GetPointPosition(0);

        if (path.Curve.PointCount >= 2)
        {
            _curveClosed = false;
        }
        else
        {
            var firstPos = path.Curve.GetPointPosition(0);
            var lastPos = path.Curve.GetPointPosition(path.Curve.PointCount - 1);
            _curveClosed = firstPos.IsEqualApprox(lastPos);
        }



        return base.Enter(previousState);
    }

    public override State PhysicsProcess(double delta)
    {
        if (!_platform.Freezable.IsFrozen)
        {
            pathFollow.Progress += _platform.Speed * (float)delta * _direction;

            _platform.GlobalPosition = pathFollow.GlobalPosition;

            if (!_curveClosed)
            {
                if (pathFollow.ProgressRatio >= 1)
                {
                    _direction = -1;
                }
                else if (pathFollow.ProgressRatio <= 0)
                {
                    _direction = 1;
                }
            }

            GD.Print("Progress: ", pathFollow.Progress);
        }

        return base.PhysicsProcess(delta);
    }

}
