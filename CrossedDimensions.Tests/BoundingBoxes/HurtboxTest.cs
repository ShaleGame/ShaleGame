using CrossedDimensions.BoundingBoxes;
using CrossedDimensions.Components;
using CrossedDimensions.Characters;
using Godot;

namespace CrossedDimensions.Tests.BoundingBoxes;

[Collection("GodotHeadless")]
public class HurtboxTest(GodotHeadlessFixedFpsFixture godot)
{
    [Theory]
    [InlineData(0f, 10f, 1f)]
    [InlineData(2.5f, 10f, 0.75f)]
    [InlineData(5f, 10f, 0.5f)]
    [InlineData(10f, 10f, 0f)]
    public void CalculateFalloffFactor_InterpolatesLinearly(float a, float b, float x)
    {
        var f = Hurtbox.CalculateFalloffFactor(a, b);
        f.ShouldBe(x);
    }

    [Fact]
    public void CalculateFalloffFactor_WhenZero_ReturnsZero()
    {
        Hurtbox.CalculateFalloffFactor(1f, 0f).ShouldBe(0f);
    }

    [Fact]
    public void CalculateFalloffFactor_WhenBeyondMax_Clamps()
    {
        Hurtbox.CalculateFalloffFactor(15f, 10f).ShouldBe(0f);
    }

    [Fact]
    public void CalculateFalloffFactor_WhenNegativeDistance_Clamps()
    {
        Hurtbox.CalculateFalloffFactor(-5f, 10f).ShouldBe(1f);
    }

    [Fact]
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

        godot.Tree.Root.AddChild(owner);
        godot.Tree.Root.AddChild(hitbox);
        godot.Tree.Root.AddChild(hurtbox);
        godot.Tree.Root.AddChild(health);
        godot.Tree.Root.AddChild(damage);

        // place nodes so direction is along +X and force is predictable
        hitbox.GlobalPosition = new Vector2(0, 0);
        hurtbox.GlobalPosition = new Vector2(10, 0);

        // call Hit directly (unit test of Hurtbox behavior)
        var result = hurtbox.Hit(hitbox);

        // hit registered
        result.ShouldBeTrue();

        // damage applied: 100 - 10
        health.CurrentHealth.ShouldBe(90);

        // knockback: direction (1,0) * damage * knockback = 1 * 10 * 2 = 20 on X
        owner.VelocityFromExternalForces.ShouldBe(new Vector2(20, 0));
    }

    [Fact]
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

        godot.Tree.Root.AddChild(hitbox);
        godot.Tree.Root.AddChild(hurtbox);
        godot.Tree.Root.AddChild(health);
        godot.Tree.Root.AddChild(owner);

        hitbox.GlobalPosition = new Vector2(0, 0);
        hurtbox.GlobalPosition = new Vector2(5, 0);

        var result = hurtbox.Hit(hitbox);

        // hit should be considered registered because knockback applied
        result.ShouldBeTrue();

        // damage should be prevented for self-hit
        health.CurrentHealth.ShouldBe(100);

        // but knockback should still be applied (direction normalized * damage * knockback = 1 * 10 * 1 = 10)
        owner.VelocityFromExternalForces.ShouldBe(new Vector2(10, 0));
    }

    [Fact]
    public void Hurtbox_HitWithoutHealthComponent_AppliesOnlyKnockback()
    {
        var hitbox = new Hitbox();
        var hurtbox = new Hurtbox();

        var damage = new DamageComponent() { DamageAmount = 8, KnockbackMultiplier = 1.5f };
        hitbox.DamageComponent = damage;

        hurtbox.HealthComponent = null;

        var owner = new Character();
        hurtbox.OwnerCharacter = owner;

        godot.Tree.Root.AddChild(hitbox);
        godot.Tree.Root.AddChild(hurtbox);
        godot.Tree.Root.AddChild(damage);
        godot.Tree.Root.AddChild(owner);

        hitbox.GlobalPosition = new Vector2(0, 0);
        hurtbox.GlobalPosition = new Vector2(4, 0);

        var result = hurtbox.Hit(hitbox);

        // hit registered (knockback applied)
        result.ShouldBeTrue();

        // no exception and knockback applied: normalized direction is (1,0)
        owner.VelocityFromExternalForces.ShouldBe(new Vector2(12, 0));
    }

    [Fact]
    public void Hurtbox_ShouldIgnoreHitFrom_ReturnsTrueWhenSameOwner()
    {
        var hurtbox = new Hurtbox();
        var hitbox = new Hitbox();

        var character = new Character();
        hurtbox.OwnerCharacter = character;
        hitbox.OwnerCharacter = character;

        godot.Tree.Root.AddChild(hurtbox);
        godot.Tree.Root.AddChild(hitbox);
        godot.Tree.Root.AddChild(character);

        hurtbox.ShouldIgnoreHitFrom(hitbox).ShouldBeTrue();
    }

    [Fact]
    public void Hurtbox_ShouldIgnoreHitFrom_ReturnsFalseWhenDifferentOwners()
    {
        var hurtbox = new Hurtbox();
        var hitbox = new Hitbox();

        var character1 = new Character();
        var character2 = new Character();
        hurtbox.OwnerCharacter = character1;
        hitbox.OwnerCharacter = character2;

        godot.Tree.Root.AddChild(hurtbox);
        godot.Tree.Root.AddChild(hitbox);
        godot.Tree.Root.AddChild(character1);
        godot.Tree.Root.AddChild(character2);

        hurtbox.ShouldIgnoreHitFrom(hitbox).ShouldBeFalse();
    }

    [Fact]
    public void Hurtbox_ShouldIgnoreHitFrom_ReturnsFalseWhenNoOwners()
    {
        var hurtbox = new Hurtbox();
        var hitbox = new Hitbox();

        hurtbox.OwnerCharacter = null;
        hitbox.OwnerCharacter = null;

        godot.Tree.Root.AddChild(hurtbox);
        godot.Tree.Root.AddChild(hitbox);

        hurtbox.ShouldIgnoreHitFrom(hitbox).ShouldBeFalse();
    }
}
