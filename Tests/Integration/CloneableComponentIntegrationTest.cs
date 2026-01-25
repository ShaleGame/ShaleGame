using CrossedDimensions.Characters;
using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.Integration;

[TestSuite]
[TestCategory("Integration")]
public partial class CloneableComponentIntegrationTest
{
    private Node _scene;
    private Character _character;

    [BeforeTest]
    public void SetupTest()
    {
        var runner = ISceneRunner.Load("res://Characters/Character.tscn");
        _scene = runner.Scene();
        _character = _scene as Character;
    }

    [AfterTest]
    public void TearDownTest()
    {
        _scene?.QueueFree();
        _scene = null;
    }

    [TestCase]
    [RequireGodotRuntime]
    public void GivenCharacter_WhenLoaded_ShouldInitializeCorrectly()
    {
        // smoke test to ensure the character and its CloneableComponent load
        // without errors
        AssertThat(_character).IsNotNull();
        AssertThat(_character.Cloneable).IsNotNull();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CloneableComponent_WhenLoaded_ShouldInitializeCorrectly()
    {
        var cloneable = _character.Cloneable;

        AssertThat(cloneable).IsNotNull();
        AssertThat(cloneable.Original).IsNull();
        AssertThat(cloneable.Clone).IsNull();
        AssertThat(cloneable.SplitForce).IsEqual(768f);
        AssertThat(cloneable.IsClone).IsFalse();
        AssertThat(cloneable.Mirror).IsNull();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CloneableComponent_Split_WhenNoCloneExists_ShouldCreateClone()
    {
        var originalCloneable = _character.Cloneable;
        var originalXScale = _character.Controller.XScale;

        var clone = originalCloneable.Split();

        AssertThat(clone).IsNotNull();
        AssertThat(originalCloneable.Clone).IsEqual(clone);
        AssertThat(clone.Cloneable.Original).IsEqual(_character);
        AssertThat(clone.Cloneable.Clone).IsNull();
        AssertThat(clone.Controller.XScale).IsEqual(originalXScale * -1);
        AssertThat(clone.MovementStateMachine.InitialState.Name).IsEqual("Split State");
        AssertThat(clone.GetParent()).IsEqual(_character.GetParent());
        AssertThat(clone.Cloneable.IsClone).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CloneableComponent_Split_ShouldSetSplitState()
    {
        var clone = _character.Cloneable.Split();

        var initialState = clone.MovementStateMachine.InitialState;
        AssertThat(initialState).IsNotNull();
        AssertThat(initialState.Name).IsEqual("Split State");
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CloneableComponent_Mirror_WhenOriginal_ShouldReturnItsClone()
    {
        var clone = _character.Cloneable.Split();

        var mirrorFromOriginal = _character.Cloneable.Mirror;
        AssertThat(mirrorFromOriginal).IsEqual(clone);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CloneableComponent_Mirror_WhenClone_ShouldReturnItsOriginal()
    {
        var clone = _character.Cloneable.Split();

        var mirrorFromClone = clone.Cloneable.Mirror;
        AssertThat(mirrorFromClone).IsEqual(_character);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CloneableComponent_Split_WhenCloneExists_ShouldReturnNull()
    {
        var firstClone = _character.Cloneable.Split();

        var secondClone = _character.Cloneable.Split();

        AssertThat(secondClone).IsNull();
        AssertThat(_character.Cloneable.Clone).IsEqual(firstClone);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CloneableComponent_Merge_WhenCalledOnOriginal_ShouldFreeClone()
    {
        var clone = _character.Cloneable.Split();
        var cloneInstanceId = clone.GetInstanceId();

        _character.Cloneable.Merge();

        AssertThat(_character.Cloneable.Clone).IsNull();
        AssertThat(clone.IsQueuedForDeletion()).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CloneableComponent_Merge_WhenCalledOnClone_ShouldFreeItself()
    {
        var clone = _character.Cloneable.Split();

        clone.Cloneable.Merge();

        AssertThat(_character.Cloneable.Clone).IsNull();
        AssertThat(clone.IsQueuedForDeletion()).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CloneableComponent_Merge_WhenNoMirrorExists_ShouldSkipMerge()
    {
        var originalInstanceId = _character.GetInstanceId();

        _character.Cloneable.Merge();

        AssertThat(_character).IsNotNull();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CloneableComponent_SplitAndMerge_ShouldWorkCorrectly()
    {
        var originalCloneable = _character.Cloneable;

        var clone = originalCloneable.Split();

        AssertThat(originalCloneable.Clone).IsEqual(clone);
        AssertThat(clone.Cloneable.Original).IsEqual(_character);

        originalCloneable.Merge();

        AssertThat(originalCloneable.Clone).IsNull();
        AssertThat(clone.IsQueuedForDeletion()).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CloneableComponent_Properties_ShouldMaintainConsistentState()
    {
        var clone = _character.Cloneable.Split();

        AssertThat(_character.Cloneable.IsClone).IsFalse();
        AssertThat(_character.Cloneable.Mirror).IsEqual(clone);
        AssertThat(clone.Cloneable.IsClone).IsTrue();
        AssertThat(clone.Cloneable.Mirror).IsEqual(_character);
        AssertThat(_character.Cloneable.SplitForce).IsEqual(clone.Cloneable.SplitForce);
    }
}
