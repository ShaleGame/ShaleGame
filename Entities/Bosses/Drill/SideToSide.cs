using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.Drill;

/// <summary>
/// Moves from side to side, bumping into the walls. Sometimes it jumps up.
/// </summary>

public partial class SideToSide : State
{

    [Export] public float MoveSpeed { get; set; } = 200f;
    [Export] public float JumpChance { get; set; } = 0.02f;
    [Export] public float MinJumpHeight { get; set; } = 400f;
    [Export] public float MaxJumpHeight { get; set; } = 600f;

    private Character _drillBit;
    private int _direction = 1;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override State Enter(State previousState)
    {
        _drillBit = Context as Character;

        GD.Print("Side to side!");

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (_drillBit == null)
        {
            return base.Process(delta);
        }

        float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").As<float>();

        // Apply gravity while not on floor
        if (!_drillBit.IsOnFloor())
        {
            _drillBit.Velocity = new Vector2(MoveSpeed * _direction, _drillBit.Velocity.Y + gravity * (float)delta);
        }
        else
        {
            _drillBit.Velocity = new Vector2(MoveSpeed * _direction, _drillBit.Velocity.Y);
        }

        // Random jump while grounded
        if (_drillBit.IsOnFloor() && _rng.Randf() < JumpChance)
        {
            //float jumpForce = _rng.RandfRange(MinJumpHeight, MaxJumpHeight);
            //_drillBit.Velocity = new Vector2(_drillBit.Velocity.X, -jumpForce);
        }

        _drillBit.MoveAndSlide();

        if (_drillBit.IsOnWall())
        {
            _direction *= -1;
        }

        return base.Process(delta);
    }

}
