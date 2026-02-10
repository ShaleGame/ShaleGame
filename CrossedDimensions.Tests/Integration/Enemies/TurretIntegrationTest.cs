using System.Threading.Tasks;
using CrossedDimensions.BoundingBoxes;
using CrossedDimensions.Characters;
using CrossedDimensions.Characters.Controllers;
using CrossedDimensions.Components;
using CrossedDimensions.Entities;
using CrossedDimensions.Entities.Enemies;
using Godot;
using System;
using twodog.xunit;
using Xunit;
using Shouldly;

namespace CrossedDimensions.Tests.Integration.Enemies;

[Collection("GodotHeadless")]
public class TurretIntegrationTest : IDisposable
{
    private const string ScenePath = $"{Paths.TestPath}/Integration/Enemies/TurretIntegrationTest.tscn";

    private readonly GodotHeadlessFixture _godot;
    private Node _scene;
    private Projectile _projectile;
    private Hitbox _projectileHitbox;
    private DamageComponent _projectileDamage;
    private Character _turret;
    private HealthComponent _turretHealth;
    private Hurtbox _turretHurtbox;
    private EnemyComponent _enemyComponent;
    private EnemyController _enemyController;
    private Character _character;

    public TurretIntegrationTest(GodotHeadlessFixture godot)
    {
        _godot = godot;
        _scene = null;

        var packed = ResourceLoader.Load<PackedScene>(ScenePath);
        _scene = packed.Instantiate() as Node;
        _godot.Tree.Root.AddChild(_scene);

        _projectile = _scene.GetNode<Projectile>("Projectile");
        _projectileHitbox = _scene.GetNode<Hitbox>("Projectile/Hitbox");
        _projectileDamage = _scene.GetNode<DamageComponent>("Projectile/Hitbox/DamageComponent");

        _turret = _scene.GetNode<Character>("DebugTurret");
        _turretHealth = _turret.GetNode<HealthComponent>("HealthComponent");
        _turretHurtbox = _turret.GetNode<Hurtbox>("Hurtbox");
        _enemyComponent = _turret.GetNode<EnemyComponent>("EnemyComponent");
        _enemyController = _turret.Controller as EnemyController;

        _character = _scene.GetNode<Character>("Character");
    }

    public void Dispose()
    {
        _scene?.QueueFree();
        _scene = null;
    }

    [Fact]
    public void GivenScene_WhenLoaded_ShouldInitializeCorrectly()
    {
        _projectile.ShouldNotBeNull();
        _projectileHitbox.ShouldNotBeNull();
        _projectileDamage.ShouldNotBeNull();
        _turret.ShouldNotBeNull();
        _turretHealth.ShouldNotBeNull();
        _turretHurtbox.ShouldNotBeNull();
        _enemyComponent.ShouldNotBeNull();
        _enemyController.ShouldNotBeNull();
        _character.ShouldNotBeNull();

        _projectile.Direction.ShouldBe(new Vector2(0, 1));
        _projectile.Speed.ShouldBe(256f);
        _projectileDamage.DamageAmount.ShouldBe(1000);
        _turretHealth.MaxHealth.ShouldBe(100);
        _turretHealth.CurrentHealth.ShouldBe(100);
    }

    [Fact]
    public void GivenProjectile_WhenItHitsTurret_ThenApplyDamageAndEmitSignal()
    {
        int oldHealth = _turretHealth.CurrentHealth;

        bool fired = false;

        _turretHealth.HealthChanged += (int oldHealth) =>
        {
            fired = true;
        };

        _godot.GodotInstance
            .IterateUntil(() => fired, 300)
            .ShouldBeTrue();
    }

    [Fact]
    public void GivenProjectileDealsLethalDamage_WhenHitsTurret_ThenTurretDies()
    {
        ulong turretId = _turret.GetInstanceId();

        _godot.GodotInstance
            .IterateUntil(() => !Node.IsInstanceIdValid(turretId), 300)
            .ShouldBeTrue();
    }

    [Fact]
    public void GivenCharacterInRange_WhenTurretUpdates_ThenAcquireTarget()
    {
        // free projectile so we can safely read turret state, since the
        // projectile would hit and destroy the turret
        _projectile.QueueFree();

        _godot.GodotInstance
            .IterateUntil(() => _enemyComponent.HasTarget, 300)
            .ShouldBeTrue();
    }

    [Fact]
    public void GivenCharacterOutOfRange_WhenTurretUpdates_ThenLoseTarget()
    {
        _projectile.QueueFree();

        _godot.GodotInstance
            .IterateUntil(() => _enemyComponent.HasTarget, 300)
            .ShouldBeTrue();

        var seekNode = _turret.GetNode("BrainStateMachine/Not found player/Seek player");
        float detectionRange = seekNode.Get("detection_range").As<float>();

        float pos = detectionRange * 4f;
        _character.GlobalPosition = _turret.GlobalPosition + new Vector2(pos, pos);

        _godot.GodotInstance
            .IterateUntil(() => !_enemyComponent.HasTarget, 300)
            .ShouldBeTrue();
    }

    [Fact]
    public void GivenCharacterInRange_WhenTurretAcquiresTarget_ThenFireInput()
    {
        _projectile.QueueFree();

        _godot.GodotInstance
            .IterateUntil(() => _enemyController.IsMouse1Held, 300)
            .ShouldBeTrue();
    }
}
