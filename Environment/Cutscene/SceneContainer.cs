using System;
using Godot;

namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// Scene container
/// Contains an array of dialogue reels and optional cutscene transition data.
/// </summary>

public partial class SceneContainer : Resource
{
    [Export]
    public DialogueReel[] Dialogue { get; set; } = Array.Empty<DialogueReel>();

    [Export]
    public CutsceneMetadata Cutscene { get; set; }

    [Signal]
    public delegate void TriggeredEventHandler();

    public virtual void Trigger()
    {
        GD.Print($"Scene start signal received");
        EmitSignal(SignalName.Triggered);
    }
}
