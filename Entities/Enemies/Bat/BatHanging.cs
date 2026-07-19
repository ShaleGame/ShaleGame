using CrossedDimensions.Characters;
using Godot;
using System;

namespace CrossedDimensions.States.Enemies;

[GlobalClass]
public partial class BatHanging : State
{

    private Character _bat;
    private AnimatedSprite2D _sprite;
    private State _frozen;

    public Vector2 HangPosition { get; set; }

    public override State Enter(State previousState)
    {
        _bat = Context as Character;
        _sprite = _bat?.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _frozen = GetParent().GetNode<State>("Frozen");

        // If this is the first time hanging, store initial position
        if (HangPosition == Vector2.Zero && _bat != null)
        {
            HangPosition = _bat.GlobalPosition;
        }

        // Play hanging position
        if (_sprite != null && _sprite.SpriteFrames.HasAnimation("hanging"))
        {
            _sprite.Play("hanging");
            _sprite.Pause();
            _sprite.Frame = 0;
        }

        // Ensure bat is at hanging position and not moving
        if (_bat != null)
        {
            _bat.GlobalPosition = HangPosition;
            _bat.Velocity = Vector2.Zero;
        }

        return base.Enter(previousState);
    }

    public override State PhysicsProcess(double delta)
    {
        if (_bat == null)
        {
            return base.PhysicsProcess(delta);
        }

        if (_bat.IsFrozen)
        {
            return _frozen;
        }

        // Keep velocity at zero while hanging
        _bat.Velocity = Vector2.Zero;

        return base.PhysicsProcess(delta);
    }
}
