using System.Text.Json.Serialization;

namespace CrossedDimensions.Environment.Cutscene;

[JsonSerializable(typeof(DialogueFrame))]
internal partial class DialogueFrameJsonContext : JsonSerializerContext
{
}
