using CrossedDimensions.Characters;
using CrossedDimensions.Entities.Bosses;
using CrossedDimensions.Saves;
using Godot;


namespace CrossedDimensions.Entities.BossSystem;

public partial class BossSystem : Node2D
{

	// Boss spawning
	[ExportGroup("Boss Spawning")]
	[Export] PackedScene BossScene;
	// If boss does not position itself, it will spawn at the position given here
	// If it does need a position to spawn at, make sure to attach the position node in the editor
	[Export] Node2D spawnPosition;
	Boss bossInstance;

	// Boss area trigger. Can be used to spawn the boss when the player enters, but will always center the camera in the middle of the area2D. Tho that is also optional.
	[Export] Area2D bossRoom;
	[Export] bool triggeredBossSpawn = true;
	[Export] bool centerCamera = true;
	bool isCameraCentered = false;

	Camera2D camera;

	// Signals for boss defeat and spawn
	[Signal] public delegate void BossSpawnedEventHandler();
	[Signal] public delegate void BossDefeatedEventHandler();
	
	bool bossDefeated = false; // This should be set based on the save system

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (bossRoom != null)
		{
			bossRoom.BodyEntered += OnBossRoomEntered;
			bossRoom.BodyExited += OnBossRoomExited;
		}

		camera = GetViewport().GetCamera2D();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (centerCamera && isCameraCentered && camera != null)
		{
			// Make sure bossRoom's collision is centered and bossroom is in the center of the room
			camera.GlobalPosition = bossRoom.GlobalPosition;
		}

	}

	private void OnBossRoomEntered(Node body)
	{
		if (body is Character && body.IsInGroup("Player"))
		{
			if (triggeredBossSpawn)
			{
				SpawnBoss();
			}

			if (centerCamera)
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

	public void SpawnBoss()
	{
		if (BossScene != null && !bossDefeated)
		{
			bossInstance = BossScene.Instantiate<Boss>();
			AddChild(bossInstance);

			if (spawnPosition != null)
			{
				bossInstance.Position = spawnPosition.Position;
			}

			bossInstance.Health.HealthChanged += (oldHealth) =>
			{
				if (!bossInstance.Health.IsAlive)
				{
					OnBossDefeated();
				}
			};

			EmitSignal(SignalName.BossSpawned);
		}
	}

	public void OnBossDefeated()
	{
		bossDefeated = true;
		EmitSignal(SignalName.BossDefeated);

		// Saves boss defeat state
		// Make sure boss node is named appropriately
		SaveManager.Instance.GetKeyOrDefault("bosses/" + bossInstance.bossName, false);

		isCameraCentered = false;
	}
}
