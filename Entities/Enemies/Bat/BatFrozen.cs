using CrossedDimensions.Characters;
using Godot;

namespace CrossedDimensions.States.Enemies;

[GlobalClass]
public partial class BatFrozen : State
{
    private Character _bat;
    private AnimatedSprite2D _sprite;
    private Area2D _hitbox;

    public override State Enter(State previousState)
    {
        _bat = Context as Character;
        _sprite = _bat?.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _hitbox = _bat?.GetNode<Area2D>("Hitbox");

        if (_bat != null)
        {
            _bat.Velocity = Vector2.Zero;
        }

        _sprite?.Pause();

        // A frozen bat acts as a platform, so it should not deal contact
        // damage while standing on it.
        _hitbox?.SetDeferred(Area2D.PropertyName.Monitoring, false);
        _hitbox?.SetDeferred(Area2D.PropertyName.Monitorable, false);

        return base.Enter(previousState);
    }

    public override State PhysicsProcess(double delta)
    {
        if (_bat == null)
        {
            return base.PhysicsProcess(delta);
        }

        _bat.Velocity = Vector2.Zero;

        if (!_bat.IsFrozen)
        {
            // Idle's Enter moves the movement machine back to Hanging.
            var brainSM = _bat.GetNode<StateMachine>("BrainStateMachine");
            brainSM?.ChangeState("Idle");
        }

        return base.PhysicsProcess(delta);
    }

    public override void Exit(State nextState)
    {
        _hitbox?.SetDeferred(Area2D.PropertyName.Monitoring, true);
        _hitbox?.SetDeferred(Area2D.PropertyName.Monitorable, true);

        base.Exit(nextState);
    }
}
