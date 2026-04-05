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

    private Character platform;

    public override State Enter(State previousState)
    {
        platform = Context as Character;

        platform.GlobalPosition = path.Curve.GetPointPosition(0);

        return base.Enter(previousState);
    }

    public override State PhysicsProcess(double delta)
    {
        pathFollow.Progress += platform.Speed * (float)delta;

        platform.GlobalPosition = pathFollow.GlobalPosition;
        
        GD.Print("Progress: ", pathFollow.Progress);

        return base.PhysicsProcess(delta);
    }

}
