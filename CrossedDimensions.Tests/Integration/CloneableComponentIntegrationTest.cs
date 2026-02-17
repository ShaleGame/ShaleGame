using CrossedDimensions.Characters;
using Godot;
using System;

namespace CrossedDimensions.Tests.Integration;

[Collection("GodotHeadless")]
public class CloneableComponentIntegrationTest : IDisposable
{
    private const string ScenePath = "res://Characters/Character.tscn";

    private readonly GodotHeadlessFixedFpsFixture _godot;
    private Node _scene;
    private Character _character;

    public CloneableComponentIntegrationTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;
        _scene = null;

        var packed = ResourceLoader.Load<PackedScene>(ScenePath);
        _scene = new Node2D();
        _character = packed.Instantiate<Character>();
        _scene.AddChild(_character);
        _godot.Tree.Root.AddChild(_scene);
    }

    public void Dispose()
    {
        _scene?.QueueFree();
        _scene = null;
    }

    [Fact]
    public void GivenCharacter_WhenLoaded_ShouldInitializeCorrectly()
    {
        _character.ShouldNotBeNull();
        _character.Cloneable.ShouldNotBeNull();
    }

    [Fact]
    public void CloneableComponent_WhenLoaded_ShouldInitializeCorrectly()
    {
        var cloneable = _character.Cloneable;

        cloneable.ShouldNotBeNull();
        cloneable.Original.ShouldBeNull();
        cloneable.Clone.ShouldBeNull();
        cloneable.SplitForce.ShouldBe(768f);
        cloneable.IsClone.ShouldBeFalse();
        cloneable.Mirror.ShouldBeNull();
    }

    [Fact]
    public void CloneableComponent_Split_WhenNoCloneExists_ShouldCreateClone()
    {
        var originalCloneable = _character.Cloneable;
        var originalXScale = _character.Controller.XScale;

        var clone = originalCloneable.Split();

        clone.ShouldNotBeNull();
        originalCloneable.Clone.ShouldBe(clone);
        clone.Cloneable.Original.ShouldBe(_character);
        clone.Cloneable.Clone.ShouldBeNull();
        clone.Controller.XScale.ShouldBe(originalXScale * -1);
        clone.MovementStateMachine.InitialState.Name.ToString().ShouldBe("Split State");
        clone.GetParent().ShouldBe(_character.GetParent());
        clone.Cloneable.IsClone.ShouldBeTrue();
    }

    [Fact]
    public void CloneableComponent_Split_ShouldSetSplitState()
    {
        var clone = _character.Cloneable.Split();

        var initialState = clone.MovementStateMachine.InitialState;
        initialState.ShouldNotBeNull();
        initialState.Name.ToString().ShouldBe("Split State");
    }

    [Fact]
    public void CloneableComponent_Mirror_WhenOriginal_ShouldReturnItsClone()
    {
        var clone = _character.Cloneable.Split();

        var mirrorFromOriginal = _character.Cloneable.Mirror;
        mirrorFromOriginal.ShouldBe(clone);
    }

    [Fact]
    public void CloneableComponent_Mirror_WhenClone_ShouldReturnItsOriginal()
    {
        var clone = _character.Cloneable.Split();

        var mirrorFromClone = clone.Cloneable.Mirror;
        mirrorFromClone.ShouldBe(_character);
    }

    [Fact]
    public void CloneableComponent_Split_WhenCloneExists_ShouldReturnNull()
    {
        var firstClone = _character.Cloneable.Split();

        var secondClone = _character.Cloneable.Split();

        secondClone.ShouldBeNull();
        _character.Cloneable.Clone.ShouldBe(firstClone);
    }

    [Fact]
    public void CloneableComponent_Merge_WhenCalledOnOriginal_ShouldFreeClone()
    {
        var clone = _character.Cloneable.Split();

        _character.Cloneable.Merge();

        _character.Cloneable.Clone.ShouldBeNull();
        clone.IsQueuedForDeletion().ShouldBeTrue();
    }

    [Fact]
    public void CloneableComponent_Merge_WhenCalledOnClone_ShouldFreeItself()
    {
        var clone = _character.Cloneable.Split();

        clone.Cloneable.Merge();

        _character.Cloneable.Clone.ShouldBeNull();
        clone.IsQueuedForDeletion().ShouldBeTrue();
    }

    [Fact]
    public void CloneableComponent_Merge_WhenNoMirrorExists_ShouldSkipMerge()
    {
        var originalInstanceId = _character.GetInstanceId();

        _character.Cloneable.Merge();

        _character.ShouldNotBeNull();
    }

    [Fact]
    public void CloneableComponent_SplitAndMerge_ShouldWorkCorrectly()
    {
        var originalCloneable = _character.Cloneable;

        var clone = originalCloneable.Split();

        originalCloneable.Clone.ShouldBe(clone);
        clone.Cloneable.Original.ShouldBe(_character);

        originalCloneable.Merge();

        originalCloneable.Clone.ShouldBeNull();
        clone.IsQueuedForDeletion().ShouldBeTrue();
    }

    [Fact]
    public void CloneableComponent_Properties_ShouldMaintainConsistentState()
    {
        var clone = _character.Cloneable.Split();

        _character.Cloneable.IsClone.ShouldBeFalse();
        _character.Cloneable.Mirror.ShouldBe(clone);
        clone.Cloneable.IsClone.ShouldBeTrue();
        clone.Cloneable.Mirror.ShouldBe(_character);
        _character.Cloneable.SplitForce.ShouldBe(clone.Cloneable.SplitForce);
    }

    [Fact]
    public void GivenOriginal_WhenSplitting_ShouldSplitCurrentHealth()
    {
        var originalHealth = _character.Health.CurrentHealth;

        var clone = _character.Cloneable.Split();

        var expectedHealth = (int)Math.Ceiling(originalHealth / 2.0);
        _character.Health.CurrentHealth.ShouldBe(expectedHealth);
    }

    [Fact]
    public void GivenOriginal_WhenSplitting_ShouldSplitMaxHealth()
    {
        var originalMaxHealth = _character.Health.MaxHealth;

        var clone = _character.Cloneable.Split();

        var expectedMaxHealth = (int)Math.Ceiling(originalMaxHealth / 2.0);
        _character.Health.MaxHealth.ShouldBe(expectedMaxHealth);
    }

    [Fact]
    public void GivenOriginal_WhenMerging_ShouldCombineHealth()
    {
        var originalHealth = _character.Health.CurrentHealth;

        var clone = _character.Cloneable.Split();

        clone.Health.CurrentHealth -= 10;

        _character.Cloneable.Merge();

        var expectedHealth = originalHealth - 10;
        _character.Health.CurrentHealth.ShouldBe(expectedHealth);
    }

    [Fact]
    public void GivenOriginal_WhenMerging_ShouldCombineMaxHealth()
    {
        var originalMaxHealth = _character.Health.MaxHealth;

        var clone = _character.Cloneable.Split();

        _character.Cloneable.Merge();

        var expectedMaxHealth = originalMaxHealth;
        _character.Health.MaxHealth.ShouldBe(expectedMaxHealth);
    }

    [Fact]
    public void GivenCloneable_WhenSplitReleasedInWindow_ShouldMerge()
    {
        _character.Cloneable.SplitMergeWindowDuration = 0.25;

        Godot.Input.ActionPress("move_right");
        Godot.Input.ActionPress("split");
        _godot.GodotInstance.Iteration();
        Godot.Input.ActionRelease("split");
        _godot.GodotInstance.Iteration();

        _character.Cloneable.Clone.ShouldBeNull();
    }

    [Fact]
    public void GivenCLoneable_WhenSplitReleasedAfterWindow_ShouldNotMerge()
    {
        _character.Cloneable.SplitMergeWindowDuration = 0;

        Godot.Input.ActionPress("move_right");
        Godot.Input.ActionPress("split");
        _godot.GodotInstance.Iteration();
        Godot.Input.ActionRelease("split");
        _godot.GodotInstance.Iteration();

        _character.Cloneable.Clone.ShouldNotBeNull();
    }
}
