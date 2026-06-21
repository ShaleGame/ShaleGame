using Godot;

namespace CrossedDimensions.BoundingBoxes;

[GlobalClass]
public partial class MergePlayerArea : Area2D
{
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
        if (body is not Characters.Character character)
        {
            return;
        }

        if (!character.IsInGroup("Player"))
        {
            return;
        }

        character.Cloneable?.Merge();
    }
}
