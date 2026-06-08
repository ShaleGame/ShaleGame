using CrossedDimensions.BoundingBoxes;
using CrossedDimensions.Characters;
using Godot;
using System;

namespace CrossedDimensions.Tests.BoundingBoxes;

[Collection("GodotHeadless")]
public sealed class MergePlayerAreaTest : IDisposable
{
    private const string CharacterScenePath = "res://Characters/Character.tscn";

    private readonly GodotHeadlessFixedFpsFixture _godot;
    private readonly Node _sceneRoot;
    private readonly Character _player;
    private readonly MergePlayerArea _mergeArea;

    public MergePlayerAreaTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;
        _sceneRoot = new Node { Name = "merge_player_area_test_root" };
        _godot.Tree.Root.AddChild(_sceneRoot);

        _player = ResourceLoader
            .Load<PackedScene>(CharacterScenePath)
            .Instantiate<Character>();
        _sceneRoot.AddChild(_player);

        _mergeArea = new MergePlayerArea { Name = "merge_player_area" };
        _sceneRoot.AddChild(_mergeArea);

        _godot.GodotInstance.Iteration(2);
    }

    public void Dispose()
    {
        _sceneRoot?.QueueFree();
    }

    [Fact]
    public void MergePlayerArea_WhenOriginalPlayerEnters_MergesClone()
    {
        var clone = _player.Cloneable.Split();
        clone.ShouldNotBeNull();

        _mergeArea.EmitSignal(Area2D.SignalName.BodyEntered, _player);

        _player.Cloneable.Clone.ShouldBeNull();
        clone.IsQueuedForDeletion().ShouldBeTrue();
    }

    [Fact]
    public void MergePlayerArea_WhenClonePlayerEnters_MergesClone()
    {
        var clone = _player.Cloneable.Split();
        clone.ShouldNotBeNull();

        _mergeArea.EmitSignal(Area2D.SignalName.BodyEntered, clone);

        _player.Cloneable.Clone.ShouldBeNull();
        clone.IsQueuedForDeletion().ShouldBeTrue();
    }

    [Fact]
    public void MergePlayerArea_WhenNonPlayerCharacterEnters_DoesNotMerge()
    {
        _player.RemoveFromGroup("Player");
        var clone = _player.Cloneable.Split();
        clone.ShouldNotBeNull();

        _mergeArea.EmitSignal(Area2D.SignalName.BodyEntered, _player);
        _godot.GodotInstance.Iteration(1);

        _player.Cloneable.Clone.ShouldBe(clone);
        clone.IsQueuedForDeletion().ShouldBeFalse();
    }
}
