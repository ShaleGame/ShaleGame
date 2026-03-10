using Godot;

namespace CrossedDimensions.BoundingBoxes;

[GlobalClass]
public partial class CameraBoundsTrigger : Area2D
{
    [Export]
    public Rect2 CameraBounds { get; set; }

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    public override void _ExitTree()
    {
        BodyEntered -= OnBodyEntered;
        BodyExited -= OnBodyExited;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is Characters.Character c)
        {
            if (c.IsInGroup("Player"))
            {
                var camera = GetViewport().GetCamera2D();
                if (camera != null)
                {
                    camera.LimitLeft = (int)CameraBounds.Position.X;
                    camera.LimitTop = (int)CameraBounds.Position.Y;
                    camera.LimitRight = (int)(CameraBounds.Position.X + CameraBounds.Size.X);
                    camera.LimitBottom = (int)(CameraBounds.Position.Y + CameraBounds.Size.Y);
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
                var camera = GetViewport().GetCamera2D();
                if (camera != null)
                {
                    camera.LimitLeft = int.MinValue;
                    camera.LimitTop = int.MinValue;
                    camera.LimitRight = int.MaxValue;
                    camera.LimitBottom = int.MaxValue;
                }
            }
        }
    }
}
