using System.Linq;
using CrossedDimensions.Environment.Cutscene;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.Integration;

[Collection("GodotHeadless")]
public partial class DialoguePlayerIntegrationTest : System.IDisposable
{
    private GodotHeadlessFixedFpsFixture _godot;

    private Node _scene;
    private DialoguePlayer _chatPlayer;
    private DialogueReel _chatReel;
    private DialogueFrame _chatFrameA;
    private DialogueFrame _chatFrameB;
    private DialogueFrame _chatFrameC;

    public const string ScenePath = $"{Paths.TestPath}/Integration/DialoguePlayerIntegrationTest.tscn";

    public DialoguePlayerIntegrationTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;

        var packed = ResourceLoader.Load<PackedScene>(ScenePath);
        _scene = packed.Instantiate<Node>();
        _godot.Tree.Root.AddChild(_scene);

        _chatPlayer = _scene.GetNode<DialoguePlayer>("DialoguePlayer");
        _chatReel = _scene.GetNode<DialogueReel>("Dialogue Reel");
        _chatFrameA = _scene.GetNode<DialogueFrame>("Frame A");
        _chatFrameB = _scene.GetNode<DialogueFrame>("Frame B");
        _chatFrameC = _scene.GetNode<DialogueFrame>("Frame C");
    }

    public void Dispose()
    {
        _scene?.QueueFree();
        _scene = null;
    }

    //TODO: rewrite below to match new framework

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
        chat_reel.Frames.Append(chat_frame_a);
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
    public void StartDialogue_ShouldCallLoadFrame()
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

        bool loading = false;
        chat_player.Loading += () =>
        {
            GD.Print("Loading event fired!");
            loading = true;
        };

        chat_player.StartDialogue(chat_reel);
        
        //detect 'loading signal'
        AssertThat(loading).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void LoadFrame_ShouldEmitLoadingSignal()
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

        bool loading = false;
        chat_player.Loading += () =>
        {
            GD.Print("Loading event fired!");
            loading = true;
        };

        chat_player.LoadFrame(chat_frame);
        
        //detect 'loading signal'
        AssertThat(loading).IsTrue();
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
            CurrentFrame = chat_frame,
            displayText = "",
            targetText = chat_frame.Text,
            currentMode = DialoguePlayer.textAdvanceMode.printing
        };

        //simulate process time;
        chat_player._Process(1.0);
        
        AssertThat(chat_player.targetText).Contains(chat_player.displayText);
        AssertThat(chat_player.displayText).IsNotEmpty();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Process_ShouldNotAppendDisplayText_IfNotPrinting()
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
            CurrentFrame = chat_frame,
            displayText = "",
            targetText = chat_frame.Text,
            currentMode = DialoguePlayer.textAdvanceMode.loading
        };

        //simulate process time;
        chat_player._Process(0.3);

        AssertThat(chat_player.displayText).IsEmpty();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Process_ShouldSetCurrentModeReady_IfPrintingAndEndOfCurrentFrameText()
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
            CurrentFrame = chat_frame,
            displayText = "",
            targetText = chat_frame.Text,
            currentMode = DialoguePlayer.textAdvanceMode.printing
        };

        //simulate process time; 1 sec = 60 frames
        chat_player._Process(1.0); 

        AssertThat(chat_player.targetText).IsEqual(chat_player.displayText);
        AssertThat(chat_player.currentMode == DialoguePlayer.textAdvanceMode.ready).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void InteractPressed_ShouldAdvanceToReady_IfPrinting()
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
            CurrentFrame = chat_frame,
            displayText = "",
            targetText = chat_frame.Text,
            currentMode = DialoguePlayer.textAdvanceMode.printing
        };

        //simulate button press
        var pressed = CreateActionEvent(chat_player.AdvanceAction);

        Input.ParseInputEvent(pressed);
        
        AssertThat(chat_player.displayText).IsEqual(chat_player.targetText);
        AssertThat(chat_player.currentMode == DialoguePlayer.textAdvanceMode.ready).IsTrue();

    }

    [TestCase]
    [RequireGodotRuntime]
    public void InteractPressed_ShouldAdvanceText_IfReady()
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
            CurrentFrame = chat_frame,
            displayText = chat_frame.Text,
            targetText = chat_frame.Text,
            currentMode = DialoguePlayer.textAdvanceMode.ready
        };

        bool advancing = false;
        chat_player.Advancing += () =>
        {
            GD.Print("Advancing event fired!");
            advancing = true;
        };

        //simulate button press
        var pressed = CreateActionEvent(chat_player.AdvanceAction, true);

        Input.ParseInputEvent(pressed);
        
        //detect 'advancing signal'
        AssertThat(advancing).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void AdvanceText_ShouldSetCurrentModeLoading_IfFramesRemain()
    {
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
        chat_reel.Frames.Append(chat_frame_a);
        chat_reel.Frames.Append(chat_frame_b);

        var chat_player = new DialoguePlayer
        {
            DialogueActive = true,
            CurrentReel = chat_reel,
            CurrentFrame = chat_frame_a,
            displayText = chat_frame_a.Text,
            targetText = chat_frame_a.Text,
            currentMode = DialoguePlayer.textAdvanceMode.ready
        };
        chat_player.ScriptQueue.Enqueue(chat_frame_b);

        chat_player.AdvanceText();
        
        AssertThat(chat_player.currentMode == DialoguePlayer.textAdvanceMode.loading).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void AdvanceText_ShouldClearDisplayAndTargetText()
    {
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
        chat_reel.Frames.Append(chat_frame_a);
        chat_reel.Frames.Append(chat_frame_b);

        var chat_player = new DialoguePlayer
        {
            DialogueActive = true,
            CurrentReel = chat_reel,
            CurrentFrame = chat_frame_a,
            displayText = chat_frame_a.Text,
            targetText = chat_frame_a.Text,
            currentMode = DialoguePlayer.textAdvanceMode.ready
        };
        chat_player.ScriptQueue.Enqueue(chat_frame_b);

        chat_player.AdvanceText();

        AssertThat(chat_player.displayText).IsEmpty();
        AssertThat(chat_player.targetText).IsEmpty();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void AdvanceText_ShouldLoadNextFrameFromQueue_IfFramesRemain()
    {
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
        chat_reel.Frames.Append(chat_frame_a);
        chat_reel.Frames.Append(chat_frame_b);

        var chat_player = new DialoguePlayer
        {
            DialogueActive = true,
            CurrentReel = chat_reel,
            CurrentFrame = chat_frame_a,
            displayText = chat_frame_a.Text,
            targetText = chat_frame_a.Text,
            currentMode = DialoguePlayer.textAdvanceMode.ready
        };
        chat_player.ScriptQueue.Enqueue(chat_frame_b);

        chat_player.AdvanceText();

        AssertThat(chat_player.CurrentFrame == chat_frame_b).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void AdvanceText_ShouldCallLoadFrame_IfFramesRemain()
    {
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
        chat_reel.Frames.Append(chat_frame_a);
        chat_reel.Frames.Append(chat_frame_b);

        var chat_player = new DialoguePlayer
        {
            DialogueActive = true,
            CurrentReel = chat_reel,
            CurrentFrame = chat_frame_a,
            displayText = chat_frame_a.Text,
            targetText = chat_frame_a.Text,
            currentMode = DialoguePlayer.textAdvanceMode.ready
        };
        chat_player.ScriptQueue.Enqueue(chat_frame_b);

        bool loading = false;
        chat_player.Loading += () =>
        {
            GD.Print("Loading event fired!");
            loading = true;
        };

        chat_player.AdvanceText();
        
        //detect 'loading signal'
        AssertThat(loading).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void AdvanceText_ShouldEndDialogue_IfNoMoreFrames()
    {
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
        chat_reel.Frames.Append(chat_frame_a);
        chat_reel.Frames.Append(chat_frame_b);

        var chat_player = new DialoguePlayer
        {
            DialogueActive = true,
            CurrentReel = chat_reel,
            CurrentFrame = chat_frame_a,
            displayText = chat_frame_a.Text,
            targetText = chat_frame_a.Text,
            currentMode = DialoguePlayer.textAdvanceMode.ready
        };
        chat_player.ScriptQueue.Clear();

        bool ending = false;
        chat_player.Ending += () =>
        {
            GD.Print("ending event fired!");
            ending = true;
        };

        chat_player.AdvanceText();

        AssertThat(ending).IsTrue();
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
        chat_reel.Frames.Append(chat_frame_a);
        chat_reel.Frames.Append(chat_frame_b);

        var chat_player = new DialoguePlayer
        {
            DialogueActive = true,
            CurrentReel = chat_reel,
            CurrentFrame = chat_frame_a,
            displayText = chat_frame_a.Text,
            targetText = chat_frame_a.Text,
            currentMode = DialoguePlayer.textAdvanceMode.ready
        };
        chat_player.ScriptQueue.Enqueue(chat_frame_b);

        chat_player.EndDialogue();

        AssertThat(chat_player.ScriptQueue.Count).IsZero();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void EndDialogue_ShouldSetCurrentModeNotReady()
    {
        var chat_player = new DialoguePlayer
        {
            DialogueActive = true,
            currentMode = DialoguePlayer.textAdvanceMode.ready
        };

        chat_player.EndDialogue();

        AssertThat(chat_player.currentMode == DialoguePlayer.textAdvanceMode.not_ready).IsTrue();
    }

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
}
