using Godot;

namespace CrossedDimensions.Environment;

[GlobalClass]
public partial class AreaData : Resource
{
    [Export]
    public string Title { get; set; } = "";

    [Export]
    public string Subtitle { get; set; } = "";
}
