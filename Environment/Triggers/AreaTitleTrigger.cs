using CrossedDimensions.Environment;
using Godot;

namespace CrossedDimensions.Environment.Triggers;

[GlobalClass]
public partial class AreaTitleTrigger : Area2D
{
    [Export]
    public AreaData AreaData { get; set; }

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

        if (character.Cloneable?.IsClone ?? true)
        {
            return;
        }

        if (AreaData is null)
        {
            GD.PushWarning($"AreaTitleTrigger '{Name}' does not have AreaData assigned.");
            return;
        }

        AreaManager.Instance?.NotifyAreaTitleTriggerEntered(AreaData);
    }
}
