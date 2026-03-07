using System;
using CrossedDimensions.BoundingBoxes;
using CrossedDimensions.Components;
using CrossedDimensions.Characters;
using Godot;

namespace CrossedDimensions.Tests.Integration;

public class CloneAttacksEnemySetup
{
    public readonly Character Original;
    public readonly Character Clone;
    public readonly Character Enemy;
    public readonly CloneableComponent OriginalCloneable;
    public readonly CloneableComponent CloneCloneable;
    public readonly Hitbox Hitbox;
    public readonly DamageComponent Damage;
    public readonly Hurtbox Hurtbox;
    public readonly HealthComponent EnemyHealth;

    public CloneAttacksEnemySetup(GodotHeadlessFixedFpsFixture godot, Node scene, int damageAmount = 40)
    {
        var packed = ResourceLoader.Load<PackedScene>("res://Characters/Character.tscn");

        Original = packed.Instantiate<Character>();
        Clone = packed.Instantiate<Character>();
        Enemy = packed.Instantiate<Character>();

        scene.AddChild(Original);
        scene.AddChild(Clone);
        scene.AddChild(Enemy);

        OriginalCloneable = Original.Cloneable;
        OriginalCloneable.Clone = Clone;

        CloneCloneable = Clone.Cloneable;
        CloneCloneable.Original = Original;

        Hitbox = new Hitbox();
        Damage = new DamageComponent { DamageAmount = damageAmount };
        Hitbox.DamageComponent = Damage;
        Hitbox.OwnerCharacter = Clone;

        EnemyHealth = new HealthComponent { MaxHealth = 100, CurrentHealth = 100 };
        Hurtbox = new Hurtbox { HealthComponent = EnemyHealth };
        Hurtbox.OwnerCharacter = Enemy;

        godot.Tree.Root.AddChild(Hitbox);
        godot.Tree.Root.AddChild(Hurtbox);
        godot.Tree.Root.AddChild(EnemyHealth);
        godot.Tree.Root.AddChild(Damage);

        Hitbox.GlobalPosition = new Vector2(0, 0);
        Hurtbox.GlobalPosition = new Vector2(10, 0);
    }
}

[Collection("GodotHeadless")]
public class HealingPoolIntegrationTest : IDisposable
{
    private const string ScenePath = "res://Characters/Character.tscn";

    private readonly GodotHeadlessFixedFpsFixture _godot;
    private Node _scene;

    public HealingPoolIntegrationTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;
        _scene = new Node2D();
        _godot.Tree.Root.AddChild(_scene);
    }

    public void Dispose()
    {
        _scene?.QueueFree();
        _scene = null;
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(40)]
    public void Hurtbox_Hit_WhenCloneDealsDamage_ShouldFeedHealingPool(int damage)
    {
        var setup = new CloneAttacksEnemySetup(_godot, _scene);

        setup.Damage.DamageAmount = damage;
        setup.Hurtbox.Hit(setup.Hitbox);

        float expected = damage * setup.CloneCloneable.HealEfficiency;
        setup.OriginalCloneable.HealingPool.ShouldBe(expected);
    }

    [Fact]
    public void Hurtbox_Hit_WhenCloneDealsDamage_ShouldEmitHealPoolChangeSignal()
    {
        var setup = new CloneAttacksEnemySetup(_godot, _scene);

        bool signalEmitted = false;
        setup.OriginalCloneable.HealingPoolChanged += (_, _) =>
        {
            signalEmitted = true;
        };

        setup.Hurtbox.Hit(setup.Hitbox);

        signalEmitted.ShouldBeTrue();
    }

    [Fact]
    public void Hurtbox_Hit_WhenNonCloneDealsDamage_ShouldNotFeedHealingPool()
    {
        var packed = ResourceLoader.Load<PackedScene>(ScenePath);
        var attacker = packed.Instantiate<Character>();
        _scene.AddChild(attacker);

        var hitbox = new Hitbox();
        var damage = new DamageComponent { DamageAmount = 40 };
        hitbox.DamageComponent = damage;
        hitbox.OwnerCharacter = attacker;

        var victim = packed.Instantiate<Character>();
        _scene.AddChild(victim);
        var victimHealth = new HealthComponent { MaxHealth = 100, CurrentHealth = 100 };
        var hurtbox = new Hurtbox { HealthComponent = victimHealth };
        hurtbox.OwnerCharacter = victim;

        _godot.Tree.Root.AddChild(hitbox);
        _godot.Tree.Root.AddChild(hurtbox);
        _godot.Tree.Root.AddChild(victimHealth);
        _godot.Tree.Root.AddChild(damage);

        hitbox.GlobalPosition = new Vector2(0, 0);
        hurtbox.GlobalPosition = new Vector2(10, 0);

        hurtbox.Hit(hitbox);

        attacker.Cloneable.HealingPool.ShouldBe(0f);
    }

    [Fact]
    public void Hurtbox_Hit_WhenCloneDealsDamage_ShouldCapHealingPoolAtMaxHealth()
    {
        var setup = new CloneAttacksEnemySetup(_godot, _scene);
        var maxHealth = setup.Original.Health.MaxHealth;
        setup.OriginalCloneable.AddToHealingPool(maxHealth - 5f);

        setup.Hurtbox.Hit(setup.Hitbox);

        setup.OriginalCloneable.HealingPool.ShouldBe((float)maxHealth);
    }

    [Fact]
    public void Hurtbox_Hit_WhenZeroDamage_ShouldNotFeedHealingPool()
    {
        var setup = new CloneAttacksEnemySetup(_godot, _scene, damageAmount: 0);

        setup.Hurtbox.Hit(setup.Hitbox);

        setup.OriginalCloneable.HealingPool.ShouldBe(0f);
    }
}
