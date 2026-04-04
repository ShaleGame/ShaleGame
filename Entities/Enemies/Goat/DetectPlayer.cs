using Godot;
using System;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using CrossedDimensions.Characters.Controllers;

namespace CrossedDimensions.Entities.Enemies;

// When goat has not detected a player

public partial class DetectPlayer : State
{
    [Export]
    public int sight;

    private Character _goat;
    private EnemyController _controller;

    [Export]
    public State charge;

    [Export]
    public RayCast2D floorRaycast;
    [Export]
    public RayCast2D wallRaycast;

    private int _direction = -1;
    private AnimatedSprite2D _animSprite;

    public bool detectedPlayer = false;

    public override State Enter(State previousState)
    {
        _goat = Context as Character;
        _controller = _goat.Controller as EnemyController;

        _animSprite = _goat.FindChild("AnimatedSprite2D") as AnimatedSprite2D;

        detectedPlayer = false;

        _goat.Speed = 35;

        _direction = _animSprite.FlipH ? 1 : -1;

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {

        if (wallRaycast.IsColliding() || !floorRaycast.IsColliding())
        {
            _direction *= -1;

            wallRaycast.TargetPosition = new Vector2(wallRaycast.TargetPosition.X * -1, wallRaycast.TargetPosition.Y);

            floorRaycast.Position = new Vector2(floorRaycast.Position.X * -1, floorRaycast.Position.Y);

            _animSprite.FlipH = _direction > 0;

            if (_controller != null && _controller.HasMethod("SetMovementInput"))
            {
                _controller.SetMovementInput(new Vector2(_direction, 0));
            }

        }

        if (!detectedPlayer)
        {
            // Raycast to find player
            var spaceState = _goat.GetWorld2D().DirectSpaceState;
            float x = _goat.GlobalPosition.X + Vector2.Right.X * _direction * sight;
            float y = _goat.GlobalPosition.Y + Vector2.Right.Y * _direction * sight + 10;
            Vector2 rayTo = new Vector2(x, y);
            var query = PhysicsRayQueryParameters2D.Create(_goat.GlobalPosition, rayTo);
            query.CollisionMask = 1 << 0; // Colliding with environment and player
            query.CollisionMask |= 1 << 1;
            query.CollideWithAreas = false;
            query.CollideWithBodies = true;

            var result = spaceState.IntersectRay(query);

            if (result.Count > 0)
            {
                var collider = result["collider"].As<Node>();
                if (collider is Character character)
                {
                    if (collider.IsInGroup("Player"))
                    {
                        detectedPlayer = true;
                        return charge;
                    }
                }
            }
        }

        return base.Process(delta);
    }

    public void UndetectPlayer()
    {
        detectedPlayer = false;
    }

}
