using CrossedDimensions.States;
using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.States;

[TestSuite]
public partial class StateMachineTest
{
    private partial class DummyState : CrossedDimensions.States.State { }

    private partial class TestStateThatTransitions : CrossedDimensions.States.State
    {
        public override State Process(double delta)
        {
            var parent = GetParent();
            return parent.GetNode<State>("TestSibling");
        }
    }

    [TestCase]
    [RequireGodotRuntime]
    public void StateMachine_ShouldSetContextAndEnterInitialState()
    {
        var machine = new CrossedDimensions.States.StateMachine();

        var contextNode = new Node();
        contextNode.AddChild(machine);

        var initial = new DummyState();
        machine.AddChild(initial);
        machine.InitialState = initial;

        AddNode(contextNode);

        machine.Initialize(contextNode);

        AssertThat(machine.Context).IsEqual(contextNode);
        AssertThat(machine.CurrentState).IsEqual(initial);
        AssertThat(initial.Context).IsEqual(contextNode);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void StateMachine_ShouldChangeStateByTypeAndName()
    {
        var machine = new CrossedDimensions.States.StateMachine();

        var childA = new DummyState();
        childA.Name = "A";
        var childB = new DummyState();
        childB.Name = "B";

        machine.AddChild(childA);
        machine.AddChild(childB);

        AddNode(machine);

        machine.ChangeState<DummyState>();
        // Should pick the first DummyState child (childA)
        AssertThat(machine.CurrentState).IsEqual(childA);

        machine.ChangeState("B");
        AssertThat(machine.CurrentState).IsEqual(childB);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void StateMachine_ShouldProcessAndHandleTransition()
    {
        var machine = new CrossedDimensions.States.StateMachine();
        var contextNode = new Node();
        var s1 = new TestStateThatTransitions();
        var s2 = new DummyState();
        s2.Name = "TestSibling";

        contextNode.AddChild(machine);

        machine.AddChild(s1);
        machine.AddChild(s2);

        AddNode(contextNode);

        machine.InitialState = s1;
        machine.Initialize(contextNode);

        // s1.Process should return s2 to cause a transition
        machine.Process(0.016);
        AssertThat(machine.CurrentState).IsEqual(s2);
    }
}
