using System.Text.Json.Serialization;

namespace CrossedDimensions.Environment.Cutscene;

[JsonSerializable(typeof(DialogueReel))]
internal partial class DialogueReelJsonContext : JsonSerializerContext
{
}
