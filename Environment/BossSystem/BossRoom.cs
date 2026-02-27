using CrossedDimensions.Environment.BossSystem;
using CrossedDimensions.Characters;
using Godot;

namespace CrossedDimensions.Environment.BossSystem;

public partial class BossRoom : Node2D
{
    [Export] BossSystem bossSystem;

    // Boss area trigger. Can be used to spawn the boss when the player enters, but will always center the camera in the middle of the area2D. Tho that is also optional.
    [Export] Area2D bossRoomArea;
    [Export] bool triggeredBossSpawn = true;
    [Export] bool centerCamera = true;
    bool isCameraCentered = false;

    Camera2D camera;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (bossRoomArea != null)
        {
            bossRoomArea.BodyEntered += OnBossRoomEntered;
            bossRoomArea.BodyExited += OnBossRoomExited;
        }

        camera = GetViewport().GetCamera2D();

        bossSystem.BossDefeated += onBossDefeated;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (centerCamera && isCameraCentered && camera != null)
        {
            // Make sure bossRoom's collision is centered and bossroom is in the center of the room
            camera.GlobalPosition = bossRoomArea.GlobalPosition;
        }
    }

    private void OnBossRoomEntered(Node body)
    {
        GD.Print(body.Name + " entered boss room");

        if (body is Character && body.IsInGroup("Player"))
        {

            if (triggeredBossSpawn)
            {
                bossSystem.CallDeferred(nameof(bossSystem.SpawnBoss));
            }

            if (centerCamera && !bossSystem.bossDefeated)
            {
                isCameraCentered = true;
            }
        }
    }

    private void OnBossRoomExited(Node body)
    {
        if (body is Character && body.IsInGroup("Player"))
        {
            if (centerCamera)
            {
                isCameraCentered = false;
            }
        }
    }

    private void onBossDefeated()
    {
        isCameraCentered = false;
    }
}
