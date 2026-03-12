using Godot;

namespace CrossedDimensions.Environment.Triggers;

/// <summary>
/// Trigger that activates when a boss is defeated. Listens to a BossSystem
/// for boss spawn/defeat events.
/// </summary>
[GlobalClass]
public partial class BossTrigger : Trigger
{
    [Export]
    public BossSystem.BossSystem BossSystem { get; set; }

    /// <summary>
    /// If <c>true</c>, the trigger will deactivate when the boss spawns and
    /// reactivate when the boss is defeated. Otherwise, it will only activate
    /// when the boss is defeated but will not deactivate on spawn. This can be
    /// useful for triggers that should only care about boss defeat but not
    /// necessarily deactivate during the fight.
    /// </summary>
    [Export]
    public bool DeactivateOnBossSpawn { get; set; } = true;

    public override void _Ready()
    {
        base._Ready();

        if (BossSystem == null)
        {
            var err = $"BossTrigger '{Name}' requires a reference to a BossSystem.";
            throw new System.Exception(err);
        }

        BossSystem.BossSpawned += OnBossSpawned;
        BossSystem.BossDefeated += OnBossDefeated;

        if (BossSystem.IsNodeReady())
        {
            SetInitialState();
        }
        else
        {
            BossSystem.Ready += () =>
            {
                SetInitialState();
            };
        }
    }

    private void SetInitialState()
    {
        // set initial state based on whether the boss is already defeated
        if (DeactivateOnBossSpawn)
        {
            // always open on initial start
            SetActive(true, saveToFile: false);
        }
        else
        {
            SetActive(BossSystem.IsBossDefeated, saveToFile: false);
        }
    }

    private void OnBossSpawned()
    {
        SetActive(false);
    }

    private void OnBossDefeated()
    {
        SetActive(true);
    }
}
