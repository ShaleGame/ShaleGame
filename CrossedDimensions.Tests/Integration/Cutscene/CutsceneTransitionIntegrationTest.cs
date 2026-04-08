using System;
using System.Collections.Generic;
using System.Linq;
using CrossedDimensions.Characters;
using CrossedDimensions.Components;
using CrossedDimensions.Environment.Cutscene;
using CrossedDimensions.Environment.Triggers;
using CrossedDimensions.Items;
using CrossedDimensions.Saves;
using CrossedDimensions.UI;
using Godot;
using Shouldly;
using Xunit;

namespace CrossedDimensions.Tests.Integration.Cutscene;

[Collection("GodotHeadless")]
public sealed class CutsceneTransitionIntegrationTest : IDisposable
{
    private const string GameplayScenePath =
        $"{Paths.TestPath}/Integration/Cutscene/" +
        "CutsceneTransitionGameplayScene.tscn";
    private const string CutsceneScenePath =
        $"{Paths.TestPath}/Integration/Cutscene/" +
        "CutsceneTransitionCutsceneScene.tscn";

    private readonly GodotHeadlessFixedFpsFixture _godot;
    private readonly SaveManager _saveManager;
    private readonly SceneManager _sceneManager;
    private readonly ScreenOverlayManager _screenOverlay;
    private readonly Node _originalScene;
    private readonly SaveFile _originalSave;

    private readonly Node _gameplayScene;
    private readonly Character _player;
    private readonly Node2D _statefulNode;

    public CutsceneTransitionIntegrationTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;
        _saveManager = _godot.Tree.Root.GetNode<SaveManager>("/root/SaveManager");
        _sceneManager = _godot.Tree.Root.GetNode<SceneManager>("/root/SceneManager");
        _screenOverlay = _godot.Tree.Root.GetNode<ScreenOverlayManager>(
            "/root/ScreenOverlayManager");
        _originalScene = _godot.Tree.CurrentScene;
        _originalSave = _saveManager.CurrentSave;
        _saveManager.CurrentSave = new SaveFile();

        _gameplayScene = ResourceLoader
            .Load<PackedScene>(GameplayScenePath)
            .Instantiate<Node>();
        _godot.Tree.Root.AddChild(_gameplayScene);
        _godot.Tree.CurrentScene = _gameplayScene;
        _godot.GodotInstance.Iteration(2);
        _screenOverlay.FadeAnimationPlayer.Stop();
        _screenOverlay.FadeOverlay.Visible = false;
        _screenOverlay.FadeOverlay.Modulate = new Color(1, 1, 1, 0);

