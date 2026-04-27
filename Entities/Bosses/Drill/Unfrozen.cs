using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Components;

namespace CrossedDimensions.Entities.Bosses.Drill;

/// <summary>
/// The vent is unfrozen and can't be hurt.
/// </summary>

public partial class Unfrozen : State
{

    [Export] public FreezableComponent Freezeable { get; set; }
    [Export] public CollisionShape2D HurtboxCollision { get; set; }
    [Export] public CollisionShape2D FrozenBoxCollision { get; set; }
    [Export] public State FrozenState { get; set; }
    [Export] public AnimatedSprite2D Sprite { get; set; }
    [Export] public GpuParticles2D SmokeParticles { get; set; }

    public override State Enter(State previousState)
    {

        HurtboxCollision.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        FrozenBoxCollision.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);

        Freezeable.Frozen -= Frozen; // Avoid duplicate signals
        Freezeable.Frozen += Frozen;

        Sprite.Play("Default");

        SmokeParticles.Emitting = true;

        return base.Enter(previousState);
    }

    public override void Exit(State nextState)
    {
        Freezeable.Frozen -= Frozen;

        SmokeParticles.Emitting = false;

        base.Exit(nextState);
    }

    private void Frozen(float timeLeft)
    {
        StateMachine parent = GetParent<StateMachine>();

        parent.ChangeState(FrozenState);
    }

}
