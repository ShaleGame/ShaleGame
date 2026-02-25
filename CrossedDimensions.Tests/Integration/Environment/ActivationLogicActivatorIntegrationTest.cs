using System;
using System.Collections.Generic;
using CrossedDimensions.Environment.Triggers;
using Godot;
using Shouldly;
using Xunit;

namespace CrossedDimensions.Tests.Integration.Environment;

[Collection("GodotHeadless")]
public sealed class ActivationLogicActivatorIntegrationTest : IDisposable
{
    private readonly GodotHeadlessFixedFpsFixture _godot;
    private readonly Node _sceneRoot;

    public ActivationLogicActivatorIntegrationTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;
        _sceneRoot = new Node { Name = "activation_logic_test_root" };
        _godot.Tree.Root.AddChild(_sceneRoot);
        _godot.GodotInstance.Iteration(1);
    }

    public void Dispose()
    {
        _sceneRoot?.QueueFree();
    }

    [Fact]
    public void RequireAllOnlyActivatesWhenAllTriggersActive()
    {
        var (activator, triggers) = CreateLogicActivator(ActivationLogic.RequireAll, true, true);

        activator.IsActivated.ShouldBeTrue();

        triggers[1].SetActive(false);
        _godot.GodotInstance.Iteration(1);
        activator.IsActivated.ShouldBeFalse();

        triggers[1].SetActive(true);
        _godot.GodotInstance.Iteration(1);
        activator.IsActivated.ShouldBeTrue();
    }

    [Fact]
    public void RequireAnyActivatesWhenAnyTriggerActive()
    {
        var (activator, triggers) = CreateLogicActivator(ActivationLogic.RequireAny, true, false);

        activator.IsActivated.ShouldBeTrue();

        triggers[0].SetActive(false);
        _godot.GodotInstance.Iteration(1);
        activator.IsActivated.ShouldBeFalse();

        triggers[1].SetActive(true);
        _godot.GodotInstance.Iteration(1);
        activator.IsActivated.ShouldBeTrue();

        triggers[1].SetActive(false);
        _godot.GodotInstance.Iteration(1);
        activator.IsActivated.ShouldBeFalse();
    }

    [Fact]
    public void RequireOneOnlyActivatesWithSingleTrigger()
    {
        var (activator, triggers) = CreateLogicActivator(ActivationLogic.RequireOne, true, false);

        activator.IsActivated.ShouldBeTrue();

        triggers[1].SetActive(true);
        _godot.GodotInstance.Iteration(1);
        activator.IsActivated.ShouldBeFalse();

        triggers[0].SetActive(false);
        _godot.GodotInstance.Iteration(1);
        activator.IsActivated.ShouldBeTrue();

        triggers[1].SetActive(false);
        _godot.GodotInstance.Iteration(1);
        activator.IsActivated.ShouldBeFalse();
    }

    [Fact]
    public void RequireNoneActivatesWhenNoTriggersAreActive()
    {
        var (activator, triggers) = CreateLogicActivator(ActivationLogic.RequireNone, false, false);

        activator.IsActivated.ShouldBeTrue();

        triggers[0].SetActive(true);
        _godot.GodotInstance.Iteration(1);
        activator.IsActivated.ShouldBeFalse();

        triggers[0].SetActive(false);
        _godot.GodotInstance.Iteration(1);
        activator.IsActivated.ShouldBeTrue();
    }

    private (
        ActivationLogicActivator activator,
        IReadOnlyList<LogicTrigger> triggers
    ) CreateLogicActivator(
        ActivationLogic logic,
        params bool[] initialStates
    )
    {
        var triggers = new List<LogicTrigger>();

        for (var i = 0; i < initialStates.Length; i++)
        {
            var trigger = new LogicTrigger
            {
                Name = $"logic_trigger_{logic}_{i}"
            };

            if (initialStates[i])
            {
                trigger.SetActive(true);
            }

            _sceneRoot.AddChild(trigger);
            triggers.Add(trigger);
        }

        var activator = new ActivationLogicActivator
        {
            Name = $"logic_activator_{logic}",
            Logic = logic,
            Triggers = new Godot.Collections.Array<Trigger>(triggers.ToArray())
        };

        _sceneRoot.AddChild(activator);
        _godot.GodotInstance.Iteration(1);

        return (activator, triggers);
    }
}

internal sealed partial class LogicTrigger : Trigger { }
