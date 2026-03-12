using Godot;
using System;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Enemies;

// When goat has not detected a player

public partial class DetectPlayer : State
{
    [Export]
    public int sight;

    private Character _goat;

    private StateMachine selfMachine;
    private StateMachine moveMachine;
    private State charge;

    private int _direction = -1;
    private AnimatedSprite2D _animSprite;

    public bool detectedPlayer = false;

    public override State Enter(State previousState)
    {
        _goat = Context as Character;

        selfMachine = GetParent<StateMachine>();

        moveMachine = _goat.FindChild("Movement") as StateMachine;
        charge = moveMachine.FindChild("Charge") as State;

        _animSprite = _goat.FindChild("AnimatedSprite2D") as AnimatedSprite2D;

        detectedPlayer = false;

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        _direction = _animSprite.FlipH ? 1 : -1;
        
        if (!detectedPlayer)
        {
            // Raycast to find player
            var spaceState = _goat.GetWorld2D().DirectSpaceState;
            var query = PhysicsRayQueryParameters2D.Create(_goat.GlobalPosition + Vector2.Right * _direction * sight, _goat.GlobalPosition);
            query.CollisionMask = 1 << 0; // Colliding with environment and player
            query.CollisionMask += 1 << 1;
            query.CollideWithAreas = false;
            query.CollideWithBodies = true;

            var result = spaceState.IntersectRay(query);

            if (result.Count > 0)
            {
                GD.Print("Casting");

                var collider = result["collider"].As<Node>();
                if (collider is Character character)
                {
                    if (collider.IsInGroup("Player"))
                    {
                        detectedPlayer = true;

                        moveMachine.ChangeState(charge);
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