        _player = _gameplayScene.GetNode<Character>("Player");
        _statefulNode = _gameplayScene.GetNode<Node2D>("StatefulNode");
    }

    public void Dispose()
    {
        Input.ActionRelease("move_right");
        Input.ActionRelease("move_left");
        Input.ActionRelease("move_up");
        Input.ActionRelease("move_down");

        if (_sceneManager.ActiveCutsceneScene is not null
            && GodotObject.IsInstanceValid(_sceneManager.ActiveCutsceneScene)
            && _sceneManager.ActiveCutsceneScene.GetParent() is not null)
        {
            _sceneManager.ActiveCutsceneScene
                .GetParent()
                .RemoveChild(_sceneManager.ActiveCutsceneScene);
            _sceneManager.ActiveCutsceneScene.QueueFree();
        }

        if (_gameplayScene is not null
            && GodotObject.IsInstanceValid(_gameplayScene)
            && _gameplayScene.GetParent() is not null)
        {
            _gameplayScene.GetParent().RemoveChild(_gameplayScene);
            _gameplayScene.QueueFree();
        }

        if (_originalScene is not null && GodotObject.IsInstanceValid(_originalScene))
        {
            _godot.Tree.CurrentScene = _originalScene;
        }

        if (_saveManager is not null && GodotObject.IsInstanceValid(_saveManager))
        {
            _saveManager.CurrentSave = _originalSave;
        }
    }

    [Fact]
    public void CutsceneTrigger_TransitionsToSeparateSceneAfterFade_AndSuspendsGameplayState()
    {
        var events = new List<string>();
        ScreenOverlayManager.FadeInStartedEventHandler fadeInStarted =
            () => events.Add("fade_in_started");
        ScreenOverlayManager.FadeInCompletedEventHandler fadeInCompleted =
            () => events.Add("fade_in_completed");
        SceneManager.CutsceneLoadedEventHandler cutsceneLoaded = _ => events.Add("cutscene_loaded");

        _screenOverlay.FadeInStarted += fadeInStarted;
        _screenOverlay.FadeInCompleted += fadeInCompleted;
        _sceneManager.CutsceneLoaded += cutsceneLoaded;

        try
        {
            _statefulNode.SetMeta("persisted", "still_here");
            var trigger = CreateTrigger();
            var originalStatefulNodeId = _statefulNode.GetInstanceId();

            TriggerCutscene(trigger);
            var cutscene = WaitForCutsceneLoaded();

            _sceneManager.SuspendedScene.ShouldNotBeNull();
            _sceneManager.PreviousScene.ShouldBe(GameplayScenePath);
            IsCutsceneSceneActive().ShouldBeTrue();
            _gameplayScene.GetParent().ShouldBeNull();
            _sceneManager.ActiveCutsceneScene.GetInstanceId().ShouldBe(cutscene.GetInstanceId());
            _sceneManager.SuspendedScene
                .GetNode<Node2D>("StatefulNode")
                .GetInstanceId()
                .ShouldBe(originalStatefulNodeId);
            _statefulNode.GetMeta("persisted").As<string>().ShouldBe("still_here");
            GodotObject.IsInstanceValid(_statefulNode).ShouldBeTrue();

            var fadeInStartedIndex = events.IndexOf("fade_in_started");
            var fadeInCompletedIndex = events.IndexOf("fade_in_completed");
            var cutsceneLoadedIndex = events.IndexOf("cutscene_loaded");

            fadeInStartedIndex.ShouldBeGreaterThanOrEqualTo(0);
            fadeInCompletedIndex.ShouldBeGreaterThan(fadeInStartedIndex);
            cutsceneLoadedIndex.ShouldBeGreaterThan(fadeInCompletedIndex);
        }
        finally
        {
            _screenOverlay.FadeInStarted -= fadeInStarted;
            _screenOverlay.FadeInCompleted -= fadeInCompleted;
            _sceneManager.CutsceneLoaded -= cutsceneLoaded;
        }
    }

    [Fact]
    public void Cutscene_SuppressesInput_AndEndsWhenAnimationFinishes()
    {
        var trigger = CreateTrigger();

        TriggerCutscene(trigger);
        var cutscene = WaitForCutsceneLoaded();
        WaitForCutsceneStart(cutscene);

        var cutscenePlayer = cutscene.GetNode<Character>("CutscenePlayer");
        var suspendedPlayer = _sceneManager.SuspendedScene.GetNode<Character>("Player");
        cutscenePlayer.Controller.ShouldBeNull();

        var suspendedPlayerPosition = suspendedPlayer.GlobalPosition;
        var cutscenePlayerPosition = cutscenePlayer.GlobalPosition;
        Input.ActionPress("move_right");

        _godot.GodotInstance.Iteration(10);

        IsCutsceneSceneActive().ShouldBeTrue();
        suspendedPlayer.GlobalPosition.ShouldBe(suspendedPlayerPosition);
        cutscenePlayer.GlobalPosition.ShouldBe(cutscenePlayerPosition);

        _godot.GodotInstance
            .IterateUntil(
                () => IsGameplaySceneRestored() && _sceneManager.ActiveCutsceneScene is null,
                240)
            .ShouldBeTrue();
    }

    [Fact]
    public void CutsceneCompletion_RestoresGameplayState_RepositionsPlayer_AndFades()
    {
        var events = new List<string>();
        Vector2? resumedPlayerPosition = null;
        ScreenOverlayManager.FadeInStartedEventHandler fadeInStarted =
            () => events.Add("fade_in_started");
        ScreenOverlayManager.FadeInCompletedEventHandler fadeInCompleted =
            () => events.Add("fade_in_completed");
        ScreenOverlayManager.FadeOutStartedEventHandler fadeOutStarted =
            () => events.Add("fade_out_started");
        ScreenOverlayManager.FadeOutCompletedEventHandler fadeOutCompleted =
            () => events.Add("fade_out_completed");
        SceneManager.GameplayResumedEventHandler gameplayResumed = _ =>
        {
            events.Add("gameplay_resumed");
            resumedPlayerPosition = _player.GlobalPosition;
        };

        _screenOverlay.FadeInStarted += fadeInStarted;
        _screenOverlay.FadeInCompleted += fadeInCompleted;
        _screenOverlay.FadeOutStarted += fadeOutStarted;
        _screenOverlay.FadeOutCompleted += fadeOutCompleted;
        _sceneManager.GameplayResumed += gameplayResumed;

        try
        {
            var returnPosition = new Vector2(144, 48);
            var liveGameplayScene = _godot.Tree.CurrentScene;
            var liveGameplayPlayer = liveGameplayScene.GetNode<Character>("Player");
            var liveStatefulNode = liveGameplayScene.GetNode<Node2D>("StatefulNode");
            _statefulNode.SetMeta("counter", 7);
            var trigger = CreateTrigger(returnPlayerPosition: returnPosition);

            TriggerCutscene(trigger);
            WaitForCutsceneLoaded();

            _godot.GodotInstance
                .IterateUntil(
                    () => IsGameplaySceneRestored()
                        && _sceneManager.ActiveCutsceneScene is null
                        && !_screenOverlay.FadeOverlay.Visible,
                    360)
                .ShouldBeTrue();

            _gameplayScene.GetParent().ShouldNotBeNull();
            _gameplayScene.GetInstanceId().ShouldBe(liveGameplayScene.GetInstanceId());
            _player.GetInstanceId().ShouldBe(liveGameplayPlayer.GetInstanceId());
            _statefulNode.GetInstanceId().ShouldBe(liveStatefulNode.GetInstanceId());
            resumedPlayerPosition.ShouldNotBeNull();
            resumedPlayerPosition.Value.ShouldBe(returnPosition);
            _statefulNode.GetMeta("counter").As<int>().ShouldBe(7);
            _sceneManager.SuspendedScene.ShouldBeNull();
            _sceneManager.ActiveCutsceneScene.ShouldBeNull();
            _screenOverlay.FadeOverlay.Visible.ShouldBeFalse();

            events.Count(e => e == "fade_in_started").ShouldBeGreaterThanOrEqualTo(2);
            events.Count(e => e == "fade_in_completed").ShouldBeGreaterThanOrEqualTo(2);
            events.Count(e => e == "fade_out_started").ShouldBeGreaterThanOrEqualTo(2);
            events.Count(e => e == "fade_out_completed").ShouldBeGreaterThanOrEqualTo(2);

            var firstFadeInStarted = events.IndexOf("fade_in_started");
            var gameplayResumedIndex = events.IndexOf("gameplay_resumed");
            var fadeInCompletedBeforeResume = events
                .Take(gameplayResumedIndex)
                .Any(e => e == "fade_in_completed");
            var fadeOutStartedAfterResume = events
                .Skip(gameplayResumedIndex + 1)
                .Any(e => e == "fade_out_started");
            var fadeOutCompletedAfterResume = events
                .Skip(gameplayResumedIndex + 1)
                .Any(e => e == "fade_out_completed");

            firstFadeInStarted.ShouldBeGreaterThanOrEqualTo(0);
            gameplayResumedIndex.ShouldBeGreaterThan(firstFadeInStarted);
            fadeInCompletedBeforeResume.ShouldBeTrue();
            fadeOutStartedAfterResume.ShouldBeTrue();
            fadeOutCompletedAfterResume.ShouldBeTrue();
        }
        finally
        {
            _screenOverlay.FadeInStarted -= fadeInStarted;
            _screenOverlay.FadeInCompleted -= fadeInCompleted;
            _screenOverlay.FadeOutStarted -= fadeOutStarted;
            _screenOverlay.FadeOutCompleted -= fadeOutCompleted;
            _sceneManager.GameplayResumed -= gameplayResumed;
        }
    }

    [Fact]
    public void CutsceneCompletion_DoesNotReemitWeaponAddedForExistingInventoryWeapons()
    {
        var inventory = _player.Inventory;
        var weapon = new Weapon();
        int addedCount = 0;
        InventoryComponent.WeaponAddedEventHandler onWeaponAdded =
            (_, _) => addedCount++;

        inventory.WeaponAdded += onWeaponAdded;

        try
        {
            inventory.AddChild(weapon);
            _godot.GodotInstance.Iteration(2);
            addedCount.ShouldBe(1);
            addedCount = 0;

            var trigger = CreateTrigger();
            TriggerCutscene(trigger);
            WaitForCutsceneLoaded();

            _godot.GodotInstance
                .IterateUntil(
                    () => IsGameplaySceneRestored()
                        && _sceneManager.ActiveCutsceneScene is null,
                    360)
                .ShouldBeTrue();

            addedCount.ShouldBe(0);
            inventory.GetChildren().OfType<Weapon>().ShouldContain(weapon);
        }
        finally
        {
            inventory.WeaponAdded -= onWeaponAdded;
        }
    }

    [Fact]
    public void CutsceneTrigger_DoesNotRetriggerUntilPlayerExitsArea()
    {
        var trigger = CreateTrigger();

        TriggerCutscene(trigger);
        WaitForCutsceneLoaded();

        _godot.GodotInstance
            .IterateUntil(
                () => IsGameplaySceneRestored()
                    && _sceneManager.ActiveCutsceneScene is null,
                360)
            .ShouldBeTrue();

        TriggerCutscene(trigger);
        _godot.GodotInstance.Iteration(10);
        _sceneManager.ActiveCutsceneScene.ShouldBeNull();

        trigger.EmitSignal(Area2D.SignalName.BodyExited, _player);
        TriggerCutscene(trigger);

        WaitForCutsceneLoaded().ShouldNotBeNull();
        _godot.GodotInstance
            .IterateUntil(
                () => IsGameplaySceneRestored()
                    && _sceneManager.ActiveCutsceneScene is null,
                360)
            .ShouldBeTrue();
    }

    [Fact]
    public void CutsceneTrigger_CanDisableAfterPlaying()
    {
        var trigger = CreateTrigger(disableAfterPlaying: true);

        TriggerCutscene(trigger);
        WaitForCutsceneLoaded();

        _godot.GodotInstance
            .IterateUntil(
                () => IsGameplaySceneRestored()
                    && _sceneManager.ActiveCutsceneScene is null,
                360)
            .ShouldBeTrue();

        _godot.GodotInstance
            .IterateUntil(
                () => !trigger.Monitoring && !trigger.Monitorable,
                60)
            .ShouldBeTrue();

        TriggerCutscene(trigger);
        _godot.GodotInstance.Iteration(10);
        _sceneManager.ActiveCutsceneScene.ShouldBeNull();
    }

    [Fact]
    public void CutsceneTrigger_CanDisableImmediately_AndPersistConsumedState()
    {
        const string saveKey = "integration/cutscene_trigger_consumed";
        var trigger = CreateTrigger(
            disableAfterPlaying: true,
            disableImmediatelyOnTrigger: true,
            saveKey: saveKey);

        TriggerCutscene(trigger);

        trigger.Monitoring.ShouldBeFalse();
        trigger.Monitorable.ShouldBeFalse();
        _saveManager.TryGetKey<bool>(saveKey, out var isConsumed).ShouldBeTrue();
        isConsumed.ShouldBeTrue();

        WaitForCutsceneLoaded();
        _godot.GodotInstance
            .IterateUntil(
                () => IsGameplaySceneRestored()
                    && _sceneManager.ActiveCutsceneScene is null,
                360)
            .ShouldBeTrue();

        var restoredTrigger = CreateTrigger(
            disableAfterPlaying: true,
            disableImmediatelyOnTrigger: true,
            saveKey: saveKey);

        restoredTrigger.Monitoring.ShouldBeFalse();
        restoredTrigger.Monitorable.ShouldBeFalse();

        TriggerCutscene(restoredTrigger);
        _godot.GodotInstance.Iteration(10);
        _sceneManager.ActiveCutsceneScene.ShouldBeNull();
    }

    [Fact]
    public void CutsceneTrigger_CanDestroyAfterPlaying()
    {
        var trigger = CreateTrigger(destroyAfterPlaying: true);
        var triggerName = trigger.Name.ToString();

        TriggerCutscene(trigger);
        WaitForCutsceneLoaded();

        _godot.GodotInstance
            .IterateUntil(
                () => IsGameplaySceneRestored()
                    && _sceneManager.ActiveCutsceneScene is null,
                360)
            .ShouldBeTrue();

        _godot.GodotInstance
            .IterateUntil(
                () => _gameplayScene.GetNodeOrNull<CutsceneTrigger>(triggerName) is null,
                240)
            .ShouldBeTrue();
    }

    private CutsceneTrigger CreateTrigger(
        Vector2? returnPlayerPosition = null,
        bool disableAfterPlaying = false,
        bool destroyAfterPlaying = false,
        bool disableImmediatelyOnTrigger = false,
        string saveKey = "")
    {
        var metadata = new CutsceneMetadata
        {
            CutsceneScenePath = CutsceneScenePath,
            RepositionPlayerOnReturn = returnPlayerPosition.HasValue,
            ReturnPlayerPosition = returnPlayerPosition ?? Vector2.Zero
        };

        var trigger = new CutsceneTrigger
        {
            Name = $"cutscene_trigger_{Guid.NewGuid():N}",
            Cutscene = metadata,
            SaveKey = saveKey,
            DisableAfterPlaying = disableAfterPlaying,
            DestroyAfterPlaying = destroyAfterPlaying,
            DisableImmediatelyOnTrigger = disableImmediatelyOnTrigger
        };

        _gameplayScene.AddChild(trigger);
        _godot.GodotInstance.Iteration(1);
        return trigger;
    }

    private void TriggerCutscene(CutsceneTrigger trigger)
    {
        trigger.EmitSignal(Area2D.SignalName.BodyEntered, _player);
    }

    private CutsceneScene WaitForCutsceneLoaded()
    {
        _godot.GodotInstance
            .IterateUntil(() => _sceneManager.ActiveCutsceneScene is CutsceneScene, 240)
            .ShouldBeTrue();

        return (CutsceneScene)_sceneManager.ActiveCutsceneScene;
    }

    private void WaitForCutsceneStart(CutsceneScene cutscene)
    {
        _godot.GodotInstance
            .IterateUntil(() => cutscene.IsStarted, 120)
            .ShouldBeTrue();
    }

    private bool IsGameplaySceneRestored()
    {
        return GodotObject.IsInstanceValid(_gameplayScene)
            && _gameplayScene.GetParent() is not null;
    }

    private bool IsCutsceneSceneActive()
    {
        return _sceneManager.ActiveCutsceneScene is not null
            && GodotObject.IsInstanceValid(_sceneManager.ActiveCutsceneScene)
            && _sceneManager.ActiveCutsceneScene.GetParent() is not null;
    }

}
