using CrossedDimensions.Characters;
using CrossedDimensions.Entities;
using Godot;
using Shouldly;
using Xunit;

namespace CrossedDimensions.Tests.Integration;

[Collection("GodotHeadless")]
public class ProjectileHierarchyIntegrationTest : System.IDisposable
{
    private const string ScenePath = $"{Paths.TestPath}/Integration/ProjectileHierarchyIntegrationTest.tscn";

    private readonly GodotHeadlessFixedFpsFixture _godot;
    private Node _scene;
    private Projectile _projectileA;
    private Projectile _projectileB;

    public ProjectileHierarchyIntegrationTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;

        var packed = ResourceLoader.Load<PackedScene>(ScenePath);
        _scene = packed.Instantiate();
        _godot.Tree.Root.AddChild(_scene);

        _projectileA = _scene.GetNode<Projectile>("ProjectileA");
        _projectileB = _scene.GetNode<Projectile>("ProjectileB");
    }

    public void Dispose()
    {
        _scene?.QueueFree();
        _scene = null;
    }

    [Fact]
    public void GivenHigherPenetrationProjectile_WhenProjectilesCollide_ThenLowerPenetrationProjectileIsFreed()
    {
        var ownerA = new Character();
        var ownerB = new Character();
        _projectileA.OwnerCharacter = ownerA;
        _projectileB.OwnerCharacter = ownerB;
        _projectileA.Penetration = 2;
        _projectileB.Penetration = 1;

        ulong projectileAId = _projectileA.GetInstanceId();
        ulong projectileBId = _projectileB.GetInstanceId();

        _godot.GodotInstance.IterateUntil(() => !Node.IsInstanceIdValid(projectileBId), 30)
            .ShouldBeTrue();

        Node.IsInstanceIdValid(projectileAId).ShouldBeTrue();
        Node.IsInstanceIdValid(projectileBId).ShouldBeFalse();
    }

    [Fact]
    public void GivenSamePenetrationProjectiles_WhenProjectilesCollide_ThenBothProjectilesAreFreed()
    {
        var ownerA = new Character();
        var ownerB = new Character();
        _projectileA.OwnerCharacter = ownerA;
        _projectileB.OwnerCharacter = ownerB;

        _projectileA.Penetration = 1;
        _projectileB.Penetration = 1;

        ulong projectileAId = _projectileA.GetInstanceId();
        ulong projectileBId = _projectileB.GetInstanceId();

        // Let the simulation run a short while; projectiles of the same
        // penetration should subtract an equal amount from each other, resulting in both being freed.
        _godot.GodotInstance.Iteration(30);

        Node.IsInstanceIdValid(projectileAId).ShouldBeFalse();
        Node.IsInstanceIdValid(projectileBId).ShouldBeFalse();
    }

    [Fact]
    public void GivenSameOwnerProjectiles_WhenProjectilesCollide_ThenDoNotCancelEachOther()
    {
        _projectileA.Penetration = 1;
        _projectileB.Penetration = 1;

        var owner = new Character();
        _projectileA.OwnerCharacter = owner;
        _projectileB.OwnerCharacter = owner;

        ulong projectileAId = _projectileA.GetInstanceId();
        ulong projectileBId = _projectileB.GetInstanceId();

        _godot.GodotInstance.Iteration(30);

        Node.IsInstanceIdValid(projectileAId).ShouldBeTrue();
        Node.IsInstanceIdValid(projectileBId).ShouldBeTrue();
    }
}
