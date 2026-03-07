using Godot;

namespace CrossedDimensions.States.Characters;

public sealed partial class CharacterMergeHoldState : CharacterState
{
    [Export]
    public State IdleState { get; set; }

    private float _healAccumulator;

    public override State Enter(State previousState)
    {
        _healAccumulator = 0f;
        CharacterContext.VelocityFromExternalForces = Vector2.Zero;
        CharacterContext.Velocity = Vector2.Zero;
        CharacterContext.Cloneable.Merge();
        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        var cloneable = CharacterContext.Cloneable;
        if (cloneable.HealingPool > 0)
        {
            float maxHealth = CharacterContext.Health.MaxHealth;
            float healPerSecond = maxHealth / (float)cloneable.HealTime;
            float healAmount = healPerSecond * (float)delta;

            float actualHeal = Mathf.Min(healAmount, cloneable.HealingPool);
            float excessDrain = actualHeal - Mathf.Min(actualHeal, maxHealth - CharacterContext.Health.CurrentHealth);
            actualHeal -= excessDrain;

            cloneable.DrainHealingPool(actualHeal + excessDrain);
            _healAccumulator += actualHeal;

            int wholeHeal = (int)_healAccumulator;
            if (wholeHeal > 0)
            {
                CharacterContext.Health.CurrentHealth += wholeHeal;
                _healAccumulator -= wholeHeal;
            }

            GD.Print($"Healing: {CharacterContext.Health.CurrentHealth}, Healing Pool: {cloneable.HealingPool}");

            if (cloneable.HealingPool <= 0)
            {
                return IdleState;
            }
        }
        else
        {
            return IdleState;
        }

        return base.Process(delta);
    }

    public override State PhysicsProcess(double delta)
    {
        if (CharacterContext.Controller.IsSplitReleased)
        {
            CharacterContext.Cloneable.ClearHealingPool();
            CharacterContext.Cloneable.Merge();
            return IdleState;
        }

        CharacterContext.VelocityFromExternalForces = Vector2.Zero;
        CharacterContext.Velocity = Vector2.Zero;
        CharacterContext.MoveAndSlide();

        return base.PhysicsProcess(delta);
    }

    public override void Exit(State nextState)
    {
        CharacterContext.Cloneable.ClearHealingPool();
        _healAccumulator = 0f;
        base.Exit(nextState);
    }
}
