using CrossedDimensions.Characters;
using Godot;
using System;

namespace CrossedDimensions.States.Enemies;

[GlobalClass]
public partial class BatSwooping : State
{
    [Export] public float SwoopSpeed = 300f;

    private Character _bat;
    private AnimatedSprite2D _sprite;

    public Vector2 SwoopTarget { get; set; }

    private bool goingRight = false;
    private bool pastRight = false;

    public override State Enter(State previousState)
    {
        _bat = Context as Character;
        _sprite = _bat?.GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        // Play swoop animation
        if (_sprite != null && _sprite.SpriteFrames.HasAnimation("flying")) ;
        {
            _sprite.Play("flying");
        }

        return base.Enter(previousState);
    }

    public override State PhysicsProcess(double delta)
    {
        if (_bat != null)
        {
            // Move toward the swoop target
            Vector2 direction = (SwoopTarget - _bat.GlobalPosition).Normalized();
            _bat.Velocity = direction * SwoopSpeed;

            if (direction.X > 0)
            {
                goingRight = true;
            }
            else
            {
                goingRight = false;
            }

            if (goingRight != pastRight)
            {
                if (goingRight)
                {
                    _sprite.FlipH = true;
                }
                else
                {
                    _sprite.FlipH = false;
                }

                GD.Print(goingRight);

                pastRight = goingRight;
            }

            _bat.MoveAndSlide();
        }

        return base.PhysicsProcess(delta);
    }

}
