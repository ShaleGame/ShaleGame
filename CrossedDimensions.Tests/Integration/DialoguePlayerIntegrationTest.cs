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

    [Fact]
    public void GivenScene_WhenLoaded_ShouldInitializeCorrectly()
    {
        _chatPlayer.ShouldNotBeNull();
        _chatReel.ShouldNotBeNull();
        _chatFrameA.ShouldNotBeNull();
        _chatFrameB.ShouldNotBeNull();
        _chatFrameC.ShouldNotBeNull();
    }

    [Fact]
    public void GivenDialoguePlayer_WhenStartDialogue_ShouldSetDialogueActiveTrue()
    {
        _chatPlayer.DialogueActive = false;
        _chatPlayer.StartDialogue(_chatReel);
        _chatPlayer.DialogueActive.ShouldBeTrue();
    }

    [Fact]
    public void GivenDialoguePlayer_WhenStartDialogue_ShouldSetCurrentModeLoading()
    {
        _chatPlayer.currentMode = DialoguePlayer.textAdvanceMode.not_ready;
        _chatPlayer.StartDialogue(_chatReel);
        _chatPlayer.currentMode.ShouldBe(DialoguePlayer.textAdvanceMode.loading);
    }

    [Fact]
    public void GivenDialoguePlayer_WhenStartDialogue_ShouldSetCurrentReel()
    {
        _chatPlayer.StartDialogue(_chatReel);
        _chatPlayer.CurrentReel.ShouldBe(_chatReel);
    }

    [Fact]
    public void GivenDialoguePlayer_WhenStartDialogue_ShouldEnqueueAllFramesInReel()
    {
        _chatReel.Frames = new[]
        {
            _chatFrameA,
            _chatFrameB,
            _chatFrameC
        };

        _chatPlayer.StartDialogue(_chatReel);
        _chatPlayer.ScriptQueue.Contains(_chatFrameA).ShouldBeTrue();
        _chatPlayer.ScriptQueue.Contains(_chatFrameB).ShouldBeTrue();
        _chatPlayer.ScriptQueue.Contains(_chatFrameC).ShouldBeTrue();
    }

    [Fact]
    public void GivenDialoguePlayer_WhenStartDialogue_ShouldSetCurrentFrame()
    {
        _chatReel.Frames = new[]
        {
            _chatFrameA
        };

        _chatPlayer.StartDialogue(_chatReel);
        _chatPlayer.CurrentFrame.ShouldBe(_chatFrameA);
    }

    [Fact]
    public void GivenDialoguePlayer_WhenStartDialogue_ShouldCallLoadFrame()
    {
        InitializeTestFrame(_chatFrameA);

        _chatReel.Frames = new[]
        {
            _chatFrameA
        };

        bool loading = false;
        _chatPlayer.Loading += () => loading = true;

        _chatPlayer.StartDialogue(_chatReel);

        loading.ShouldBeTrue();
    }

    [Fact]
    public void GivenDialoguePlayer_WhenLoadFrame_ShouldSetTargetText()
    {
        InitializeTestFrame(_chatFrameA);

        _chatReel.Frames = new[]
        {
            _chatFrameA
        };

        _chatPlayer.DialogueActive = true;
        _chatPlayer.CurrentReel = _chatReel;

        _chatPlayer.LoadFrame(_chatFrameA);

        _chatPlayer.targetText.ShouldBe(_chatPlayer.CurrentFrame.Text);
    }

    [Fact]
    public void GivenDialoguePlayer_WhenLoadFrame_ShouldSetCurrentModePrinting()
    {
        InitializeTestFrame(_chatFrameA);

        _chatReel.Frames = new[]
        {
            _chatFrameA
        };

        _chatPlayer.DialogueActive = true;
        _chatPlayer.CurrentReel = _chatReel;
        _chatPlayer.currentMode = DialoguePlayer.textAdvanceMode.loading;

        _chatPlayer.LoadFrame(_chatFrameA);

        _chatPlayer.currentMode.ShouldBe(DialoguePlayer.textAdvanceMode.printing);
    }

    [Fact]
    public void GivenDialoguePlayer_WhenProcess_ShouldAppendDisplayText_IfPrinting()
    {
        InitializeTestFrame(_chatFrameA);

        _chatReel.Frames = new[]
        {
            _chatFrameA
        };

        _chatPlayer.DialogueActive = true;
        _chatPlayer.CurrentReel = _chatReel;
        _chatPlayer.CurrentFrame = _chatFrameA;
        _chatPlayer.currentMode = DialoguePlayer.textAdvanceMode.printing;
        _chatPlayer.displayText = "";
        _chatPlayer.targetText = _chatFrameA.Text;

        _godot.GodotInstance.Iteration(1);
        _chatPlayer.targetText.ShouldContain(_chatPlayer.displayText);
        _chatPlayer.displayText.ShouldNotBeEmpty();
    }

    [Fact]
    public void GivenDialoguePlayer_WhenProcess_ShouldNotAppendDisplayText_IfNotPrinting()
    {
        InitializeTestFrame(_chatFrameA);

        _chatReel.Frames = new[]
        {
            _chatFrameA
        };

        _chatPlayer.DialogueActive = true;
        _chatPlayer.CurrentReel = _chatReel;
        _chatPlayer.CurrentFrame = _chatFrameA;
        _chatPlayer.currentMode = DialoguePlayer.textAdvanceMode.loading;
        _chatPlayer.displayText = "";
        _chatPlayer.targetText = _chatFrameA.Text;

        _godot.GodotInstance.Iteration(1);
        _chatPlayer.displayText.ShouldBeEmpty();
    }

    [Fact]
    public void GivenDialoguePlayer_WhenProcess_ShouldSetCurrentModeReady_IfEndOfCurrentFrameTextAndPrinting()
    {
        InitializeTestFrame(_chatFrameA);

        _chatReel.Frames = new[]
        {
            _chatFrameA
        };

        _chatPlayer.DialogueActive = true;
        _chatPlayer.CurrentReel = _chatReel;
        _chatPlayer.CurrentFrame = _chatFrameA;
        _chatPlayer.currentMode = DialoguePlayer.textAdvanceMode.printing;
        _chatPlayer.displayText = "";
        _chatPlayer.targetText = _chatFrameA.Text;

        _godot.GodotInstance.Iteration(20);

        _chatPlayer.displayText.ShouldBe(_chatPlayer.targetText);
        _chatPlayer.currentMode.ShouldBe(DialoguePlayer.textAdvanceMode.ready);
    }

    [Fact]
    public void GivenDialoguePlayer_WhenInteractPressed_ShouldAdvanceToReady_IfPrinting()
    {
        InitializeTestFrame(_chatFrameA);

        _chatReel.Frames = new[]
        {
            _chatFrameA
        };

        _chatPlayer.DialogueActive = true;
        _chatPlayer.CurrentReel = _chatReel;
        _chatPlayer.CurrentFrame = _chatFrameA;
        _chatPlayer.currentMode = DialoguePlayer.textAdvanceMode.printing;
        _chatPlayer.displayText = "";
        _chatPlayer.targetText = _chatFrameA.Text;

        //simulate button press
        var pressed = CreateActionEvent(_chatPlayer.AdvanceAction);

        Input.ParseInputEvent(pressed);
        
        _chatPlayer.displayText.ShouldBe(_chatPlayer.targetText);
        _chatPlayer.currentMode.ShouldBe(DialoguePlayer.textAdvanceMode.ready);

    }

    [Fact]
    public void GivenDialoguePlayer_WhenInteractPressed_ShouldAdvanceText_IfReady()
    {
        InitializeTestFrame(_chatFrameA);

        _chatReel.Frames = new[]
        {
            _chatFrameA
        };

        _chatPlayer.DialogueActive = true;
        _chatPlayer.CurrentReel = _chatReel;
        _chatPlayer.CurrentFrame = _chatFrameA;
        _chatPlayer.currentMode = DialoguePlayer.textAdvanceMode.ready;
        _chatPlayer.displayText = _chatFrameA.Text;
        _chatPlayer.targetText = _chatFrameA.Text;

        bool advancing = false;
        _chatPlayer.Advancing += () => advancing = true;

        //simulate button press
        var pressed = CreateActionEvent(_chatPlayer.AdvanceAction, true);

        Input.ParseInputEvent(pressed);
        
        //detect 'advancing signal'
        advancing.ShouldBeTrue();
    }

    [Fact]
    public void GivenDialoguePlayer_AdvanceText_ShouldSetCurrentModeLoading_IfFramesRemain()
    {
        InitializeTestFrame(_chatFrameA);
        InitializeTestFrame(_chatFrameB);

        var chat_reel = new DialogueReel();
        chat_reel.Frames = new[]
        {
            _chatFrameA,
            _chatFrameB
        };

        var chat_player = new DialoguePlayer
        {
            DialogueActive = true,
            CurrentReel = chat_reel,
            CurrentFrame = _chatFrameA,
            displayText = _chatFrameA.Text,
            targetText = _chatFrameA.Text,
            currentMode = DialoguePlayer.textAdvanceMode.ready
        };
        chat_player.ScriptQueue.Enqueue(_chatFrameB);

        chat_player.AdvanceText();
        
        chat_player.currentMode.ShouldBe(DialoguePlayer.textAdvanceMode.loading);
    }

    [Fact]
    public void GivenDialoguePlayer_AdvanceText_ShouldClearDisplayAndTargetText()
    {
        InitializeTestFrame(_chatFrameA);
        InitializeTestFrame(_chatFrameB);

        var chat_reel = new DialogueReel();
        chat_reel.Frames = new[]
        {
            _chatFrameA,
            _chatFrameB
        };

        var chat_player = new DialoguePlayer
        {
            DialogueActive = true,
            CurrentReel = chat_reel,
            CurrentFrame = _chatFrameA,
            displayText = _chatFrameA.Text,
            targetText = _chatFrameA.Text,
            currentMode = DialoguePlayer.textAdvanceMode.ready
        };
        chat_player.ScriptQueue.Enqueue(_chatFrameB);

        chat_player.AdvanceText();

        chat_player.displayText.ShouldBeEmpty();
        chat_player.targetText.ShouldBeEmpty();
    }

    [Fact]
    public void GivenDialoguePlayer_AdvanceText_ShouldLoadNextFrameFromQueue_IfFramesRemain()
    {
        InitializeTestFrame(_chatFrameA);
        InitializeTestFrame(_chatFrameB);

        var chat_reel = new DialogueReel();
        chat_reel.Frames = new[]
        {
            _chatFrameA,
            _chatFrameB
        };

        var chat_player = new DialoguePlayer
        {
            DialogueActive = true,
            CurrentReel = chat_reel,
            CurrentFrame = _chatFrameA,
            displayText = _chatFrameA.Text,
            targetText = _chatFrameA.Text,
            currentMode = DialoguePlayer.textAdvanceMode.ready
        };
        chat_player.ScriptQueue.Enqueue(_chatFrameB);

        chat_player.AdvanceText();

        chat_player.CurrentFrame.ShouldBe(_chatFrameB);
    }

    [Fact]
    public void GivenDialoguePlayer_AdvanceText_ShouldCallLoadFrame_IfFramesRemain()
    {
        InitializeTestFrame(_chatFrameA);
        InitializeTestFrame(_chatFrameB);

        var chat_reel = new DialogueReel();
        chat_reel.Frames = new[]
        {
            _chatFrameA,
            _chatFrameB
        };

        var chat_player = new DialoguePlayer
        {
            DialogueActive = true,
            CurrentReel = chat_reel,
            CurrentFrame = _chatFrameA,
            displayText = _chatFrameA.Text,
            targetText = _chatFrameA.Text,
            currentMode = DialoguePlayer.textAdvanceMode.ready
        };
        chat_player.ScriptQueue.Enqueue(_chatFrameB);

        bool loading = false;
        chat_player.Loading += () =>
        {
            GD.Print("Loading event fired!");
            loading = true;
        };

        chat_player.AdvanceText();
        
        //detect 'loading signal'
        loading.ShouldBeTrue();
    }

    [Fact]
    public void GivenDialoguePlayer_AdvanceText_ShouldEndDialogue_IfNoMoreFrames()
    {
        InitializeTestFrame(_chatFrameA);
        InitializeTestFrame(_chatFrameB);

        var chat_reel = new DialogueReel();
        chat_reel.Frames = new[]
        {
            _chatFrameA,
            _chatFrameB
        };

        var chat_player = new DialoguePlayer
        {
            DialogueActive = true,
            CurrentReel = chat_reel,
            CurrentFrame = _chatFrameA,
            displayText = _chatFrameA.Text,
            targetText = _chatFrameA.Text,
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

        ending.ShouldBeTrue();
    }

    [Fact]
    public void GivenDialoguePlayer_EndDialogue_ShouldSetDialogueActiveFalse()
    {
        var chat_player = new DialoguePlayer
        {
            DialogueActive = true
        };

        chat_player.EndDialogue();

        chat_player.DialogueActive.ShouldBeFalse();
    }

    [Fact]
    public void GivenDialoguePlayer_EndDialogue_ShouldInitializeQueues()
    {
        InitializeTestFrame(_chatFrameA);
        InitializeTestFrame(_chatFrameB);

        var chat_reel = new DialogueReel();
        chat_reel.Frames = new[]
        {
            _chatFrameA,
            _chatFrameB
        };

        var chat_player = new DialoguePlayer
        {
            DialogueActive = true,
            CurrentReel = chat_reel,
            CurrentFrame = _chatFrameA,
            displayText = _chatFrameA.Text,
            targetText = _chatFrameA.Text,
            currentMode = DialoguePlayer.textAdvanceMode.ready
        };
        chat_player.ScriptQueue.Enqueue(_chatFrameB);

        chat_player.EndDialogue();

        chat_player.ScriptQueue.Count.ShouldBe(0);
    }

    [Fact]
    public void GivenDialoguePlayer_EndDialogue_ShouldSetCurrentModeNotReady()
    {
        var chat_player = new DialoguePlayer
        {
            DialogueActive = true,
            currentMode = DialoguePlayer.textAdvanceMode.ready
        };

        chat_player.EndDialogue();

        chat_player.currentMode.ShouldBe(DialoguePlayer.textAdvanceMode.not_ready);
    }

    private static void InitializeTestFrame(DialogueFrame frame)
    {
        frame.Speaker = "Test speaker";
        frame.Text = "Test text";

        frame.Portrait = new Texture2D[0];
        frame.PortraitPosition = new Vector2[]
        {
            new Vector2(0, 0)
        };
    }

    private static DialogueFrame CreateTestFrame()
    {
        return new DialogueFrame
        {
            Speaker = "Test speaker",
            Text = "Test text",
            Portrait = new Texture2D[0],
            PortraitPosition = new Vector2[]
            {
                new Vector2(0, 0)
            }
        };
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
