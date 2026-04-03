using CrossedDimensions.Characters;
using CrossedDimensions.Environment.Triggers;
using CrossedDimensions.States.Characters;
using Godot;

namespace CrossedDimensions.Tests.Integration.Environment;

[Collection("GodotHeadless")]
public sealed class DashCrystalIntegrationTest : System.IDisposable
{
    private const string CharacterScenePath = "res://Characters/Character.tscn";

    private readonly GodotHeadlessFixedFpsFixture _godot;
    private readonly Node _sceneRoot;
    private readonly Character _player;
    private readonly DashCrystal _crystal;

    public DashCrystalIntegrationTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;
        _sceneRoot = new Node { Name = "dash_crystal_test_root" };
        _godot.Tree.Root.AddChild(_sceneRoot);

        _player = ResourceLoader
            .Load<PackedScene>(CharacterScenePath)
            .Instantiate<Character>();
        _sceneRoot.AddChild(_player);

        _crystal = new DashCrystal
        {
            Name = "dash_crystal"
        };
        _sceneRoot.AddChild(_crystal);

        _godot.GodotInstance.Iteration(2);
    }

    public void Dispose()
    {
        Input.ActionRelease("split");
        Input.ActionRelease("move_right");
        _sceneRoot?.QueueFree();
    }

    [Fact]
    public void DashCrystal_WhenOriginalPlayerEnters_ResetsSplitCooldown()
    {
        TriggerSplitAndApplyCooldown(keepClone: false);
        var splitState = GetSplitState(_player);
        splitState.CanSplit.ShouldBeFalse();

        _crystal.EmitSignal(Area2D.SignalName.BodyEntered, _player);
        _godot.GodotInstance.Iteration(1);

        splitState.CanSplit.ShouldBeTrue();
    }

    [Fact]
    public void DashCrystal_WhenCloneEnters_DoesNotResetSplitCooldown()
    {
        TriggerSplitAndApplyCooldown(keepClone: true);
        var splitState = GetSplitState(_player);
        splitState.CanSplit.ShouldBeFalse();
        _player.Cloneable.Clone.ShouldNotBeNull();

        _crystal.EmitSignal(Area2D.SignalName.BodyEntered, _player.Cloneable.Clone);
        _godot.GodotInstance.Iteration(1);

        splitState.CanSplit.ShouldBeFalse();
    }

    [Fact]
    public void DashCrystal_WhenPlayerEntersAgain_ResetsCooldownAgain()
    {
        TriggerSplitAndApplyCooldown(keepClone: false);
        var splitState = GetSplitState(_player);
        splitState.CanSplit.ShouldBeFalse();

        _crystal.EmitSignal(Area2D.SignalName.BodyEntered, _player);
        _godot.GodotInstance.Iteration(1);
        splitState.CanSplit.ShouldBeTrue();

        TriggerSplitAndApplyCooldown(keepClone: false);
        splitState.CanSplit.ShouldBeFalse();

        _crystal.EmitSignal(Area2D.SignalName.BodyEntered, _player);
        _godot.GodotInstance.Iteration(1);

        splitState.CanSplit.ShouldBeTrue();
    }

    private CharacterSplitState GetSplitState(Character character)
    {
        return character
            .MovementStateMachine
            .GetNode<CharacterSplitState>("Split State");
    }

    private void TriggerSplitAndApplyCooldown(bool keepClone)
    {
        _player.Cloneable.SplitMergeWindowDuration = keepClone ? 0 : 0.25;

        Input.ActionPress("move_right");
        Input.ActionPress("split");
        _godot.GodotInstance.Iteration(3);
        Input.ActionRelease("split");
        _godot.GodotInstance.Iteration(3);
        Input.ActionRelease("move_right");
    }
}
