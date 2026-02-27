using CrossedDimensions.Characters;
using CrossedDimensions.Characters.Controllers;
using CrossedDimensions.Components;
using CrossedDimensions.Entities.Enemies;
using CrossedDimensions.Saves;
using Godot;


namespace CrossedDimensions.Environment.BossSystem;

public partial class BossSystem : Node2D
{

    // Boss spawning
    [Export] PackedScene BossScene;
    // If boss does not position itself, it will spawn at the position given here
    // If it does need a position to spawn at, make sure to attach the position node in the editor
    [Export] Node2D spawnPosition;
    Character bossInstance;

    // Signals for boss defeat and spawn
    [Signal] public delegate void BossSpawnedEventHandler();
    [Signal] public delegate void BossDefeatedEventHandler();

    public bool bossDefeated = false; // This should be set based on the save system

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

		CharacterController controller = bossInstance.Controller;
		string name = "null";
		if (controller != null && controller is EnemyController enemyComponent)
		{
			name = enemyComponent.EnemyComponent.EnemyName;
		}

        // Saves boss defeat state
        // Make sure boss node is named appropriately
        SaveManager.Instance.GetKeyOrDefault("bosses/" + name, false);

    }
}
