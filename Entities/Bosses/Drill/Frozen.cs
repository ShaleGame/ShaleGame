using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Components;

namespace CrossedDimensions.Entities.Bosses.Drill;

/// <summary>
/// The vent is frozen and can be hurt.
/// </summary>

public partial class Frozen : State
{

    [Export] public FreezableComponent Freezeable { get; set; }
    [Export] public CollisionShape2D HurtboxCollision { get; set; }
    [Export] public CollisionShape2D FrozenBoxCollision { get; set; }
    [Export] public HealthComponent Health { get; set; }
    [Export] public State UnfrozenState { get; set; }
    [Export] public State DeathState { get; set; }
    [Export] public AnimatedSprite2D Sprite { get; set; }
    [Export] public float MaxFreezeTime { get; set; } = 15f;

    private float curTime = 0f;

    public override State Enter(State previousState)
    {
        curTime = 0f;

        HurtboxCollision.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
        FrozenBoxCollision.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);

        Freezeable.Unfrozen -= Unfrozen; // Avoid duplicate signals
        Freezeable.Unfrozen += Unfrozen;

        Health.HealthChanged -= HealthChanged; // Avoid duplicate signals
        Health.HealthChanged += HealthChanged;

        Sprite.Play("Frozen");

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        curTime += (float)delta;

        if (curTime >= MaxFreezeTime)
        {
            Freezeable.Unfreeze();
        }

        return base.Process(delta);
    }

    public override void Exit(State nextState)
    {
        Freezeable.Unfrozen -= Unfrozen;

        Health.HealthChanged -= HealthChanged;

        base.Exit(nextState);
    }

    private void Unfrozen()
    {
        StateMachine parent = GetParent<StateMachine>();

        parent.ChangeState(UnfrozenState);
    }

    private void HealthChanged(int oldHealth)
    {
        var curHealth = Health.CurrentHealth;

        GD.Print("Current vent health: ", curHealth);

        if (curHealth <= 0)
        {
            StateMachine parent = GetParent<StateMachine>();

            parent.ChangeState(DeathState);
        }
    }

}
