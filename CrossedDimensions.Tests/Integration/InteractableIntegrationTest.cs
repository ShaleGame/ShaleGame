using CrossedDimensions.Environment.Cutscene.Interactables;
using Godot;

namespace CrossedDimensions.Tests.Integration;

[Collection("GodotHeadless")]
public class InteractableIntegrationTest : System.IDisposable
{
    private readonly GodotHeadlessFixedFpsFixture _godot;

    private Node _scene;
    private Interactable _interactable;
    private Characters.Character _character;

    public const string ScenePath = $"{Paths.TestPath}/Integration/InteractableIntegrationTest.tscn";

    //constructor
    public InteractableIntegrationTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;

        var packed = ResourceLoader.Load<PackedScene>(ScenePath);
        _scene = packed.Instantiate<Node>();
        _godot.Tree.Root.AddChild(_scene);

        _interactable = _scene.GetNode<Interactable>("Interactable");
        _character = _scene.GetNode<Characters.Character>("Character");
    }

    //destructor
    public void Dispose()
    {
        _scene?.QueueFree();
        _scene = null;
    }

    //helpers
    private static InputEventAction CreateActionEvent(StringName action, bool pressed = true)
    {
        if (!InputMap.HasAction(action))
        {
            InputMap.AddAction(action);
        }

        return new InputEventAction
        {
            Action = action,
            Pressed = pressed
        };
    }

    //tests
    [Fact]
    public void GivenScene_WhenLoaded_ShouldInitializeCorrectly()
    {
        _interactable.ShouldNotBeNull();
        _character.ShouldNotBeNull();
    }

    [Fact]
    public void GivenInteractable_WhenPlayerEnters_ThenInteractionIsAllowed()
    {
        _character.GlobalPosition = _interactable.GlobalPosition;
        _godot.GodotInstance.Iteration(2);
        _interactable.InteractAllowed.ShouldBeTrue();
    }

    [Fact]
    public void GivenInteractable_WhenPlayerExits_ThenInteractionIsNotAllowed()
    {
        _character.QueueFree();
        _godot.GodotInstance.Iteration(2);
        _interactable.InteractAllowed.ShouldBeFalse();
    }

    [Fact]
    public void GivenInteractable_WhenNotAllowed_ThenInteractedEventDoesNotFire()
    {
        var interactable = new Interactable();
        var action = CreateActionEvent(interactable.InteractAction);

        bool fired = false;
        interactable.Interacted += () => fired = true;

        _godot.GodotInstance.Iteration(2);

        Input.ActionPress(interactable.InteractAction);
        _interactable._Process(interactable.HoldSecs + 0.1f);
        Input.ActionRelease(interactable.InteractAction);

        fired.ShouldBeFalse();
    }

    [Fact]
    public void GivenInteractable_WhenPlayerReleasesEarly_ThenHoldTimerResets()
    {
        var pressed = CreateActionEvent(_interactable.InteractAction, true);
        var released = CreateActionEvent(_interactable.InteractAction, false);

        bool fired = false;
        _interactable.Interacted += () =>
        {
            GD.Print("Interacted event fired!");
            fired = true;
        };

        _godot.GodotInstance.Iteration(2);

        Input.ActionPress(_interactable.InteractAction);
        _interactable._Process(0.3);

        Input.ActionRelease(_interactable.InteractAction);
        _interactable._Process(0.1);

        Input.ActionPress(_interactable.InteractAction);
        _interactable._Process(0.3);

        fired.ShouldBeFalse();
    }

    [Fact]
    public void GivenInteractable_WhenPlayerEntersAndHolds_ThenInteractedEventFires()
    {
        // we have to configure the interactable to have minimal holding time
        // to make this test run in a reasonable time frame
        _interactable.HoldSecs = 0.001f;

        var action = CreateActionEvent(_interactable.InteractAction);

        bool fired = false;
        _interactable.Interacted += () => fired = true;

        Input.ActionPress(_interactable.InteractAction);

        // run through enough iterations for the hold timer to reach the target
        // IterateUntil expects a predicate; wrap the flag in a lambda
        _godot.GodotInstance.IterateUntil(() => fired == true);

        Input.ActionRelease(_interactable.InteractAction);

        fired.ShouldBeTrue();
    }

    [Fact]
    public void GivenInteractable_WhenAllowed_ThenInteractKeyUIDisplays()
    {
        bool fired = false;
        _interactable.InteractAvailable += () =>
        {
            GD.Print("Interact Available event fired!");
            fired = true;
        };

        _interactable.InteractAllowed = true;

        _godot.GodotInstance.Iteration(1);

        fired.ShouldBeTrue();
    }

    [Fact]
    public void GivenInteractable_WhenNotAllowed_ThenInteractKeyUIDoesNotDisplay()
    {
        _character.QueueFree();
        _interactable.InteractAllowed = false;

        bool fired = false;
        _interactable.InteractAvailable += () =>
        {
            GD.Print("Interact Available event fired!");
            fired = true;
        };

        _godot.GodotInstance.Iteration(1);

        fired.ShouldBeFalse();
    }

    [Fact]
    public void GivenInteractable_WhenNotAllowed_ThenHoldTimerZero()
    {
        //make the required hold timer small, so that it triggers after one frame held
        _interactable.HoldSecs = 0.001f;

        var pressed = CreateActionEvent(_interactable.InteractAction, true);

        bool fired = false;
        _interactable.Interacted += () =>
        {
            GD.Print("Interacted event fired!");
            fired = true;
        };
        _godot.GodotInstance.Iteration(1);

        _interactable.InteractAllowed = false;

        Input.ActionPress(_interactable.InteractAction);
        _godot.GodotInstance.Iteration(2);

        fired.ShouldBeFalse();
        _interactable.HoldTimer.ShouldBe(0.0f);
    }

    [Fact]
    public void GivenInteractable_WhenHoldTimerNonZero_ThenHoldTimerUIDisplays()
    {
        _interactable.HoldSecs = 0.5f; //default

        var pressed = CreateActionEvent(_interactable.InteractAction, true);

        bool fired = false;
        _interactable.DisplayingHoldUI += () =>
        {
            GD.Print("Hold event fired!");
            fired = true;
        };
        _godot.GodotInstance.Iteration(1);

        _interactable.InteractAllowed = true;

        Input.ActionPress(_interactable.InteractAction);
        _godot.GodotInstance.Iteration(2);

        fired.ShouldBeTrue();
        _interactable.HoldTimer.ShouldBeGreaterThan(0.0f);
    }

    [Fact]
    public void GivenInteractable_WhenHoldTimerZero_ThenHoldTimerUIDoesNotDisplay()
    {
        _interactable.HoldSecs = 0.5f;

        var pressed = CreateActionEvent(_interactable.InteractAction, true);

        bool fired = false;
        _interactable.DisplayingHoldUI += () =>
        {
            GD.Print("Hold event fired!");
            fired = true;
        };
        _godot.GodotInstance.Iteration(1);

        _interactable.InteractAllowed = false;

        Input.ActionPress(_interactable.InteractAction);
        _godot.GodotInstance.Iteration(2);

        fired.ShouldBeFalse();
        _interactable.HoldTimer.ShouldBe(0.0f);
    }

    [Fact]
    public void GivenInteractable_AfterInteracted_HoldTimerShouldBeZero()
    {
        _interactable.HoldSecs = 0.001f;

        var pressed = CreateActionEvent(_interactable.InteractAction, true);

        bool fired = false;
        _interactable.Interacted += () =>
        {
            GD.Print("Interacted event fired!");
            fired = true;
        };
        _godot.GodotInstance.Iteration(1);

        _interactable.InteractAllowed = true;

        Input.ActionPress(_interactable.InteractAction);
        _godot.GodotInstance.IterateUntil(() => fired == true);

        //at interaction, when still holding, reset the timer
        _interactable.HoldTimer.ShouldBe(0.0f);

        //after interaction, continuing to hold, timer should remain reset
        _godot.GodotInstance.Iteration(1);
        _interactable.HoldTimer.ShouldBe(0.0f);
    }

}
