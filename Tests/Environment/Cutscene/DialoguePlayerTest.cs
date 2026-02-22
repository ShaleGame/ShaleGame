using System.Linq;
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

    public void StartDialogue_ShouldSetCurrentModeLoading()
    {
        var chat_player = new DialoguePlayer
        {
            DialogueActive = false
        };

        var chat_reel = new DialogueReel();

        chat_player.StartDialogue(chat_reel);

        AssertThat(chat_player.currentMode == DialoguePlayer.textAdvanceMode.loading);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void StartDialogue_ShouldLoadReel()
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
    public void StartDialogue_ShouldEnqueueAllFramesInReel()
    {
        var chat_player = new DialoguePlayer
        {
            DialogueActive = false
        };

        var chat_frame_a = new DialogueFrame
        {
            Speaker = "Test speaker",
            Text = "Test text",
        };
        chat_frame_a.PortraitPosition.Append(new Godot.Vector2(0,0));

        var chat_frame_b = new DialogueFrame
        {
            Speaker = "Test speaker",
            Text = "Test text",
        };
        chat_frame_b.PortraitPosition.Append(new Godot.Vector2(0,0));

        var chat_reel = new DialogueReel();
        chat_reel.Frames.Append(chat_frame_b);

        chat_player.StartDialogue(chat_reel);

        AssertThat(chat_player.ScriptQueue).Contains(chat_frame_a);
        AssertThat(chat_player.ScriptQueue).Contains(chat_frame_b);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void StartDialogue_ShouldLoadFirstFrameFromQueue()
    {
        var chat_player = new DialoguePlayer
        {
            DialogueActive = false
        };

        var chat_frame = new DialogueFrame
        {
            Speaker = "Test speaker",
            Text = "Test text",
        };
        chat_frame.PortraitPosition.Append(new Godot.Vector2(0,0));

        var chat_reel = new DialogueReel();
        chat_reel.Frames.Append(chat_frame);

        chat_player.StartDialogue(chat_reel);

        AssertThat(chat_player.CurrentFrame == chat_frame).IsTrue(); 
    }

    [TestCase]
    [RequireGodotRuntime]
    public void LoadFrame_ShouldSetTargetText()
    {
        var chat_frame = new DialogueFrame
        {
            Speaker = "Test speaker",
            Text = "Test text",
        };
        chat_frame.PortraitPosition.Append(new Godot.Vector2(0,0));

        var chat_reel = new DialogueReel();
        chat_reel.Frames.Append(chat_frame);

        var chat_player = new DialoguePlayer
        {
            DialogueActive = true,
            CurrentReel = chat_reel,
        };

        chat_player.LoadFrame(chat_frame);

        AssertThat(chat_player.targetText == chat_player.CurrentFrame.Text).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void LoadFrame_ShouldSetCurrentModePrinting()
    {
        var chat_frame = new DialogueFrame
        {
            Speaker = "Test speaker",
            Text = "Test text",
        };
        chat_frame.PortraitPosition.Append(new Godot.Vector2(0,0));

        var chat_reel = new DialogueReel();
        chat_reel.Frames.Append(chat_frame);

        var chat_player = new DialoguePlayer
        {
            DialogueActive = true,
            CurrentReel = chat_reel,
        };

        chat_player.LoadFrame(chat_frame);

        AssertThat( chat_player.currentMode == DialoguePlayer.textAdvanceMode.printing );
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Process_ShouldAppendDisplayText_IfPrinting()
    {
        
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Process_ShouldNotAppendDisplayText_IfNotPrinting()
    {
        
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Process_ShouldSetCurrentModeReady_IfPrintingAndEndOfCurrentFrameText()
    {
        
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Process_ShouldSetAdvanceText_IfReadyAndInteractPressed()
    {
        
    }

    [TestCase]
    [RequireGodotRuntime]
    public void AdvanceText_ShouldSetCurrentModeLoading()
    {
        
    }

    [TestCase]
    [RequireGodotRuntime]
    public void AdvanceText_ShouldLoadNextFrameFromReel()
    {
        
    }

    [TestCase]
    [RequireGodotRuntime]
    public void AdvanceText_ShouldEndDialogue_IfNoMoreFrames()
    {
        
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

    [TestCase]
    [RequireGodotRuntime]
    public void EndDialogue_ShouldInitializeQueues()
    {
        
    }

    [TestCase]
    [RequireGodotRuntime]
    public void EndDialogue_ShouldSetCurrentModeNotReady()
    {
        
    }

}
