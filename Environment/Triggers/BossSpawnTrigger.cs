using Godot;

namespace CrossedDimensions.Environment.Triggers;

/// <summary>
/// Trigger that activates when a boss is defeated. Listens to a BossSystem
/// for boss spawn/defeat events.
/// </summary>
[GlobalClass]
public partial class BossSpawnTrigger : Trigger
{
    [Export]
    public BossSystem.BossSystem BossSystem { get; set; }

    public override void _Ready()
    {
        base._Ready();

        if (BossSystem == null)
        {
            var err = $"BossSpawnTrigger '{Name}' requires a reference to a BossSystem.";
            throw new System.Exception(err);
        }

        BossSystem.BossSpawned += OnBossSpawned;
        BossSystem.BossDefeated += OnBossDefeated;

        SetActive(false, saveToFile: false);
    }

    private void OnBossSpawned()
    {
        SetActive(true);
    }

    private void OnBossDefeated()
    {
        SetActive(false);
    }
}
