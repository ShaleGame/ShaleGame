using Godot;
using CrossedDimensions.Characters;
using CrossedDimensions.Entities.BossSystem;

public partial class BossSystemDoor : Node2D
{
	[Export] BossSystem bossSystem;

	// Make sure door area is off by default
	public Area2D doorArea;

	public bool closed = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		doorArea = GetNode<Area2D>("Area2D");

		bossSystem.BossSpawned += OnBossSpawned;
		bossSystem.BossDefeated += OnBossDefeated;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnBossSpawned()
	{
		closed = true;
		doorArea.Monitoring = true;
	}

	private void OnBossDefeated()
	{
		closed = false;
		doorArea.Monitoring = false;
	}
}
