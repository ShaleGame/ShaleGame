using System.Collections;

namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// Interface for handling visual novel-like dialogue scenes
/// </summary>

public interface IDialogueHandler
{
    public bool DialogueActive { get; set; }
    public bool DialogueVisible { get; set; }
    public DialogueReel CurrentReel { get; set; }
    public DialogueFrame CurrentFrame { get; set; }
    public Queue ScriptQueue { get; set; }
    public string targetText { get; set; }
    public string displayText { get; set; }

    public void StartDialogue(DialogueReel reel);
    public void LoadFrame(DialogueFrame frame);
    public void AdvanceText();
    public void Process();
    public void EndDialogue();
}
