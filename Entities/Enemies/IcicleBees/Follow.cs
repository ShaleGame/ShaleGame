using System;
using CrossedDimensions.BoundingBoxes;
using CrossedDimensions.Characters;
using Godot;

namespace CrossedDimensions.States.Enemies.IceBees;

// Tracks the player and tries to stab them by floating and rotating around them. If they touch the player, they instantly die.

public partial class Follow : State
{
    public float moveSmoothing = 4f;
    [Export] public Hitbox hitbox;

    private Character _bee;
    private Character _player;
    private Vector2 _velocity = Vector2.Zero;

    private Callable _onHitCallable;
    private RandomNumberGenerator _rng;

    private float _minSpeed = 50f;
    private float _maxSpeed = 300f;

    public override void _Ready()
    {
        _onHitCallable = new Callable(this, nameof(OnHit));

        base._Ready();
    }

    public override State Enter(State previousState)
    {
        _bee = Context as Character;
        _player = _bee?.GetTree().GetFirstNodeInGroup("Player") as Character;

        _rng = new RandomNumberGenerator();

        moveSmoothing = (float)_rng.RandiRange(2, 6);

        if (hitbox != null)
        {
            hitbox.Connect(Hitbox.SignalName.HitCharacter, _onHitCallable);
        }

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (_bee == null || _player == null)
        {
            return base.Process(delta);
        }

        float dt = (float)delta;

        Vector2 toPlayer = _player.GlobalPosition - _bee.GlobalPosition;
        Vector2 toPlayerNorm = toPlayer.Normalized();

        float dot = _velocity.Normalized().Dot(toPlayerNorm);

        float speed = Mathf.Lerp(_minSpeed, _maxSpeed, (dot + 1f) / 2f);

        Vector2 desiredVelocity = (_player.GlobalPosition - _bee.GlobalPosition).Normalized() * speed;

        _velocity = _velocity.Lerp(desiredVelocity, moveSmoothing * dt);

        _bee.Velocity = _velocity;
        _bee.Rotation = _bee.Velocity.Angle() + Mathf.DegToRad(90f);
        _bee.MoveAndSlide();

        return base.Process(delta);
    }

    private void OnHit(Hitbox hitbox, Character character)
    {
        if (character == _player)
            _bee?.QueueFree();
    }

}
