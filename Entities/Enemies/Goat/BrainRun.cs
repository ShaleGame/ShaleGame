using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using CrossedDimensions.Characters.Controllers;

namespace CrossedDimensions.Entities.Enemies;

public partial class BrainRun : State
{
    private Character _goat;
    private EnemyController _controller;

    [Export]
    public State Detect { get; set; }

    [Export]
    public RayCast2D FloorRaycast { get; set; }

    [Export]
    public RayCast2D WallRaycast { get; set; }

    private int _direction = -1;
    private AnimatedSprite2D _animSprite;

    public override State Enter(State previousState)
    {
        _goat = Context as Character;
        if (_goat is null)
        {
            return null;
        }

        _controller = _goat.Controller as EnemyController;
        if (_controller is null)
        {
            return null;
        }

        _animSprite = _goat.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        if (_animSprite is null)
        {
            return null;
        }

        _goat.Speed = 300;

        _direction = _animSprite.FlipH ? 1 : -1;

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (WallRaycast is null || FloorRaycast is null || _controller is null || _animSprite is null)
        {
            return base.Process(delta);
        }

        if (WallRaycast.IsColliding() || !FloorRaycast.IsColliding())
        {
            _direction *= -1;

            WallRaycast.TargetPosition = new Vector2(-WallRaycast.TargetPosition.X, WallRaycast.TargetPosition.Y);

            FloorRaycast.Position = new Vector2(-FloorRaycast.Position.X, FloorRaycast.Position.Y);

            _animSprite.FlipH = _direction > 0;

            _controller.SetMovementInput(new Vector2(_direction, 0));

            return Detect;
        }

        _controller.SetMovementInput(new Vector2(_direction, 0));

        return base.Process(delta);
    }

}
