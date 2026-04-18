using Godot;
using CrossedDimensions.Saves;

namespace CrossedDimensions.BoundingBoxes;

[GlobalClass]
public partial class SceneTransitionTrigger : Area2D
{
    [Export(PropertyHint.File, "*.tscn")]
    public string TargetScenePath { get; set; } = "";

    [Export]
    public string TargetMarkerName { get; set; } = "";

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            BodyEntered -= OnBodyEntered;
        }
    }

    private void OnBodyEntered(Node body)
    {
        if (body is Characters.Character c)
        {
            if (c.IsInGroup("Player"))
            {
                if (c.Cloneable.IsClone)
                {
                    c.Cloneable.Merge();
                }
                else
                {
                    SceneManager.Instance.CallDeferred(
                        "LoadSceneWithMarker",
                        TargetScenePath,
                        TargetMarkerName,
                        true);
                }
            }
        }
    }
}
