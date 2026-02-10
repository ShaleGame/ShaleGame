using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using CrossedDimensions.Environment.Cutscene;
using Microsoft.VisualBasic;

namespace CrossedDimensions.Tests.Environment.Cutscene;

[TestSuite]
public partial class InteractableTest
{

    // -------------------------------------------------
    // Defaults
    // -------------------------------------------------

    [TestCase]
    [RequireGodotRuntime]
    public void DefaultsAreCorrect()
    {
        var _interactable = new Interactable();
        AssertThat(_interactable.HoldSecs).IsEqual(1.0f);
        AssertThat(_interactable.InteractPriority).IsEqual(0);
        AssertThat(_interactable.InteractAction.ToString()).IsEqual("interact");
    }

    // -------------------------------------------------
    // Interaction gating
    // -------------------------------------------------

    [TestCase]
    [RequireGodotRuntime]
    public void BodyEnter_EnablesInteraction()
    {
        var _interactable = new Interactable();
        var player = new CharacterBody2D();

        _interactable.OnArea2DBodyEntered(player);

        // simulate frame
        _interactable._Process(0.1);

        // hold timer shouldn't reset anymore, meaning allowed
        AssertThat(_interactable.InteractAllowed).IsTrue(); // no crash = success
    }

    [TestCase]
    [RequireGodotRuntime]
    public void BodyExit_DisablesInteraction()
    {
        var _interactable = new Interactable();
        var player = new CharacterBody2D();

        _interactable.OnArea2DBodyEntered(player);
        _interactable.OnArea2DBodyExited(player);

        _interactable._Process(2.0);

        AssertThat(_interactable.InteractAllowed).IsFalse();
    }

    // -------------------------------------------------
    // Hold logic
    // -------------------------------------------------

    [TestCase]
    [RequireGodotRuntime]
    public void DoesNotInteract_IfNotAllowed()
    {
        var _interactable = new Interactable();

        //check for the interact signal
        bool fired = false;
        _interactable.Interacted += () => fired = true;

        //simulate held frames
        for (int i = 0; i < 20; i++)
        {
            Input.ActionPress("interact");
            _interactable._Process(0.1);
        }

        //since no player entering the Area2D, signal should not fire
        AssertThat(fired).IsFalse();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Interacts_AfterHoldTimeReached()
    {
        var _interactable = new Interactable();
        var player = new CharacterBody2D();

        //check for the interact signal
        bool fired = false;
        _interactable.Interacted += () => fired = true;

        //player enters interactable body
        _interactable.OnArea2DBodyEntered(player);

        //simulate held frames
        for (int i = 0; i < 20; i++)
        {
            Input.ActionPress("interact");
            _interactable._Process(0.1);
        }

        //since the player entered the Area2D, signal should fire
        AssertThat(fired).IsTrue();

        Input.ActionRelease("interact");
    }

    [TestCase]
    [RequireGodotRuntime]
    public void ReleasingEarly_ResetsHoldTimer()
    {
        var _interactable = new Interactable();
        var player = new CharacterBody2D();

        //check for the interact signal
        bool fired = false;
        _interactable.Interacted += () => fired = true;

        //player enters interactable body
        _interactable.OnArea2DBodyEntered(player);

        // press briefly
        Input.ActionPress("interact");
        _interactable._Process(0.3);

        // release
        Input.ActionRelease("interact");
        _interactable._Process(0.1);

        // press again but not enough time
        Input.ActionPress("interact");
        _interactable._Process(0.3);

        //since input was not held for long enough, signal should not fire
        AssertThat(fired).IsFalse();

        Input.ActionRelease("interact");
    }
}