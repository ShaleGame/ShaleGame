using Godot;
using System.Text.Json.Serialization;

namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// A class for individual 'frames' of dialogue
/// </summary>

public class DialogueFrame
{
    [JsonPropertyName("speaker")]
    public string Speaker { get; set; }
    
    [JsonPropertyName("text")]
    public string Text { get; set; }
    
    [JsonPropertyName("portrait")]
    public Sprite2D[] Portrait { get; set; }
    
    [JsonPropertyName("portraitPosition")]
    public Vector2[] PortraitPosition { get; set; }
    
    [JsonPropertyName("background")]
    public Sprite2D Background { get; set; }
}
