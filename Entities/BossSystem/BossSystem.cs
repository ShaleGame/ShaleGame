using CrossedDimensions.Characters;
using Godot;
using System;

namespace CrossedDimensions.Entities.BossSystem;

// Method for spawning boss
// Query the save system for boss check
// Only spawn if boss has not been defeated
// Bosses should use the character class
// Keep track of the current boss
// Emit signals when boss spawns and dies

public partial class BossSystem : Node2D
{

	// Boss spawning
	[Export] PackedScene BossScene;
	// If boss does not position itself, it will spawn at the position given here
	[Export] Node2D spawnPosition;
	Character bossInstance;
	
	bool bossDefeated = false; // This should be set based on the save system

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SpawnBoss()
	{
		if (BossScene != null && !bossDefeated)
		{
			bossInstance = BossScene.Instantiate<Character>();
			AddChild(bossInstance);

			if (spawnPosition != null)
			{
				bossInstance.Position = spawnPosition.Position;
			}
		}
	}
}
