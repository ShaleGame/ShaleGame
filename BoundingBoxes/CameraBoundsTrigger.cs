using Godot;

namespace CrossedDimensions.BoundingBoxes;

[GlobalClass]
public partial class CameraBoundsTrigger : Area2D
{
    //[Export]
    //public Rect2 CameraBounds { get; set; }

    [Export]
    public Marker2D TopLeftMarker { get; set; }

    [Export]
    public Marker2D BottomRightMarker { get; set; }

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            BodyEntered -= OnBodyEntered;
            BodyExited -= OnBodyExited;
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
                    return;
                }

                var camera = GetViewport().GetCamera2D();
                if (camera != null)
                {
                    int left = (int)TopLeftMarker.GlobalPosition.X;
                    int top = (int)TopLeftMarker.GlobalPosition.Y;
                    int right = (int)BottomRightMarker.GlobalPosition.X;
                    int bottom = (int)BottomRightMarker.GlobalPosition.Y;

                    camera.LimitLeft = left;
                    camera.LimitTop = top;
                    camera.LimitRight = right;
                    camera.LimitBottom = bottom;
                }
            }
        }
    }

    private void OnBodyExited(Node body)
    {
        if (body is Characters.Character c)
        {
            if (c.IsInGroup("Player"))
            {
                if (c.Cloneable.IsClone)
                {
                    return;
                }

                var camera = GetViewport().GetCamera2D();
                if (camera != null)
                {
                    camera.LimitLeft = short.MinValue;
                    camera.LimitTop = short.MinValue;
                    camera.LimitRight = short.MaxValue;
                    camera.LimitBottom = short.MaxValue;
                }
            }
        }
    }
}
