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
    [Export]
    public PackedScene BossScene { get; set; }
    // If boss does not position itself, it will spawn at the position given here
    // If it does need a position to spawn at, make sure to attach the position node in the editor
    [Export]
    public Node2D SpawnPosition { get; set; }

    [Export]
    public Character BossInstance { get; set; }

    // Signals for boss defeat and spawn
    [Signal]
    public delegate void BossSpawnedEventHandler();

    [Signal]
    public delegate void BossDefeatedEventHandler();

    [Export]
    public string BossKey { get; set; }

    public SaveManager SaveManager { get; set; }

    public bool IsBossDefeated => SaveManager.GetKeyOrDefault(BossKey, false);

    public override void _Ready()
    {
        SaveManager ??= SaveManager.Instance;
    }

    public void SpawnBoss()
    {
        if (BossScene != null && !IsBossDefeated)
        {
            BossInstance = BossScene.Instantiate<Character>();
            AddChild(BossInstance);

            if (SpawnPosition != null)
            {
                BossInstance.Position = SpawnPosition.Position;
            }

            BossInstance.Health.HealthChanged += (oldHealth) =>
            {
                if (!BossInstance.Health.IsAlive)
                {
                    OnBossDefeated();
                }
            };

            EmitSignal(SignalName.BossSpawned);
        }
    }

    public void OnBossDefeated()
    {
        EmitSignal(SignalName.BossDefeated);

        // Saves boss defeat state
        // Make sure boss node is named appropriately
        SaveManager.SetKey(BossKey, true);
    }
}
