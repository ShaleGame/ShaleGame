using CrossedDimensions.BoundingBoxes;
using CrossedDimensions.Components;
using CrossedDimensions.Characters;
using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.BoundingBoxes;

[TestSuite]
public partial class HurtboxTest
{
    [TestCase(0f, 10f, 1f)]
    [TestCase(2.5f, 10f, 0.75f)]
    [TestCase(5f, 10f, 0.5f)]
    [TestCase(10f, 10f, 0f)]
    public void CalculateFalloffFactor_InterpolatesLinearly(float a, float b, float x)
    {
        var f = Hurtbox.CalculateFalloffFactor(a, b);
        // 1 - (2.5/10) = 0.75
        AssertThat(f).IsEqual(x);
    }

    [TestCase]
    public void CalculateFalloffFactor_WhenZero_ReturnsZero()
    {
        AssertThat(Hurtbox.CalculateFalloffFactor(1f, 0f)).IsEqual(0f);
    }

    [TestCase]
    public void CalculateFalloffFactor_WhenBeyondMax_Clamps()
    {
        AssertThat(Hurtbox.CalculateFalloffFactor(15f, 10f)).IsEqual(0f);
    }

    [TestCase]
    public void CalculateFalloffFactor_WhenNegativeDistance_Clamps()
    {
        AssertThat(Hurtbox.CalculateFalloffFactor(-5f, 10f)).IsEqual(1f);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Hurtbox_Hit_AppliesDamageAndKnockback()
    {
        var hitbox = new Hitbox();
        var hurtbox = new Hurtbox();

        var damage = new DamageComponent() { DamageAmount = 10, KnockbackMultiplier = 2f };
        hitbox.DamageComponent = damage;

        var health = new HealthComponent() { MaxHealth = 100 };
        health.CurrentHealth = 100;
        hurtbox.HealthComponent = health;

        var owner = new Character();
        hurtbox.OwnerCharacter = owner;

        AddNode(owner);
        AddNode(hitbox);
        AddNode(hurtbox);
        AddNode(health);
        AddNode(damage);

        // place nodes so direction is along +X and force is predictable
        hitbox.GlobalPosition = new Vector2(0, 0);
        hurtbox.GlobalPosition = new Vector2(10, 0);

        // call Hit directly (unit test of Hurtbox behavior)
        hurtbox.Hit(hitbox);

        // damage applied: 100 - 10
        AssertThat(health.CurrentHealth).IsEqual(90);

        // knockback: direction (1,0) * damage * knockback = 1 * 10 * 2 = 20 on X
        AssertThat(owner.VelocityFromExternalForces).IsEqual(new Vector2(20, 0));
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Hurtbox_Hit_PreventsSelfDamage_ButAppliesKnockback()
    {
        var hitbox = new Hitbox();
        var hurtbox = new Hurtbox();

        var damage = new DamageComponent() { DamageAmount = 10, KnockbackMultiplier = 1f };
        hitbox.DamageComponent = damage;
        hitbox.AddChild(damage);

        var health = new HealthComponent() { MaxHealth = 100 };
        health.CurrentHealth = 100;
        hurtbox.HealthComponent = health;

        var owner = new Character();
        hurtbox.OwnerCharacter = owner;
        hitbox.OwnerCharacter = owner; // same owner -> should not take damage

        AddNode(hitbox);
        AddNode(hurtbox);
        AddNode(damage);
        AddNode(health);
        AddNode(owner);

        hitbox.GlobalPosition = new Vector2(0, 0);
        hurtbox.GlobalPosition = new Vector2(5, 0);

        hurtbox.Hit(hitbox);

        // damage should be prevented for self-hit
        AssertThat(health.CurrentHealth).IsEqual(100);

        // but knockback should still be applied (direction normalized * damage * knockback = 1 * 10 * 1 = 10)
        AssertThat(owner.VelocityFromExternalForces).IsEqual(new Vector2(10, 0));
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Hurtbox_HitWithoutHealthComponent_AppliesOnlyKnockback()
    {
        var hitbox = new Hitbox();
        var hurtbox = new Hurtbox();

        var damage = new DamageComponent() { DamageAmount = 8, KnockbackMultiplier = 1.5f };
        hitbox.DamageComponent = damage;

        hurtbox.HealthComponent = null;

        var owner = new Character();
        hurtbox.OwnerCharacter = owner;

        AddNode(hitbox);
        AddNode(hurtbox);
        AddNode(damage);
        AddNode(owner);

        hitbox.GlobalPosition = new Vector2(0, 0);
        hurtbox.GlobalPosition = new Vector2(4, 0);

        hurtbox.Hit(hitbox);

        // no exception and knockback applied: normalized direction is (1,0)
        AssertThat(owner.VelocityFromExternalForces).IsEqual(new Vector2(12, 0));
    }
}
