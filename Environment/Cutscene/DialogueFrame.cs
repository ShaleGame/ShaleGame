using Godot;
using System.Text.Json.Serialization;

namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// A class for individual 'frames' of dialogue
/// </summary>

public partial class DialogueFrame : Resource
{
    public string Speaker { get; set; }
    public string Text { get; set; }
    public Sprite2D[] Portrait { get; set; }
    public Vector2[] PortraitPosition { get; set; }
    public Sprite2D Background { get; set; }
}
