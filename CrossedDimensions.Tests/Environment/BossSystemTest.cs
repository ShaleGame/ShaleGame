using CrossedDimensions.BoundingBoxes;
using CrossedDimensions.Characters;
using CrossedDimensions.Saves;
using CrossedDimensions.Environment.BossSystem;
using Godot;
using CrossedDimensions.Entities.Enemies;
using CrossedDimensions.Characters.Controllers;
using Castle.Components.DictionaryAdapter.Xml;
using CrossedDimensions.Components;

namespace CrossedDimensions.Tests.Environment;

[Collection("GodotHeadless")]
public class BossSystemTest(GodotHeadlessFixedFpsFixture godot)
{
    [Fact]
    public void SpawnBoss_IfNull_NoSpawn()
    {
        // checks if boss  doesnt send spawned signal if boss scene is null
        var bossSystem = new BossSystem();

        bool signalFired = false;
        bossSystem.BossSpawned += () => signalFired = true;

        bossSystem.SpawnBoss();

        Assert.False(signalFired);

    }

    [Fact]
    public void SpawnBoss_IfDefeated_NoSpawn()
    {
        // checks if boss  doesnt send spawned signal if boss has been defeated already
        var bossSystem = new BossSystem();
        bossSystem.bossDefeated = true;

        bool signalFired = false;
        bossSystem.BossSpawned += () => signalFired = true;

        bossSystem.SpawnBoss();

        Assert.False(signalFired);
    }

    [Fact]
    public void SpawnBoss_IfNotNullOrDefeated_IsSpawned()
    {
        // Checks if boss is instantiated and is a child of BossSystem
        var bossSystem = new BossSystem();
        bossSystem.BossScene = GD.Load<PackedScene>("res://Entities/Bosses/TestBoss/TestBoss.tscn");

        bossSystem.SpawnBoss();

        bossSystem.GetChildren().Count.ShouldBe(1);
        bossSystem.GetChild<Character>(0).ShouldNotBeNull();
    }

    [Fact]
    public void SpawnBoss_IfPosition_SpawnAtPosition()
    {
        // Checks if boss is instantiated in the correct spawn position
        var bossSystem = new BossSystem();
        bossSystem.BossScene = GD.Load<PackedScene>("res://Entities/Bosses/TestBoss/TestBoss.tscn");
        
        Node2D spawnPos = new Node2D();
        spawnPos.GlobalPosition = new Vector2(10, 10);

        bossSystem.spawnPosition = spawnPos;

        bossSystem.SpawnBoss();

        bossSystem.GetChild<Character>(0).GlobalPosition.X.ShouldBe(10);
        bossSystem.GetChild<Character>(0).GlobalPosition.Y.ShouldBe(10);
    }

    [Fact]
    public void SpawnBoss_IfPositionNull_SpawnAtZero()
    {
        // Checks if boss is instantiated in the global position of the boss system if spawn position is null
        var bossSystem = new BossSystem();
        bossSystem.BossScene = GD.Load<PackedScene>("res://Entities/Bosses/TestBoss/TestBoss.tscn");

        bossSystem.SpawnBoss();

        bossSystem.GetChild<Character>(0).GlobalPosition.X.ShouldBe(bossSystem.GlobalPosition.X);
        bossSystem.GetChild<Character>(0).GlobalPosition.Y.ShouldBe(bossSystem.GlobalPosition.Y);
    }

    [Fact]
    public void SpawnBoss_IfSpawned_SendSpawnSignal()
    {
        // checks if boss doesnt send spawned signal if boss has been defeated already
        var bossSystem = new BossSystem();
        bossSystem.BossScene = GD.Load<PackedScene>("res://Entities/Bosses/TestBoss/TestBoss.tscn");

        bool signalFired = false;
        bossSystem.BossSpawned += () => signalFired = true;

        bossSystem.SpawnBoss();

        Assert.True(signalFired);
    }

    [Fact]
    public void BossDefeated_IfDefeated_SetDefeatedVar()
    {
        // checks if boss defeated var is set upon defeat
        var bossSystem = new BossSystem();
        bossSystem.BossScene = GD.Load<PackedScene>("res://Entities/Bosses/TestBoss/TestBoss.tscn");

        bossSystem.SpawnBoss();

        bossSystem.OnBossDefeated();

        Assert.True(bossSystem.bossDefeated);
    }

    [Fact]
    public void BossDefeated_IfDefeated_SendDefeatedSignal()
    {
        // checks if boss defeated signal is emitted upon defeat
        var bossSystem = new BossSystem();
        bossSystem.BossScene = GD.Load<PackedScene>("res://Entities/Bosses/TestBoss/TestBoss.tscn");

        bossSystem.SpawnBoss();

        bool signalFired = false;
        bossSystem.BossDefeated += () => signalFired = true;

        bossSystem.OnBossDefeated();

        Assert.True(signalFired);
    }

    [Fact]
    public void BossDefeated_IfBossDies_TriggerDefeatedFunction()
    {
        // checks if boss triggers BossDefeated function when it dies
        var bossSystem = new BossSystem();
        bossSystem.BossScene = GD.Load<PackedScene>("res://Entities/Bosses/TestBoss/TestBoss.tscn");

        bossSystem.SpawnBoss();

        bool signalFired = false;
        bossSystem.BossDefeated += () => signalFired = true;

        bossSystem.bossInstance.Health.CurrentHealth = 0;

        Assert.True(signalFired);
    }
}