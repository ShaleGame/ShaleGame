using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests;

[TestSuite]
public class StringAssertTest
{
    [TestCase]
    public void IsEqual()
    {
        AssertThat("This is a test message").IsEqual("This is a test message");
    }

    [TestCase]
    [RequireGodotRuntime]
    public void TestGodotNode()
    {
        AssertThat(new CrossedDimensions.States.Characters.CharacterStateMachine())
            .IsNotNull();
    }
}
