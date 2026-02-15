using Godot;

namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// A class for dialogue 'reels' that contain frames of dialogue
/// </summary>

public partial class DialogueReel : Resource
{
    [Export]
    public DialogueFrame[] Frames { get; set; }
}
