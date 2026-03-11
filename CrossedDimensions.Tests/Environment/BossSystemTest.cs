using CrossedDimensions.Characters;
using CrossedDimensions.Environment.BossSystem;
using Godot;
using CrossedDimensions.Components;

namespace CrossedDimensions.Tests.Environment;

[Collection("GodotHeadless")]
public class BossSystemTest
{
    private readonly GodotHeadlessFixedFpsFixture _godot;
    private readonly BossSystem _bossSystem;

    public BossSystemTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;
        _bossSystem = new BossSystem();
        _bossSystem.SaveManager = new CrossedDimensions.Saves.SaveManager();
        _bossSystem.SaveManager.CurrentSave = new CrossedDimensions.Saves.SaveFile();
    }

    [Fact]
    public void SpawnBoss_IfNull_NoSpawn()
    {
        bool signalFired = false;
        _bossSystem.BossSpawned += () => signalFired = true;

        _bossSystem.SpawnBoss();

        Assert.False(signalFired);

    }

    [Fact]
    public void SpawnBoss_IfDefeated_NoSpawn()
    {
        _bossSystem.SaveManager.SetKey(_bossSystem.BossKey, true);

        bool signalFired = false;
        _bossSystem.BossSpawned += () => signalFired = true;

        _bossSystem.SpawnBoss();

        Assert.False(signalFired);
    }

    [Fact]
    public void SpawnBoss_IfNotNullOrDefeated_IsSpawned()
    {
        // Checks if boss is instantiated and is a child of BossSystem
        _bossSystem.BossScene = GD.Load<PackedScene>("res://Entities/Bosses/TestBoss/TestBoss.tscn");

        _bossSystem.SpawnBoss();

        _bossSystem.GetChildren().Count.ShouldBe(1);
        _bossSystem.GetChild<Character>(0).ShouldNotBeNull();
    }

    [Fact]
    public void SpawnBoss_IfPosition_SpawnAtPosition()
    {
        // Checks if boss is instantiated in the correct spawn position
        _bossSystem.BossScene = GD.Load<PackedScene>("res://Entities/Bosses/TestBoss/TestBoss.tscn");

        Node2D spawnPos = new Node2D();
        spawnPos.GlobalPosition = new Vector2(10, 10);

        _bossSystem.SpawnPosition = spawnPos;

        _bossSystem.SpawnBoss();

        _bossSystem.GetChild<Character>(0).GlobalPosition.X.ShouldBe(10);
        _bossSystem.GetChild<Character>(0).GlobalPosition.Y.ShouldBe(10);
    }

    [Fact]
    public void SpawnBoss_IfPositionNull_SpawnAtZero()
    {
        // Checks if boss is instantiated in the global position of the boss system if spawn position is null
        _bossSystem.BossScene = GD.Load<PackedScene>("res://Entities/Bosses/TestBoss/TestBoss.tscn");

        _bossSystem.SpawnBoss();

        _bossSystem.GetChild<Character>(0).GlobalPosition.X.ShouldBe(_bossSystem.GlobalPosition.X);
        _bossSystem.GetChild<Character>(0).GlobalPosition.Y.ShouldBe(_bossSystem.GlobalPosition.Y);
    }

    [Fact]
    public void SpawnBoss_IfSpawned_SendSpawnSignal()
    {
        // checks if boss doesnt send spawned signal if boss has been defeated already
        _bossSystem.BossScene = GD.Load<PackedScene>("res://Entities/Bosses/TestBoss/TestBoss.tscn");

        bool signalFired = false;
        _bossSystem.BossSpawned += () => signalFired = true;

        _bossSystem.SpawnBoss();

        Assert.True(signalFired);
    }

    [Fact]
    public void BossDefeated_IfDefeated_SetDefeatedVar()
    {
        // checks if boss defeated var is set upon defeat
        _bossSystem.BossScene = GD.Load<PackedScene>("res://Entities/Bosses/TestBoss/TestBoss.tscn");

        _bossSystem.SpawnBoss();

        _bossSystem.OnBossDefeated();

        Assert.True(_bossSystem.IsBossDefeated);
    }

    [Fact]
    public void BossDefeated_IfDefeated_SendDefeatedSignal()
    {
        // checks if boss defeated signal is emitted upon defeat
        _bossSystem.BossScene = GD.Load<PackedScene>("res://Entities/Bosses/TestBoss/TestBoss.tscn");

        _bossSystem.SpawnBoss();

        bool signalFired = false;
        _bossSystem.BossDefeated += () => signalFired = true;

        _bossSystem.OnBossDefeated();

        Assert.True(signalFired);
    }

    [Fact]
    public void BossDefeated_IfBossDies_TriggerDefeatedFunction()
    {
        // checks if boss triggers BossDefeated function when it dies
        _bossSystem.BossScene = GD.Load<PackedScene>("res://Entities/Bosses/TestBoss/TestBoss.tscn");

        _bossSystem.SpawnBoss();

        bool signalFired = false;
        _bossSystem.BossDefeated += () => signalFired = true;

        _bossSystem.BossInstance.Health.CurrentHealth = 0;

        Assert.True(signalFired);
    }
}
