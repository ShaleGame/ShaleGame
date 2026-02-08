using System.Threading.Tasks;
using CrossedDimensions.BoundingBoxes;
using CrossedDimensions.Characters;
using CrossedDimensions.Characters.Controllers;
using CrossedDimensions.Components;
using CrossedDimensions.Entities;
using CrossedDimensions.Entities.Enemies;
using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.Integration.Enemies;

[TestSuite]
[TestCategory("Integration")]
public partial class TurretIntegrationTest
{
    private const string ScenePath = "res://Tests/Integration/Enemies/TurretIntegrationTest.tscn";

    private ISceneRunner _runner;
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

    [BeforeTest]
    public void SetupTest()
    {
        _runner = ISceneRunner.Load(ScenePath);
        _scene = _runner.Scene();

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

    [AfterTest]
    public void TeardownTest()
    {
        _scene?.QueueFree();
        _scene = null;
    }

    [TestCase]
    [RequireGodotRuntime]
    public void GivenScene_WhenLoaded_ShouldInitializeCorrectly()
    {
        AssertThat(_projectile).IsNotNull();
        AssertThat(_projectileHitbox).IsNotNull();
        AssertThat(_projectileDamage).IsNotNull();
        AssertThat(_turret).IsNotNull();
        AssertThat(_turretHealth).IsNotNull();
        AssertThat(_turretHurtbox).IsNotNull();
        AssertThat(_enemyComponent).IsNotNull();
        AssertThat(_enemyController).IsNotNull();
        AssertThat(_character).IsNotNull();

        AssertThat(_projectile.Direction).IsEqual(new Vector2(0, 1));
        AssertThat(_projectile.Speed).IsEqual(256f);
        AssertThat(_projectileDamage.DamageAmount).IsEqual(1000);
        AssertThat(_turretHealth.MaxHealth).IsEqual(100);
        AssertThat(_turretHealth.CurrentHealth).IsEqual(100);
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task GivenProjectile_WhenItHitsTurret_ThenApplyDamageAndEmitSignal()
    {
        AssertSignal(_turretHealth).StartMonitoring();

        float oldHealth = _turretHealth.CurrentHealth;

        await AssertSignal(_turretHealth)
            .IsEmitted(HealthComponent.SignalName.HealthChanged, oldHealth)
            .WithTimeout(300);
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task GivenProjectileDealsLethalDamage_WhenHitsTurret_ThenTurretDies()
    {
        ulong turretId = _turret.GetInstanceId();

        await _runner.SimulateFrames(20, 20);

        AssertThat(Node.IsInstanceIdValid(turretId)).IsFalse();
    }
}
