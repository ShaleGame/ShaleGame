using System.Collections;
using Castle.Components.DictionaryAdapter.Xml;

namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// Interface for handling visual novel-like dialogue scenes
/// </summary>

public partial class DialoguePlayer : IDialogueHandler
{
    public bool DialogueActive { get; set; } = false;
    public bool DialogueVisible { get; set; } = false;
    public DialogueReel CurrentReel { get; set; }
    public DialogueFrame CurrentFrame { get; set; }
    public Queue ScriptQueue { get; set; }
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

    public void StartDialogue(DialogueReel reel)
    {

    }
    public void LoadFrame(DialogueFrame frame)
    {

    }
    public void AdvanceText()
    {

    }
    public void Process()
    {

    }
    public void EndDialogue()
    {

    }
    
}
