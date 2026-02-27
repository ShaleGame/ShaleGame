using Godot;
using CrossedDimensions.Characters;
using CrossedDimensions.Environment.BossSystem;

public partial class BossSystemDoor : Node2D
{
    [Export] BossSystem bossSystem;

    // Make sure door area is off by default
    public CollisionShape2D doorArea;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        doorArea = GetNode<CollisionShape2D>("StaticBody2d/CollisionShape2D");

        bossSystem.BossSpawned += OnBossSpawned;
        bossSystem.BossDefeated += OnBossDefeated;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void OnBossSpawned()
    {
        GD.Print("Boss spawned, enabling door");

        doorArea.CallDeferred("set_disabled", false);
    }

    private void OnBossDefeated()
    {

        doorArea.CallDeferred("set_disabled", true);
    }
}
