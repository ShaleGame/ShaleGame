using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using CrossedDimensions.Characters.Controllers;

namespace CrossedDimensions.Entities.Enemies;

public partial class DetectPlayer : State
{
    [Export]
    public int Sight { get; set; }

    private Character _goat;
    private EnemyController _controller;

    [Export]
    public State Charge { get; set; }

    [Export]
    public RayCast2D FloorRaycast { get; set; }

    [Export]
    public RayCast2D WallRaycast { get; set; }

    private int _direction = -1;
    private AnimatedSprite2D _animSprite;

    public bool DetectedPlayer { get; private set; }

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

        DetectedPlayer = false;

        _goat.Speed = 35;

        _direction = _animSprite.FlipH ? 1 : -1;

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (WallRaycast is null || FloorRaycast is null || _animSprite is null || _controller is null || _goat is null)
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
        }

        if (!DetectedPlayer)
        {
            var spaceState = _goat.GetWorld2D().DirectSpaceState;
            float x = _goat.GlobalPosition.X + (_direction * Sight);
            float y = _goat.GlobalPosition.Y + 10;
            Vector2 rayTo = new Vector2(x, y);
            var query = PhysicsRayQueryParameters2D.Create(_goat.GlobalPosition, rayTo);
            query.CollisionMask = 1 << 0;
            query.CollisionMask |= 1 << 1;
            query.CollideWithAreas = false;
            query.CollideWithBodies = true;

            var result = spaceState.IntersectRay(query);

            if (result.Count > 0)
            {
                var collider = result["collider"].As<Node>();
                if (collider is Character && collider.IsInGroup("Player"))
                {
                    DetectedPlayer = true;
                    return Charge;
                }
            }
        }

        return base.Process(delta);
    }

    public void UndetectPlayer()
    {
        DetectedPlayer = false;
    }
}
