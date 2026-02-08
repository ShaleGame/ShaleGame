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
    [Export(PropertyHint.File, "*.png,")]
    public string[] Portrait { get; set; }      //cannot include Node members in resource
    [Export]
    public Vector2[] PortraitPosition { get; set; }
    [Export(PropertyHint.File, "*.png,")]
    public string Background { get; set; }      //cannot include Node members in resource
}
