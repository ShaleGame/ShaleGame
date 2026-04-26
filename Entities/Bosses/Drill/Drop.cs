using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.Drill;

/// <summary>
/// Drops down from the position and transfers to SideToSide as soon as it hits the floor.
/// </summary>

public partial class Drop : State
{

    [Export] public float DropSpeed { get; set; } = 300f;
    [Export] public State SideToSideState { get; set; }

    private Character _drillBit;

    public override State Enter(State previousState)
    {
        _drillBit = Context as Character;

        GD.Print("Drop!");

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (_drillBit == null)
        {
            return base.Process(delta);
        }

        _drillBit.Velocity = new Vector2(0, DropSpeed);
        _drillBit.MoveAndSlide();

        if (_drillBit.IsOnFloor())
        {
            var parentMachine = GetParent() as StateMachine;
            parentMachine?.ChangeState(SideToSideState);
        }

        return base.Process(delta);
    }

}
