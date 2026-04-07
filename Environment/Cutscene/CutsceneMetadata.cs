using Godot;

namespace CrossedDimensions.Environment.Cutscene;

[GlobalClass]
public partial class CutsceneMetadata : Resource
{
    [Export(PropertyHint.File, "*.tscn")]
    public string CutsceneScenePath { get; set; } = "";

    [Export]
    public bool RepositionPlayerOnReturn { get; set; } = false;

    [Export]
    public Vector2 ReturnPlayerPosition { get; set; } = Vector2.Zero;
}
