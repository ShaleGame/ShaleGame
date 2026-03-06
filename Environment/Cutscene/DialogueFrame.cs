using System.Collections.Generic;
using Godot;

namespace CrossedDimensions.Environment.Cutscene;

public partial class DialogueFrame : Resource
{
    [Export]
    public string Speaker { get; set; }

    [Export(PropertyHint.MultilineText)]
    public string Text { get; set; }

    [Export]
    public Texture2D Background { get; set; }

    // Editor-friendly arrays (serialized in inspector)
    [Export]
    public Texture2D[] Portrait { get; set; }

    [Export]
    public Vector2[] PortraitPosition { get; set; }

    // Optional runtime lists for dynamic code use
    public List<Texture2D> RuntimePortraits => new List<Texture2D>(Portrait);
    public IReadOnlyList<Texture2D> RuntimePortraits => new List<Texture2D>(Portrait);
    public IReadOnlyList<Vector2> RuntimePositions => new List<Vector2>(PortraitPosition);
}
