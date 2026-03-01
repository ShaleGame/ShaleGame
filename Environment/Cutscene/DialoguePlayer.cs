using System.Collections;
using Godot;

namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// Interface for handling visual novel-like dialogue scenes
/// </summary>

public partial class DialoguePlayer : Node, IDialogueHandler
{
    public bool DialogueActive { get; set; } = false;
    public bool DialogueVisible { get; set; } = false;
    public DialogueReel CurrentReel { get; set; }
    public DialogueFrame CurrentFrame { get; set; }
    public Queue ScriptQueue { get; set; } = new Queue();
    public string targetText { get; set; } = "";
    public string displayText { get; set; } = "";
    public enum textAdvanceMode
    {
        not_ready = -1,
        loading = 0,
        printing = 1,
        ready = 2
    }
    public textAdvanceMode currentMode { get; set; } = textAdvanceMode.not_ready;

    [Signal]
    public delegate void AdvancingEventHandler();
    [Signal]
    public delegate void LoadingEventHandler();
    [Signal]
    public delegate void EndingEventHandler();
    public void StartDialogue(DialogueReel reel)
    {
        CurrentReel = reel;
        currentMode = textAdvanceMode.loading;
        DialogueVisible = true;
        DialogueActive = true;
        for (var i = 0; i < CurrentReel.Frames.Length; i++)
        {
            ScriptQueue.Enqueue(CurrentReel.Frames[i]);
        }
        LoadFrame((DialogueFrame)ScriptQueue.Dequeue());
    }
    public void LoadFrame(DialogueFrame frame)
    {
        CurrentFrame = frame;
        targetText = CurrentFrame.Text;
        displayText = "";
        currentMode = textAdvanceMode.printing;
        Load();
    }
    public void AdvanceText()
    {

        Advance();
    }
    public override void _Process(double delta)
    {
        if (currentMode == textAdvanceMode.printing && displayText != targetText)
        {
            
        } 
    }

    public void EndDialogue()
    {
        
        ScriptQueue.Clear();
        CurrentReel = null;
        CurrentFrame = null;
        currentMode = textAdvanceMode.not_ready;
        DialogueActive = false;
        End();
    }

    public IEnumerator GetDialogueIterator()
    {

        yield return null;
    }

    public virtual void Advance()
    {
        GD.Print($"Page advance detected");
        EmitSignal(SignalName.Advancing);
    }

    public virtual void Load()
    {
        GD.Print($"Load next frame detected!");
        EmitSignal(SignalName.Loading);
    }
    public virtual void End()
    {
        GD.Print($"End dialogue detected");
        EmitSignal(SignalName.Ending);
    }

}
