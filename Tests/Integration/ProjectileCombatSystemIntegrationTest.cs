using System.Threading.Tasks;
using CrossedDimensions.Components;
using CrossedDimensions.BoundingBoxes;
using CrossedDimensions.Entities;
using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.Integration;

[TestSuite]
[TestCategory("Integration")]
public partial class ProjectileCombatSystemIntegrationTest
{
    private ISceneRunner _runner;
    private Node _scene;
    private Projectile _projectile;
    private Hitbox _hitbox;
    private DamageComponent _damageComponent;
    private Characters.Character _character;
    private Hurtbox _hurtbox;
    private HealthComponent _healthComponent;

    [BeforeTest]
    public void SetupTest()
    {
        string path = "res://Tests/Integration/ProjectileCombatSystemIntegrationTest.tscn";
        _runner = ISceneRunner.Load(path);
        _scene = _runner.Scene();

        _projectile = _scene.GetNode<Projectile>("Projectile");
        _hitbox = _scene.GetNode<Hitbox>("Projectile/Hitbox");
        _damageComponent = _scene.GetNode<DamageComponent>("Projectile/Hitbox/DamageComponent");
        _character = _scene.GetNode<Characters.Character>("Character");
        _hurtbox = _scene.GetNode<Hurtbox>("Character/Hurtbox");
        _healthComponent = _scene.GetNode<HealthComponent>("Character/HealthComponent");
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
        // this is mostly a smoke test to ensure the scene loads without errors

        AssertThat(_projectile).IsNotNull();
        AssertThat(_hitbox).IsNotNull();
        AssertThat(_damageComponent).IsNotNull();
        AssertThat(_character).IsNotNull();
        AssertThat(_hurtbox).IsNotNull();
        AssertThat(_healthComponent).IsNotNull();

        AssertThat(_projectile.Direction).IsEqual(Vector2.Right);
        AssertThat(_projectile.Speed).IsEqual(256f);
        AssertThat(_damageComponent.DamageAmount).IsEqual(10);
        AssertThat(_damageComponent.KnockbackMultiplier).IsEqual(1f);
        AssertThat(_healthComponent.MaxHealth).IsEqual(100);
        AssertThat(_healthComponent.CurrentHealth).IsEqual(100);
        AssertThat(_healthComponent.IsAlive).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task GivenProjectile_WhenItHitsCharacter_ThenReduceHealth()
    {
        AssertSignal(_healthComponent).StartMonitoring();

        await AssertSignal(_healthComponent)
            .IsEmitted(HealthComponent.SignalName.HealthChanged, 100)
            .WithTimeout(100);

        AssertThat(_healthComponent.CurrentHealth).IsEqual(90);
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task GivenProjectile_WhenItHitsCharacter_ThenEmitSignal()
    {
        AssertSignal(_hitbox).StartMonitoring();

        await AssertSignal(_hitbox)
            .IsEmitted(Hitbox.SignalName.Hit)
            .WithTimeout(100);
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task GivenProjectile_WhenItHitsCharacter_ThenApplyKnockback()
    {
        _projectile.Hitbox.DamageComponent.KnockbackMultiplier = 1.0f;

        await _runner.SimulateFrames(5, 20);

        Vector2 velocityAfterHit = _character.VelocityFromExternalForces;
        AssertThat(velocityAfterHit.X).IsGreater(0);
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task GivenNoKnockback_WhenProjectileHitsCharacter_ThenDoNotApplyKnockback()
    {
        _projectile.Hitbox.DamageComponent.KnockbackMultiplier = 0.0f;

        await _runner.SimulateFrames(5, 20);

        Vector2 velocityAfterHit = _character.VelocityFromExternalForces;
        AssertThat(velocityAfterHit.X).IsEqual(0);
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task GivenProjectileThatFreesOnHit_WhenItHitsCharacter_ThenFreeProjectile()
    {
        ulong id = _projectile.GetInstanceId();
        _projectile.FreeOnHit = true;
        await _runner.SimulateFrames(5, 20);

        AssertThat(Node.IsInstanceIdValid(id)).IsFalse();
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task GivenNegativeKnockback_WhenProjectileHitsCharacter_ThenPullInsteadOfPush()
    {
        _projectile.Hitbox.DamageComponent.KnockbackMultiplier = -1.0f;

        await _runner.SimulateFrames(5, 20);

        Vector2 velocityAfterHit = _character.VelocityFromExternalForces;
        AssertThat(velocityAfterHit.X).IsLess(0);
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task GivenOwnedProjectile_WhenHitsOwner_ThenDoNotApplyKnockback()
    {
        _projectile.OwnerCharacter = _character;
        _projectile.Hitbox.CanHitSelf = false;

        await _runner.SimulateFrames(5, 20);

        Vector2 velocityAfterHit = _character.VelocityFromExternalForces;
        AssertThat(velocityAfterHit.X).IsEqual(0);
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task GivenOwnedProjectile_WhenHitsOwner_ThenDoNotApplyDamage()
    {
        _projectile.OwnerCharacter = _character;
        _projectile.Hitbox.CanHitSelf = false;

        await _runner.SimulateFrames(5, 20);

        AssertThat(_healthComponent.CurrentHealth).IsEqual(100);
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task GivenOwnedProjectileCanHitSelf_WhenHitsOwner_ThenApplyKnockback()
    {
        _projectile.OwnerCharacter = _character;
        _projectile.Hitbox.CanHitSelf = true;

        await _runner.SimulateFrames(5, 20);

        Vector2 velocityAfterHit = _character.VelocityFromExternalForces;
        AssertThat(velocityAfterHit.X).IsGreater(0);
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task GivenOwnedProjectileCanHitSelf_WhenHitsOwner_ThenDoNotApplyDamage()
    {
        _projectile.OwnerCharacter = _character;
        _projectile.Hitbox.CanHitSelf = true;

        await _runner.SimulateFrames(5, 20);

        AssertThat(_healthComponent.CurrentHealth).IsEqual(100);
    }
}
