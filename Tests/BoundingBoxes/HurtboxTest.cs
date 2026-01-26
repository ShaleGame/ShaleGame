using CrossedDimensions.BoundingBoxes;
using CrossedDimensions.Components;
using CrossedDimensions.Characters;
using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.BoundingBoxes;

[TestSuite]
public class HurtboxTest
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
        var result = hurtbox.Hit(hitbox);

        // hit registered
        AssertThat(result).IsTrue();

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

        var result = hurtbox.Hit(hitbox);

        // hit should be considered registered because knockback applied
        AssertThat(result).IsTrue();

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

        var result = hurtbox.Hit(hitbox);

        // hit registered (knockback applied)
        AssertThat(result).IsTrue();

        // no exception and knockback applied: normalized direction is (1,0)
        AssertThat(owner.VelocityFromExternalForces).IsEqual(new Vector2(12, 0));
    }

    // skip test, requires integration test with a test scene and scene runner
    //[TestCase]
    //[RequireGodotRuntime]
    public void Hurtbox_Hit_RespectsIFrameTimer()
    {
        var hitbox = new Hitbox();
        var hurtbox = new Hurtbox();

        var damage = new DamageComponent() { DamageAmount = 5, KnockbackMultiplier = 1f };
        hitbox.DamageComponent = damage;

        var health = new HealthComponent() { MaxHealth = 50 };
        health.CurrentHealth = 50;
        hurtbox.HealthComponent = health;

        var owner = new Character();
        hurtbox.OwnerCharacter = owner;

        var iframe = new Timer();
        iframe.WaitTime = 10f;
        iframe.OneShot = true;
        hurtbox.IFrameTimer = iframe;

        AddNode(hitbox);
        AddNode(hurtbox);
        AddNode(damage);
        AddNode(health);
        AddNode(owner);
        AddNode(iframe);

        hitbox.GlobalPosition = new Vector2(0, 0);
        hurtbox.GlobalPosition = new Vector2(3, 0);

        iframe.Start();

        var result = hurtbox.Hit(hitbox);
        AssertThat(result).IsFalse();

        iframe.Stop();

        var result2 = hurtbox.Hit(hitbox);
        AssertThat(result2).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Hurtbox_ShouldIgnoreHitFrom_ReturnsTrueWhenSameOwner()
    {
        var hurtbox = new Hurtbox();
        var hitbox = new Hitbox();

        var character = new Character();
        hurtbox.OwnerCharacter = character;
        hitbox.OwnerCharacter = character;

        AddNode(hurtbox);
        AddNode(hitbox);
        AddNode(character);

        AssertThat(hurtbox.ShouldIgnoreHitFrom(hitbox)).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Hurtbox_ShouldIgnoreHitFrom_ReturnsFalseWhenDifferentOwners()
    {
        var hurtbox = new Hurtbox();
        var hitbox = new Hitbox();

        var character1 = new Character();
        var character2 = new Character();
        hurtbox.OwnerCharacter = character1;
        hitbox.OwnerCharacter = character2;

        AddNode(hurtbox);
        AddNode(hitbox);
        AddNode(character1);
        AddNode(character2);

        AssertThat(hurtbox.ShouldIgnoreHitFrom(hitbox)).IsFalse();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Hurtbox_ShouldIgnoreHitFrom_ReturnsFalseWhenNoOwners()
    {
        var hurtbox = new Hurtbox();
        var hitbox = new Hitbox();

        hurtbox.OwnerCharacter = null;
        hitbox.OwnerCharacter = null;

        AddNode(hurtbox);
        AddNode(hitbox);

        AssertThat(hurtbox.ShouldIgnoreHitFrom(hitbox)).IsFalse();
    }
}
