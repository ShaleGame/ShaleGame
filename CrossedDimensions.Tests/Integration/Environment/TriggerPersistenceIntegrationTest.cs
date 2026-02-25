using System;
using CrossedDimensions.Saves;
using Godot;
using Shouldly;
using Xunit;

using TriggerBase = CrossedDimensions.Environment.Triggers.Trigger;
using ActivatorBase = CrossedDimensions.Environment.Triggers.Activator;

namespace CrossedDimensions.Tests.Integration.Environment;

[Collection("GodotHeadless")]
public sealed class TriggerPersistenceIntegrationTest : IDisposable
{
    private readonly GodotHeadlessFixedFpsFixture _godot;
    private readonly Node _sceneRoot;
    private readonly SaveManager _saveManager;

    public TriggerPersistenceIntegrationTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;
        _sceneRoot = new Node { Name = "trigger_persistence_root" };
        _godot.Tree.Root.AddChild(_sceneRoot);

        _saveManager = new SaveManager { Name = "integration_save_manager" };
        _godot.Tree.Root.AddChild(_saveManager);

        _godot.GodotInstance.Iteration(1);
        _saveManager.CreateNewSave();
    }

    public void Dispose()
    {
        _sceneRoot?.QueueFree();
        _saveManager?.QueueFree();
    }

    [Fact]
    public void StayActiveTrigger_PersistsAndRestoresState()
    {
        const string saveKey = "integration/trigger_persistence";

        var trigger = new TestTrigger
        {
            Name = "persistent_trigger",
            SaveKey = saveKey,
            StayActive = true
        };

        _sceneRoot.AddChild(trigger);
        _godot.GodotInstance.Iteration(1);

        trigger.SetActive(true);
        _godot.GodotInstance.Iteration(1);

        var saved = SaveManager.Instance.TryGetKey<bool>(saveKey, out var stored);
        saved.ShouldBeTrue();
        stored.ShouldBeTrue();

        var restored = new TestTrigger
        {
            Name = "restored_trigger",
            SaveKey = saveKey,
            StayActive = true
        };

        _sceneRoot.AddChild(restored);
        _godot.GodotInstance.Iteration(1);

        restored.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void StayActivatedActivator_PersistsAndRestoresState()
    {
        const string saveKey = "integration/activator_persistence";

        var activator = new TestActivator
        {
            Name = "persistent_activator",
            SaveKey = saveKey,
            StayActivated = true,
            ShouldActivateOverride = true
        };

        _sceneRoot.AddChild(activator);
        _godot.GodotInstance.Iteration(1);

        var saved = SaveManager.Instance.TryGetKey<bool>(saveKey, out var stored);
        saved.ShouldBeTrue();
        stored.ShouldBeTrue();

        var restored = new TestActivator
        {
            Name = "restored_activator",
            SaveKey = saveKey,
            StayActivated = true,
            ShouldActivateOverride = false
        };

        _sceneRoot.AddChild(restored);
        _godot.GodotInstance.Iteration(1);

        restored.IsActivated.ShouldBeTrue();
    }
}

internal sealed partial class TestTrigger : TriggerBase { }

internal sealed partial class TestActivator : ActivatorBase
{
    public bool ShouldActivateOverride { get; set; }

    protected override bool ShouldActivate()
    {
        return ShouldActivateOverride;
    }
}
