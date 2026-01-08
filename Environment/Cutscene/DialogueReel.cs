using Godot;
using System.Text.Json.Serialization;

namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// Class that contains a reel of DialogueFrames
/// </summary>

public partial class DialogueReel : Node
{
    [JsonPropertyName("sceneName")]
    string SceneName { get; }

    [JsonPropertyName("frame")]
    DialogueFrame[] Frame { get; }
}