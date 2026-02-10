using Godot;
using System.Collections;

namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// Scene container
/// Contains an array of dialogue reels and a timeline
/// </summary>

public partial class SceneContainer : Resource
{
    [Export]
    public DialogueReel[] Dialogue { get; set; }
    [Export]
    public ActionTimeline Timeline { get; set; }
    [Signal]
    public delegate void TriggerEventHandler();
    public virtual void Trigger()
    {
        GD.Print($"Scene start signal received");
        EmitSignal(SignalName.Trigger);
    }
}