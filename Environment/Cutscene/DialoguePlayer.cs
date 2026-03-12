using System.Collections.Generic;
using System.Runtime.Versioning;
using Godot;

namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// Interface for handling visual novel-like dialogue scenes
/// </summary>
[GlobalClass]
public partial class DialoguePlayer : Node, IDialogueHandler
{
    public bool DialogueActive { get; set; } = false;
    public bool DialogueVisible { get; set; } = false;
    public DialogueReel CurrentReel { get; set; }
    public DialogueFrame CurrentFrame { get; set; }
    public Queue<DialogueFrame> ScriptQueue { get; set; } = new();
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
    public int targetTextIndex { get; private set; } = 0;

    [Signal]
    public delegate void AdvancingEventHandler();
    [Signal]
    public delegate void LoadingEventHandler();
    [Signal]
    public delegate void EndingEventHandler();
    public void StartDialogue(DialogueReel reel)
    {
        ScriptQueue.Clear();
        CurrentReel = reel;
        currentMode = textAdvanceMode.loading;
        DialogueVisible = true;
        DialogueActive = true;
        for (var i = 0; i < CurrentReel.Frames.Length; i++)
        {
            ScriptQueue.Enqueue(CurrentReel.Frames[i]);
        }
        //LoadFrame((DialogueFrame)ScriptQueue.Dequeue());
    }
    public void LoadFrame(DialogueFrame frame)
    {
        GD.Print("Loading frame", frame);
        CurrentFrame = frame;
        targetText = CurrentFrame.Text;
        displayText = "";
        currentMode = textAdvanceMode.printing;
        Load();
    }
    public bool AdvanceText()
    {
        DialogueFrame targetFrame = null;
        Advance();
        if (ScriptQueue.TryDequeue(out targetFrame))
        {
            currentMode = textAdvanceMode.loading;
            LoadFrame(targetFrame);
            return true;
        }
        else
        {
            EndDialogue();
            return false;
        }
    }
    public override void _Process(double delta)
    {
        /*
        if (currentMode == textAdvanceMode.printing)
        {
            GetDialogueIterator().MoveNext();
        }
        */
    }

    public void EndDialogue()
    {
        ScriptQueue.Clear();
        CurrentReel = null;
        CurrentFrame = null;
        currentMode = textAdvanceMode.not_ready;
        DialogueActive = false;
        DialogueVisible = false;
        End();
    }

    public IEnumerator<string> GetDialogueIterator()
    {
        while (DialogueActive)
        {
            if (currentMode == textAdvanceMode.printing)
            {
                if (displayText != targetText)
                {
                    displayText += targetText[targetTextIndex];
                    targetTextIndex++;
                    if (targetText == displayText)
                    {
                        currentMode = textAdvanceMode.ready;
                    }
                    yield return displayText;
                }
            }
            else if (currentMode == textAdvanceMode.ready)
            {
                AdvanceText();
                yield return null;
            }
            else
            {
                yield return null;
            }
        }
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
