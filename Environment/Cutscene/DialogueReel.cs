using Godot;

namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// A class for dialogue 'reels' that contain frames of dialogue
/// </summary>
[GlobalClass]
public partial class DialogueReel : Resource
{
    [Export]
    public DialogueFrame[] Frames { get; set; }
    public int frameIndex { get; set; } = 0;
}
