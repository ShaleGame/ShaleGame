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

    //constructor
    public DialoguePlayerIntegrationTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;

        var packed = ResourceLoader.Load<PackedScene>(ScenePath);
        _scene = packed.Instantiate<Node>();
        _godot.Tree.Root.AddChild(_scene);

        _chatPlayer = _scene.GetNode<DialoguePlayer>("DialoguePlayer");
        _chatReel = _scene.GetNode<DialogueReel>("DialogueReel");
        _chatFrameA = _scene.GetNode<DialogueFrame>("FrameA");
        _chatFrameB = _scene.GetNode<DialogueFrame>("FrameB");
        _chatFrameC = _scene.GetNode<DialogueFrame>("FrameC");
        ResetPlayerState();
    }

    //destructor
    public void Dispose()
    {
        _scene?.QueueFree();
        _scene = null;
    }

    //helpers
    private void ResetPlayerState()
    {
        if (_chatPlayer == null)
            return;

        _chatPlayer.DialogueActive = false;
        _chatPlayer.CurrentReel = null;
        _chatPlayer.CurrentFrame = null;
        _chatPlayer.displayText = "";
        _chatPlayer.targetText = "";
        _chatPlayer.currentMode = DialoguePlayer.textAdvanceMode.not_ready;

        _chatPlayer.ScriptQueue.Clear();
    }

    private void ArrangeStartDialogueState(DialogueReel reel)
    {
        _chatPlayer.DialogueActive = false;
        _chatPlayer.CurrentReel = reel;
        _chatPlayer.CurrentFrame = null;
        _chatPlayer.displayText = "";
        _chatPlayer.targetText = "";
        _chatPlayer.currentMode = DialoguePlayer.textAdvanceMode.not_ready;
        _chatPlayer.ScriptQueue.Clear();
    }

    private void ArrangePrintingState(DialogueFrame currentFrame, DialogueReel reel)
    {
        _chatPlayer.DialogueActive = true;
        _chatPlayer.CurrentReel = reel;
        _chatPlayer.CurrentFrame = currentFrame;
        _chatPlayer.displayText = "";
        _chatPlayer.targetText = currentFrame.Text;
        _chatPlayer.currentMode = DialoguePlayer.textAdvanceMode.printing;
    }

    private void ArrangeReadyState(DialogueFrame currentFrame, DialogueReel reel)
    {
        _chatPlayer.DialogueActive = true;
        _chatPlayer.CurrentReel = reel;
        _chatPlayer.CurrentFrame = currentFrame;
        _chatPlayer.displayText = currentFrame.Text;
        _chatPlayer.targetText = currentFrame.Text;
        _chatPlayer.currentMode = DialoguePlayer.textAdvanceMode.ready;

        _chatPlayer.ScriptQueue.Clear();
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

    private DialogueReel CreateTwoFrameReel(DialogueFrame frameA, DialogueFrame frameB)
    {
        _chatReel.Frames = new[]
        {
            frameA,
            frameB
        };
        return _chatReel;
    }

    private DialogueReel CreateSingleFrameReel(DialogueFrame frame)
    {
        _chatReel.Frames = new[]
        {
            frame
        };
        return _chatReel;
    }

    private DialogueReel CreateTripleFrameReel(DialogueFrame frameA, DialogueFrame frameB, DialogueFrame frameC)
    {
        _chatReel.Frames = new[]
        {
            frameA,
            frameB,
            frameC
        };
        return _chatReel;
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

    //tests
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
        var chat_reel = _chatReel;
        ArrangeStartDialogueState(chat_reel);
        _chatPlayer.StartDialogue(chat_reel);
        _chatPlayer.DialogueActive.ShouldBeTrue();
    }

    [Fact]
    public void GivenDialoguePlayer_WhenStartDialogue_ShouldSetCurrentModeLoading()
    {
        var chat_reel = _chatReel;
        ArrangeStartDialogueState(chat_reel);
        _chatPlayer.currentMode = DialoguePlayer.textAdvanceMode.not_ready;
        _chatPlayer.StartDialogue(chat_reel);
        _chatPlayer.currentMode.ShouldBe(DialoguePlayer.textAdvanceMode.loading);
    }

    [Fact]
    public void GivenDialoguePlayer_WhenStartDialogue_ShouldSetCurrentReel()
    {
        var chat_reel = _chatReel;
        ArrangeStartDialogueState(chat_reel);
        _chatPlayer.StartDialogue(chat_reel);
        _chatPlayer.CurrentReel.ShouldBe(chat_reel);
    }

    [Fact]
    public void GivenDialoguePlayer_WhenStartDialogue_ShouldEnqueueAllFramesInReel()
    {
        var chat_reel = CreateTripleFrameReel(_chatFrameA, _chatFrameB, _chatFrameC);
        ArrangeStartDialogueState(chat_reel);
        _chatPlayer.StartDialogue(chat_reel);
        _chatPlayer.ScriptQueue.Contains(_chatFrameA).ShouldBeTrue();
        _chatPlayer.ScriptQueue.Contains(_chatFrameB).ShouldBeTrue();
        _chatPlayer.ScriptQueue.Contains(_chatFrameC).ShouldBeTrue();
    }

    [Fact]
    public void GivenDialoguePlayer_WhenStartDialogue_ShouldSetCurrentFrame()
    {
        var chat_reel = CreateSingleFrameReel(_chatFrameA);
        ArrangeStartDialogueState(chat_reel);
        _chatPlayer.StartDialogue(chat_reel);
        _chatPlayer.CurrentFrame.ShouldBe(_chatFrameA);
    }

    [Fact]
    public void GivenDialoguePlayer_WhenStartDialogue_ShouldCallLoadFrame()
    {
        InitializeTestFrame(_chatFrameA);

        var chat_reel = CreateSingleFrameReel(_chatFrameA);
        ArrangeStartDialogueState(chat_reel);

        bool loading = false;
        _chatPlayer.Loading += () => loading = true;

        _chatPlayer.StartDialogue(chat_reel);

        loading.ShouldBeTrue();
    }

    [Fact]
    public void GivenDialoguePlayer_WhenLoadFrame_ShouldSetTargetText()
    {
        InitializeTestFrame(_chatFrameA);

        var chat_reel = CreateSingleFrameReel(_chatFrameA);

        _chatPlayer.DialogueActive = true;
        _chatPlayer.CurrentReel = chat_reel;

        _chatPlayer.LoadFrame(_chatFrameA);

        _chatPlayer.targetText.ShouldBe(_chatPlayer.CurrentFrame.Text);
    }

    [Fact]
    public void GivenDialoguePlayer_WhenLoadFrame_ShouldSetCurrentModePrinting()
    {
        InitializeTestFrame(_chatFrameA);

        var chat_reel = CreateSingleFrameReel(_chatFrameA);

        _chatPlayer.DialogueActive = true;
        _chatPlayer.CurrentReel = chat_reel;
        _chatPlayer.currentMode = DialoguePlayer.textAdvanceMode.loading;

        _chatPlayer.LoadFrame(_chatFrameA);

        _chatPlayer.currentMode.ShouldBe(DialoguePlayer.textAdvanceMode.printing);
    }

    [Fact]
    public void GivenDialoguePlayer_WhenProcess_ShouldAppendDisplayText_IfPrinting()
    {
        InitializeTestFrame(_chatFrameA);

        var chat_reel = CreateSingleFrameReel(_chatFrameA);

        ArrangePrintingState(_chatFrameA, chat_reel);

        _godot.GodotInstance.Iteration(1);
        _chatPlayer.targetText.ShouldContain(_chatPlayer.displayText);
        _chatPlayer.displayText.ShouldNotBeEmpty();
    }

    [Fact]
    public void GivenDialoguePlayer_WhenProcess_ShouldNotAppendDisplayText_IfNotPrinting()
    {
        InitializeTestFrame(_chatFrameA);

        var chat_reel = CreateSingleFrameReel(_chatFrameA);

        _chatPlayer.DialogueActive = true;
        _chatPlayer.CurrentReel = chat_reel;
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

        var chat_reel = CreateSingleFrameReel(_chatFrameA);

        ArrangePrintingState(_chatFrameA, chat_reel);

        _godot.GodotInstance.Iteration(20);

        _chatPlayer.displayText.ShouldBe(_chatPlayer.targetText);
        _chatPlayer.currentMode.ShouldBe(DialoguePlayer.textAdvanceMode.ready);
    }

    [Fact]
    public void GivenDialoguePlayer_WhenInteractPressed_ShouldAdvanceToReady_IfPrinting()
    {
        InitializeTestFrame(_chatFrameA);
        var chat_reel = CreateSingleFrameReel(_chatFrameA);
        ArrangePrintingState(_chatFrameA, chat_reel);

        // Use iterator to progress printing
        var iterator = _chatPlayer.GetDialogueIterator();
        // Printing: should step until displayText == targetText
        while (_chatPlayer.displayText != _chatPlayer.targetText)
        {
            iterator.MoveNext();
        }
        _chatPlayer.displayText.ShouldBe(_chatPlayer.targetText);
        _chatPlayer.currentMode.ShouldBe(DialoguePlayer.textAdvanceMode.ready);
    }

    [Fact]
    public void GivenDialoguePlayer_WhenInteractPressed_ShouldAdvanceText_IfReady()
    {
        InitializeTestFrame(_chatFrameA);
        var chat_reel = CreateSingleFrameReel(_chatFrameA);
        ArrangeReadyState(_chatFrameA, chat_reel);
        bool advancing = false;
        _chatPlayer.Advancing += () => advancing = true;
        // Use iterator to progress past ready state
        var iterator = _chatPlayer.GetDialogueIterator();
        // First MoveNext should trigger advancing event
        iterator.MoveNext();
        advancing.ShouldBeTrue();
    }

    [Fact]
    public void GivenDialoguePlayer_AdvanceText_ShouldSetCurrentModeLoading_IfFramesRemain()
    {
        InitializeTestFrame(_chatFrameA);
        InitializeTestFrame(_chatFrameB);
        var chat_reel = CreateTwoFrameReel(_chatFrameA, _chatFrameB);
        ArrangeReadyState(_chatFrameA, chat_reel);
        var chat_player = _chatPlayer;
        chat_player.ScriptQueue.Enqueue(_chatFrameB);
        // Use iterator to advance to next frame
        var iterator = chat_player.GetDialogueIterator();
        iterator.MoveNext();
        chat_player.currentMode.ShouldBe(DialoguePlayer.textAdvanceMode.loading);
    }

    [Fact]
    public void GivenDialoguePlayer_AdvanceText_ShouldClearDisplayAndTargetText()
    {
        InitializeTestFrame(_chatFrameA);
        InitializeTestFrame(_chatFrameB);
        var chat_reel = CreateTwoFrameReel(_chatFrameA, _chatFrameB);
        ArrangeReadyState(_chatFrameA, chat_reel);
        var chat_player = _chatPlayer;
        chat_player.ScriptQueue.Enqueue(_chatFrameB);
        var iterator = chat_player.GetDialogueIterator();
        iterator.MoveNext();
        chat_player.displayText.ShouldBeEmpty();
        chat_player.targetText.ShouldBeEmpty();
    }

    [Fact]
    public void GivenDialoguePlayer_AdvanceText_ShouldLoadNextFrameFromQueue_IfFramesRemain()
    {
        InitializeTestFrame(_chatFrameA);
        InitializeTestFrame(_chatFrameB);
        var chat_reel = CreateTwoFrameReel(_chatFrameA, _chatFrameB);
        ArrangeReadyState(_chatFrameA, chat_reel);
        var chat_player = _chatPlayer;
        chat_player.ScriptQueue.Enqueue(_chatFrameB);
        var iterator = chat_player.GetDialogueIterator();
        iterator.MoveNext();
        chat_player.CurrentFrame.ShouldBe(_chatFrameB);
    }

    [Fact]
    public void GivenDialoguePlayer_AdvanceText_ShouldCallLoadFrame_IfFramesRemain()
    {
        InitializeTestFrame(_chatFrameA);
        InitializeTestFrame(_chatFrameB);
        var chat_reel = CreateTwoFrameReel(_chatFrameA, _chatFrameB);
        ArrangeReadyState(_chatFrameA, chat_reel);
        var chat_player = _chatPlayer;
        chat_player.ScriptQueue.Enqueue(_chatFrameB);
        bool loading = false;
        chat_player.Loading += () =>
        {
            GD.Print("Loading event fired!");
            loading = true;
        };
        var iterator = chat_player.GetDialogueIterator();
        iterator.MoveNext();
        loading.ShouldBeTrue();
    }

    [Fact]
    public void GivenDialoguePlayer_AdvanceText_ShouldEndDialogue_IfNoMoreFrames()
    {
        InitializeTestFrame(_chatFrameA);
        InitializeTestFrame(_chatFrameB);
        var chat_reel = CreateTwoFrameReel(_chatFrameA, _chatFrameB);
        ArrangeReadyState(_chatFrameA, chat_reel);
        var chat_player = _chatPlayer;
        chat_player.ScriptQueue.Clear();
        bool ending = false;
        chat_player.Ending += () =>
        {
            GD.Print("ending event fired!");
            ending = true;
        };
        var iterator = chat_player.GetDialogueIterator();
        iterator.MoveNext();
        ending.ShouldBeTrue();
    }

    [Fact]
    public void GivenDialoguePlayer_EndDialogue_ShouldSetDialogueActiveFalse()
    {
        _chatPlayer.DialogueActive = true;
        var chat_player = _chatPlayer;

        chat_player.EndDialogue();

        chat_player.DialogueActive.ShouldBeFalse();
    }

    [Fact]
    public void GivenDialoguePlayer_EndDialogue_ShouldInitializeQueues()
    {
        InitializeTestFrame(_chatFrameA);
        InitializeTestFrame(_chatFrameB);

        var chat_reel = CreateTwoFrameReel(_chatFrameA, _chatFrameB);

        ArrangeReadyState(_chatFrameA, chat_reel);

        var chat_player = _chatPlayer;
        chat_player.ScriptQueue.Enqueue(_chatFrameB);

        chat_player.EndDialogue();

        chat_player.ScriptQueue.Count.ShouldBe(0);
    }

    [Fact]
    public void GivenDialoguePlayer_EndDialogue_ShouldSetCurrentModeNotReady()
    {
        _chatPlayer.DialogueActive = true;
        _chatPlayer.currentMode = DialoguePlayer.textAdvanceMode.ready;
        var chat_player = _chatPlayer;

        chat_player.EndDialogue();

        chat_player.currentMode.ShouldBe(DialoguePlayer.textAdvanceMode.not_ready);
    }
    
}
