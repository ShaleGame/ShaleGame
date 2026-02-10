using CrossedDimensions.States;
using Godot;

namespace CrossedDimensions.Tests.States;

internal partial class DummyState : State
{
    public State NextState { get; set; }

    public override State Process(double delta)
    {
        return NextState;
    }
}

[Collection("GodotHeadless")]
public class StateMachineTest(GodotHeadlessFixture godot)
{
    [Fact]
    public void StateMachine_ShouldSetContextAndEnterInitialState()
    {
        var machine = new CrossedDimensions.States.StateMachine();

        var contextNode = new Node();
        contextNode.AddChild(machine);

        var initial = new State();
        machine.AddChild(initial);
        machine.InitialState = initial;

        godot.Tree.Root.AddChild(contextNode);

        machine.Initialize(contextNode);

        machine.Context.ShouldBe(contextNode);
        machine.CurrentState.ShouldBe(initial);
        initial.Context.ShouldBe(contextNode);
    }

    [Fact]
    public void StateMachine_ShouldChangeStateByName()
    {
        var machine = new CrossedDimensions.States.StateMachine();

        var childA = new State();
        childA.Name = "A";
        var childB = new State();
        childB.Name = "B";

        machine.AddChild(childA);
        machine.AddChild(childB);
        machine.InitialState = childA;

        godot.Tree.Root.AddChild(machine);

        machine.ChangeState("B");
        machine.CurrentState.ShouldBe(childB);
    }

    [Fact]
    public void StateMachine_ShouldChangeStateByType()
    {
        var machine = new CrossedDimensions.States.StateMachine();

        var childA = new State();
        var childB = new DummyState();
        childB.Name = "B";

        machine.AddChild(childA);
        machine.AddChild(childB);
        machine.InitialState = childA;

        godot.Tree.Root.AddChild(machine);

        machine.ChangeState<DummyState>();
        machine.CurrentState.ShouldBe(childB);
    }

    [Fact]
    public void StateMachine_ShouldChangeStateByInstance()
    {
        var machine = new CrossedDimensions.States.StateMachine();

        var childA = new State();
        var childB = new State();

        machine.AddChild(childA);
        machine.AddChild(childB);
        machine.InitialState = childA;

        godot.Tree.Root.AddChild(machine);

        machine.ChangeState(childB);
        machine.CurrentState.ShouldBe(childB);
    }

    [Fact]
    public void StateMachine_ShouldTransitionToNextStateReturnedByCurrentState()
    {
        var machine = new CrossedDimensions.States.StateMachine();

        var childA = new DummyState();
        var childB = new State();

        childA.NextState = childB;

        machine.AddChild(childA);
        machine.AddChild(childB);
        machine.InitialState = childA;
        machine.ChangeState(childA);

        godot.Tree.Root.AddChild(machine);

        machine.Process(0);
        machine.CurrentState.ShouldBe(childB);
    }
}
