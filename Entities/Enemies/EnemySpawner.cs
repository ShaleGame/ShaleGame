using Godot;

namespace CrossedDimensions.Entities.Enemies;

public partial class EnemySpawner : Node2D
{
    private bool _nearPlayer;
    private bool _enemySpawned;

    [Export]
    public int SpawnRange { get; set; } = 1000;

    [Export]
    public PackedScene EnemyScene { get; set; }

    [Export]
    public Timer RespawnTimer { get; set; }

    private Node2D _player;
    private Characters.Character _enemy;

    public override void _Ready()
    {
        _player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
        RespawnTimer.Timeout += OnTimerTimeout;
    }

    public void OnTimerTimeout()
    {
        _enemySpawned = false;
    }

    public override void _Process(double delta)
    {
        if (_player is null)
        {
            return;
        }

        var distance = GlobalPosition.DistanceTo(_player.GlobalPosition);
        _nearPlayer = distance < SpawnRange;

        if (_nearPlayer && !_enemySpawned)
        {
            SpawnEnemy();
        }
        else if (!_nearPlayer)
        {
            DespawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (EnemyScene is null)
        {
            return;
        }

        _enemy = EnemyScene.Instantiate<Characters.Character>();
        // TODO: consider adding a specific parent node to add characters
        var targetParent = GetParent()?.GetParent() ?? GetParent();
        targetParent?.AddChild(_enemy);

        if (_enemy is Node2D enemyNode)
        {
            enemyNode.GlobalPosition = GlobalPosition;
        }

        _enemy.Health.HealthChanged += (int oldHealth) =>
        {
            if (_enemy.Health.CurrentHealth <= 0)
            {
                StartSpawnTimer();
            }
        };

        _enemySpawned = true;
    }

    private void DespawnEnemy()
    {
        if (_enemy is null)
        {
            return;
        }

        _enemySpawned = false;
        _enemy.QueueFree();
        _enemy = null;
    }

    private void StartSpawnTimer()
    {
        RespawnTimer?.Start();
    }
}
