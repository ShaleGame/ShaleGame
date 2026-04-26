using Godot;

namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// One step in a cutscene playback sequence.
/// </summary>
[GlobalClass]
public partial class CutsceneStep : Resource
{
    public enum StepKind
    {
        Animation = 0,
        Dialogue = 1
    }

    [Export]
    public StepKind Kind { get; set; } = StepKind.Animation;

    [Export]
    public string AnimationName { get; set; } = "";

    [Export]
    public DialogueReel DialogueReel { get; set; }
}
