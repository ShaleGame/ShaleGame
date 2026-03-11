using CrossedDimensions.Environment.BossSystem;
using CrossedDimensions.Characters;
using Godot;

namespace CrossedDimensions.Environment.BossSystem;

public partial class BossRoom : Node2D
{
    [Export] BossSystem bossSystem;

    // Boss area trigger. Can be used to spawn the boss when the player enters.
    // Camera centering is now handled by two Marker2D nodes that define
    // the camera bounds (top-left and bottom-right). When the player is
    // inside the boss room we clamp the camera to that rectangle.
    [Export]
    private Area2D _bossRoomArea;

    [Export]
    public bool TriggerBossSpawn { get; set; }= true;

    [ExportGroup("Camera Bounds")]
    [Export]
    public Marker2D TopLeft { get; set; }

    [Export]
    public Marker2D BottomRight { get; set; }

    private bool _cameraBoundsActive = false;

    private Camera2D _camera;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (_bossRoomArea != null)
        {
            _bossRoomArea.BodyEntered += OnBossRoomEntered;
            _bossRoomArea.BodyExited += OnBossRoomExited;
        }

        _camera = GetViewport().GetCamera2D();

        bossSystem.BossDefeated += OnBossDefeated;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (_cameraBoundsActive && _camera != null && TopLeft != null && BottomRight != null)
        {
            // Clamp the camera position to the rectangle defined by the two markers.
            var min = TopLeft.GlobalPosition;
            var max = BottomRight.GlobalPosition;
        }
    }

    private void OnBossRoomEntered(Node body)
    {
        GD.Print(body.Name + " entered boss room");

        if (body is Character c && body.IsInGroup("Player"))
        {
            if (c.Cloneable.IsClone)
            {
                // most likely the clone entered the boss room, so we don't
                // want to trigger the boss spawn again or center the camera
                // again
                return;
            }

            if (TriggerBossSpawn)
            {
                bossSystem.CallDeferred(nameof(bossSystem.SpawnBoss));
            }

            BindCamera();
        }
    }

    private void OnBossRoomExited(Node body)
    {
        if (body is Character c && body.IsInGroup("Player"))
        {
            if (c.Cloneable.IsClone)
            {
                return;
            }

            ReleaseCamera();
        }
    }

    private void OnBossDefeated()
    {
        ReleaseCamera();
    }

    private void BindCamera()
    {
        _camera.LimitLeft = (int)TopLeft.GlobalPosition.X;
        _camera.LimitTop = (int)TopLeft.GlobalPosition.Y;
        _camera.LimitRight = (int)BottomRight.GlobalPosition.X;
        _camera.LimitBottom = (int)BottomRight.GlobalPosition.Y;
    }

    private void ReleaseCamera()
    {
        _camera.LimitLeft = short.MinValue;
        _camera.LimitTop = short.MinValue;
        _camera.LimitRight = short.MaxValue;
        _camera.LimitBottom = short.MaxValue;
    }
}
