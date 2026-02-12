using CrossedDimensions.Characters;
using Godot;
using System;

namespace CrossedDimensions.States.Enemies;

[GlobalClass]
public partial class BatReturning : State
{
    [Export] public float ReturnSpeed = 300f;
    [Export] public float RaycastLength = 500f;

    private Character _bat;
    private AnimatedSprite2D _sprite;
    private State _hanging;

    public override State Enter(State previousState)
    {

        _bat = Context as Character;
        _sprite = _bat?.GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        // Get reference to Hanging state
        _hanging = GetParent().GetNode<State>("Hanging");

        // Play return animation
        if (_sprite != null && _sprite.SpriteFrames.HasAnimation("flying"))
        {
            _sprite.Play("flying");
        }

        return base.Enter(previousState);
    }

    public override State PhysicsProcess(double delta)
    {
        if (_bat != null)
        {
            _bat.Velocity = Vector2.Up * ReturnSpeed;
            _bat.MoveAndSlide();

            // Check if we found a ceiling above us
            if (FindCeilingAbove(out Vector2 ceilingPosition))
            {
                // Check if we're close enough to the ceiling
                if (_bat.GlobalPosition.DistanceTo(ceilingPosition) < 10f)
                {
                    // Snap to ceiling
                    _bat.GlobalPosition = ceilingPosition;

                    // Update hanging state's position to new ceiling
                    if (_hanging is BatHanging hangingState)
                    {
                        hangingState.HangPosition = ceilingPosition;
                    }

                    // Tell brain we're done attacking
                    var brainSM = _bat.GetNode<StateMachine>("BrainStateMachine");
                    brainSM?.ChangeState("Idle");

                    // Transition to hanging

                    return _hanging;
                }
            }
        }

        return base.PhysicsProcess(delta);
    }

    private bool FindCeilingAbove(out Vector2 ceilingPosition)
    {
        if (_bat == null)
        {
            ceilingPosition = Vector2.Zero;
            return false;
        }

        // Raycast directly upward to find ceiling
        var spaceState = _bat.GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(_bat.GlobalPosition, _bat.GlobalPosition + Vector2.Up * RaycastLength);
        query.CollideWithAreas = false;
        query.CollideWithBodies = true;

        var result = spaceState.IntersectRay(query);

        if (result.Count > 0)
        {
            ceilingPosition = (Vector2)result["position"];
            return true;
        }

        ceilingPosition = Vector2.Zero;
        return false;
    }
}
