using CrossedDimensions.Environment.Cutscene;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.Environment.Cutscene;

[TestSuite]
public partial class DialoguePlayerTest
{
    [TestCase]
    [RequireGodotRuntime]
    public void StartDialogue_ShouldSetDialogueActiveTrue()
    {
        var chat_player = new DialoguePlayer
        {
            DialogueActive = false
        };

        var chat_reel = new DialogueReel();

        chat_player.StartDialogue(chat_reel);

        AssertThat(chat_player.DialogueActive).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void EndDialogue_ShouldSetDialogueActiveFalse()
    {
        var chat_player = new DialoguePlayer
        {
            DialogueActive = true
        };

        chat_player.EndDialogue();

        AssertThat(chat_player.DialogueActive).IsFalse();
    }
}