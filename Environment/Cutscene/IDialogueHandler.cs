namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// Interface for handling visual novel-like dialogue scenes
/// </summary>

public interface IDialogueHandlerComponent
{
    bool DialogueActive { get; set; }
    bool DialogueVisible { get; set; }
    DialogueReel CurrentReel { get; set; }
    DialogueFrame CurrentFrame {}
    Queue ScriptQueue {}
    string targetText { get; set; }
    string displayText { get; set; }

    void StartDialogue( DialogueReel reel );
    void LoadFrame( DialogueFrame frame );
    void AdvanceText();
    void Process();
    void EndDialogue();
}