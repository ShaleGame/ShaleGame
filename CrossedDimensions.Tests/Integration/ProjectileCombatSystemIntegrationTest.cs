using Godot;
using CrossedDimensions.Components;
using CrossedDimensions.BoundingBoxes;
using CrossedDimensions.Entities;

namespace CrossedDimensions.Tests.Integration;

[Collection("GodotHeadless")]
public class ProjectileCombatSystemIntegrationTest : System.IDisposable
{
    private const string ScenePath = $"{Paths.TestPath}/Integration/ProjectileCombatSystemIntegrationTest.tscn";

    private readonly GodotHeadlessFixedFpsFixture _godot;
    private Node _scene;
    private Projectile _projectile;
    private Hitbox _hitbox;
    private DamageComponent _damageComponent;
    private Characters.Character _character;
    private Hurtbox _hurtbox;
    private HealthComponent _healthComponent;

    public ProjectileCombatSystemIntegrationTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;

        var packed = ResourceLoader.Load<PackedScene>(ScenePath);
        _scene = packed.Instantiate() as Node;
        _godot.Tree.Root.AddChild(_scene);

        _projectile = _scene.GetNode<Projectile>("Projectile");
        _hitbox = _scene.GetNode<Hitbox>("Projectile/Hitbox");
        _damageComponent = _scene.GetNode<DamageComponent>("Projectile/Hitbox/DamageComponent");
        _character = _scene.GetNode<Characters.Character>("Character");
        _hurtbox = _scene.GetNode<Hurtbox>("Character/Hurtbox");
        _healthComponent = _scene.GetNode<HealthComponent>("Character/HealthComponent");
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
        _hitbox.ShouldNotBeNull();
        _damageComponent.ShouldNotBeNull();
        _character.ShouldNotBeNull();
        _hurtbox.ShouldNotBeNull();
        _healthComponent.ShouldNotBeNull();

        _projectile.Direction.ShouldBe(Vector2.Right);
        _projectile.Speed.ShouldBe(256f);
        _damageComponent.DamageAmount.ShouldBe(10);
        _damageComponent.KnockbackMultiplier.ShouldBe(1f);
        _healthComponent.MaxHealth.ShouldBe(100);
        _healthComponent.CurrentHealth.ShouldBe(100);
        _healthComponent.IsAlive.ShouldBeTrue();
    }

    [Fact]
    public void GivenProjectile_WhenItHitsCharacter_ThenReduceHealth()
    {
        bool hit = false;
        _hitbox.Hit += () => hit = true;

        _godot.GodotInstance.IterateUntil(() => hit, 300);

        _healthComponent.CurrentHealth.ShouldBe(90);
    }

    [Fact]
    public void GivenProjectile_WhenItHitsCharacter_ThenEmitSignal()
    {
        bool hit = false;
        _hitbox.Hit += () => hit = true;

        _godot.GodotInstance
            .IterateUntil(() => hit, 300)
            .ShouldBeTrue();
    }

    [Fact]
    public void GivenProjectile_WhenItHitsCharacter_ThenApplyKnockback()
    {
        _projectile.Hitbox.DamageComponent.KnockbackMultiplier = 1.0f;

        _godot.GodotInstance.Iteration(20);

        Vector2 velocityAfterHit = _character.VelocityFromExternalForces;
        velocityAfterHit.X.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void GivenNoKnockback_WhenProjectileHitsCharacter_ThenDoNotApplyKnockback()
    {
        _projectile.Hitbox.DamageComponent.KnockbackMultiplier = 0.0f;

        _godot.GodotInstance.Iteration(20);

        Vector2 velocityAfterHit = _character.VelocityFromExternalForces;
        velocityAfterHit.X.ShouldBe(0);
    }

    [Fact]
    public void GivenProjectileThatFreesOnHit_WhenItHitsCharacter_ThenFreeProjectile()
    {
        ulong id = _projectile.GetInstanceId();
        _projectile.FreeOnHit = true;

        _godot.GodotInstance.Iteration(20);

        Node.IsInstanceIdValid(id).ShouldBeFalse();
    }

    [Fact]
    public void GivenNegativeKnockback_WhenProjectileHitsCharacter_ThenPullInsteadOfPush()
    {
        _projectile.Hitbox.DamageComponent.KnockbackMultiplier = -1.0f;

        _godot.GodotInstance.Iteration(20);

        Vector2 velocityAfterHit = _character.VelocityFromExternalForces;
        velocityAfterHit.X.ShouldBeLessThan(0);
    }

    [Fact]
    public void GivenOwnedProjectile_WhenHitsOwner_ThenDoNotApplyKnockback()
    {
        _projectile.OwnerCharacter = _character;
        _projectile.Hitbox.CanHitSelf = false;

        _godot.GodotInstance.Iteration(20);

        Vector2 velocityAfterHit = _character.VelocityFromExternalForces;
        velocityAfterHit.X.ShouldBe(0);
    }

    [Fact]
    public void GivenOwnedProjectile_WhenHitsOwner_ThenDoNotApplyDamage()
    {
        _projectile.OwnerCharacter = _character;
        _projectile.Hitbox.CanHitSelf = false;

        _godot.GodotInstance.Iteration(20);

        _healthComponent.CurrentHealth.ShouldBe(100);
    }

    [Fact]
    public void GivenOwnedProjectileCanHitSelf_WhenHitsOwner_ThenApplyKnockback()
    {
        _projectile.OwnerCharacter = _character;
        _projectile.Hitbox.CanHitSelf = true;

        _godot.GodotInstance.Iteration(20);

        Vector2 velocityAfterHit = _character.VelocityFromExternalForces;
        velocityAfterHit.X.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void GivenOwnedProjectileCanHitSelf_WhenHitsOwner_ThenDoNotApplyDamage()
    {
        _projectile.OwnerCharacter = _character;
        _projectile.Hitbox.CanHitSelf = true;

        _godot.GodotInstance
            .IterateUntil(() => _healthComponent.CurrentHealth != 100, 20)
            .ShouldBeFalse();
    }
}
