using Godot;

namespace CrossedDimensions.Environment.Cutscene;

[GlobalClass]
public partial class CutsceneScene : Node2D
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    [Export]
    public string StartAnimation { get; set; } = "";

    public bool IsStarted { get; set; }

    public bool IsFinished { get; set; }
}
