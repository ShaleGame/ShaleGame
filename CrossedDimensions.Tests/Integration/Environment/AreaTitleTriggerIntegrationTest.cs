using CrossedDimensions.Characters;
using CrossedDimensions.Environment;
using CrossedDimensions.Environment.Triggers;
using CrossedDimensions.UI.UIPlayerHud;
using Godot;
using Shouldly;
using Xunit;

namespace CrossedDimensions.Tests.Integration.Environment;

[Collection("GodotHeadless")]
public sealed class AreaTitleTriggerIntegrationTest : System.IDisposable
{
    private const string CharacterScenePath = "res://Characters/Character.tscn";
    private const string AreaTitleCardScenePath = "res://UI/UIPlayerHud/AreaTitleCard.tscn";

    private readonly GodotHeadlessFixedFpsFixture _godot;
    private readonly Node _sceneRoot;
    private readonly AreaManager _areaManager;
    private readonly Character _player;
    private readonly AreaTitleCard _areaTitleCard;

    public AreaTitleTriggerIntegrationTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;
        _sceneRoot = new Node { Name = "area_title_trigger_test_root" };
        _godot.Tree.Root.AddChild(_sceneRoot);

        _areaManager = new AreaManager { Name = "area_manager" };
        _godot.Tree.Root.AddChild(_areaManager);

        _areaTitleCard = ResourceLoader
            .Load<PackedScene>(AreaTitleCardScenePath)
            .Instantiate<AreaTitleCard>();
        _sceneRoot.AddChild(_areaTitleCard);

        _player = ResourceLoader
            .Load<PackedScene>(CharacterScenePath)
            .Instantiate<Character>();
        _sceneRoot.AddChild(_player);

        _godot.GodotInstance.Iteration(2);
    }

    public void Dispose()
    {
        _sceneRoot?.QueueFree();
        _areaManager?.QueueFree();
    }

    [Fact]
    public void AreaTitleTrigger_WhenPlayerEnters_EmitsAreaManagerSignal()
    {
        var areaData = new AreaData
        {
            Title = "Fungal Core",
            Subtitle = "Collapsed Galleries"
        };

        var trigger = CreateTrigger(areaData);

        var signalCount = 0;
        AreaData emittedData = null;
        _areaManager.AreaTriggerEntered += data =>
        {
            signalCount++;
            emittedData = data;
        };

        trigger.EmitSignal(Area2D.SignalName.BodyEntered, _player);
        _godot.GodotInstance.Iteration(1);

        signalCount.ShouldBe(1);
        emittedData.ShouldBeSameAs(areaData);
    }

    [Fact]
    public void AreaTitleTrigger_WhenCloneEnters_DoesNotEmitAreaManagerSignal()
    {
        var areaData = new AreaData
        {
            Title = "Fungal Core",
            Subtitle = "Collapsed Galleries"
        };

        var trigger = CreateTrigger(areaData);
        var clone = CreateClone();

        var signalCount = 0;
        _areaManager.AreaTriggerEntered += _ => signalCount++;

        trigger.EmitSignal(Area2D.SignalName.BodyEntered, clone);
        _godot.GodotInstance.Iteration(1);

        signalCount.ShouldBe(0);
    }

    [Fact]
    public void AreaTitleCard_WhenEnteringSameAreaData_DoesNotRestartAnimation()
    {
        var first = new AreaData
        {
            Title = "Faded Canyon",
            Subtitle = "Dust Corridor"
        };

        var sameByValue = new AreaData
        {
            Title = "Faded Canyon",
            Subtitle = "Dust Corridor"
        };

        var firstTrigger = CreateTrigger(first);
        var secondTrigger = CreateTrigger(sameByValue);

        firstTrigger.EmitSignal(Area2D.SignalName.BodyEntered, _player);
        _godot.GodotInstance.Iteration(1);

        _areaTitleCard.AnimationPlayCount.ShouldBe(1);

        _godot.GodotInstance.Iteration(10);

        secondTrigger.EmitSignal(Area2D.SignalName.BodyEntered, _player);
        _godot.GodotInstance.Iteration(1);

        _areaTitleCard.TitleLabel.Text.ShouldBe("Faded Canyon");
        _areaTitleCard.SubtitleLabel.Text.ShouldBe("Dust Corridor");
        _areaTitleCard.AnimationPlayCount.ShouldBe(1);
    }

    [Fact]
    public void AreaTitleCard_WhenEnteringDifferentAreaData_RestartsAnimationAndUpdatesText()
    {
        var first = new AreaData
        {
            Title = "Stone Reach",
            Subtitle = "Outer Ramparts"
        };

        var second = new AreaData
        {
            Title = "Sunken Vault",
            Subtitle = "Flooded Annex"
        };

        var firstTrigger = CreateTrigger(first);
        var secondTrigger = CreateTrigger(second);

        firstTrigger.EmitSignal(Area2D.SignalName.BodyEntered, _player);
        _godot.GodotInstance.Iteration(1);

        _areaTitleCard.AnimationPlayCount.ShouldBe(1);

        _godot.GodotInstance.Iteration(10);

        secondTrigger.EmitSignal(Area2D.SignalName.BodyEntered, _player);
        _godot.GodotInstance.Iteration(1);

        _areaTitleCard.TitleLabel.Text.ShouldBe("Sunken Vault");
        _areaTitleCard.SubtitleLabel.Text.ShouldBe("Flooded Annex");
        _areaTitleCard.AnimationPlayCount.ShouldBe(2);
    }

    private AreaTitleTrigger CreateTrigger(AreaData areaData)
    {
        var trigger = new AreaTitleTrigger
        {
            AreaData = areaData,
            Name = $"area_title_trigger_{areaData.Title.Replace(' ', '_')}"
        };

        _sceneRoot.AddChild(trigger);
        _godot.GodotInstance.Iteration(1);
        return trigger;
    }

    private Character CreateClone()
    {
        var original = ResourceLoader
            .Load<PackedScene>(CharacterScenePath)
            .Instantiate<Character>();

        _sceneRoot.AddChild(original);
        _godot.GodotInstance.Iteration(1);
        var clone = original.Cloneable.Split();
        _godot.GodotInstance.Iteration(1);
        return clone;
    }
}
