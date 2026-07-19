using CrossedDimensions.Characters;
using CrossedDimensions.States;
using CrossedDimensions.States.Enemies;
using Godot;
using System;
using Xunit;
using Shouldly;

namespace CrossedDimensions.Tests.Integration.Enemies;

[Collection("GodotHeadless")]
public class BatIntegrationTest : IDisposable
{
    private const string ScenePath =
        $"{Paths.TestPath}/Integration/Enemies/BatIntegrationTest.tscn";

    private static readonly Vector2 FarAway = new(10000f, 10000f);

    private readonly GodotHeadlessFixedFpsFixture _godot;
    private Node _scene;
    private readonly Character _bat;
    private readonly StateMachine _movementSM;
    private readonly State _hangingState;
    private readonly State _frozenState;
    private readonly BatSwooping _swoopingState;
    private readonly StaticBody2D _obstruction;

    public BatIntegrationTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;

        var packed = ResourceLoader.Load<PackedScene>(ScenePath);
        _scene = packed.Instantiate() as Node;
        _godot.Tree.Root.AddChild(_scene);

        _bat = _scene.GetNode<Character>("Bat");
        _movementSM = _bat.GetNode<StateMachine>("MovementStateMachine");
        _hangingState = _movementSM.GetNode<State>("Hanging");
        _frozenState = _movementSM.GetNode<State>("Frozen");
        _swoopingState = _movementSM.GetNode<BatSwooping>("Swooping");
        _obstruction = _scene.GetNode<StaticBody2D>("Obstruction");
    }

    public void Dispose()
    {
        _scene?.QueueFree();
        _scene = null;
    }

    [Fact]
    public void GivenScene_WhenLoaded_ShouldInitializeCorrectly()
    {
        _bat.ShouldNotBeNull();
        _bat.Freezable.ShouldNotBeNull();
        _frozenState.ShouldNotBeNull();
        _movementSM.CurrentState.ShouldBe(_hangingState);
        _bat.GetCollisionLayerValue(5).ShouldBeFalse();
    }

    [Fact]
    public void GivenSwoopingBat_WhenFrozen_ThenEntersFrozenAndStopsMoving()
    {
        _obstruction.GlobalPosition = FarAway;

        Vector2 start = _bat.GlobalPosition;
        _swoopingState.SwoopTarget = start + new Vector2(200f, 200f);
        _movementSM.ChangeState("Swooping");

        _godot.GodotInstance
            .IterateUntil(() => _bat.GlobalPosition.DistanceTo(start) > 5f, 300)
            .ShouldBeTrue();

        _bat.Freezable.Freeze(60f);

        _godot.GodotInstance
            .IterateUntil(() => _movementSM.CurrentState == _frozenState, 300)
            .ShouldBeTrue();

        Vector2 frozenAt = _bat.GlobalPosition;
        _godot.GodotInstance.Iteration(30);

        _bat.GlobalPosition.DistanceTo(frozenAt).ShouldBeLessThan(0.01f);
        _bat.Velocity.ShouldBe(Vector2.Zero);
    }

    [Fact]
    public void GivenOverlappingBody_WhenFrozen_ThenCollisionDeferredUntilClear()
    {
        // let the physics server register the overlapping obstruction
        _godot.GodotInstance.Iteration(5);

        _bat.Freezable.Freeze(60f);
        _godot.GodotInstance.Iteration(10);

        _bat.GetCollisionLayerValue(5).ShouldBeFalse();

        _obstruction.GlobalPosition = FarAway;

        _godot.GodotInstance
            .IterateUntil(() => _bat.GetCollisionLayerValue(5), 300)
            .ShouldBeTrue();
    }

    [Fact]
    public void GivenFrozenBat_WhenUnfrozen_ThenReturnsToHangingWithoutCollision()
    {
        _obstruction.GlobalPosition = FarAway;
        _godot.GodotInstance.Iteration(5);

        _bat.Freezable.Freeze(60f);

        _godot.GodotInstance
            .IterateUntil(() => _bat.GetCollisionLayerValue(5), 300)
            .ShouldBeTrue();

        _bat.Freezable.Unfreeze();

        _godot.GodotInstance
            .IterateUntil(() => _movementSM.CurrentState == _hangingState, 300)
            .ShouldBeTrue();

        _bat.GetCollisionLayerValue(5).ShouldBeFalse();
    }
}
