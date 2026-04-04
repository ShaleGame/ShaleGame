using Godot;

namespace CrossedDimensions.Items;

[GlobalClass]
public partial class ItemData : Resource
{
    [Export]
    public string Name { get; set; }

    [Export]
    public string Description { get; set; }

    [Export]
    public Texture2D Icon { get; set; }
}
