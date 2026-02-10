using Godot;

namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// A class for individual 'frames' of dialogue
/// </summary>

public partial class DialogueFrame : Resource
{
    [Export]
    public string Speaker { get; set; }
    [Export(PropertyHint.MultilineText)]
    public string Text { get; set; }
    [Export]
    public Texture2D[] Portrait { get; set; }
    [Export]
    public Vector2[] PortraitPosition { get; set; }
    [Export]
    public Texture2D Background { get; set; }
}
