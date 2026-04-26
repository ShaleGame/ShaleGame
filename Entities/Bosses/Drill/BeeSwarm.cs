using Godot;
using Godot.Collections;
using System;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.Drill;

/// <summary>
/// Drill goes all the way up into the ceiling and causing a bunch of bees to start coming out of the walls. Camera shakes before bees spawn.
/// </summary>

public partial class BeeSwarm : State
{

    [Export] public float DrillTweenDuration { get; set; } = 1.5f;
    [Export] public PackedScene BeeScene { get; set; }
    [Export] public int Minbees { get; set; } = 2;
    [Export] public int MaxBees { get; set; } = 4;
    [Export] public float CameraShakeDuration { get; set; } = 1f;
    [Export] public float SpawnDelay { get; set; } = 1f;
    [Export] public StateMachine AttackStateMachine { get; set; }
    [Export] public State AttackIdleState { get; set; }

    private Character _drill;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private Tween _tween;
    private double _curTime = 0.0;
    private bool _shook = false;
    private Vector2 _start;
    private bool _spawned = false;

    // External nodes
    private Node2D _holder;
    private Node2D _ceilingPosition;
    private Array<Node2D> _leftSpawnPoints;
    private Array<Node2D> _rightSpawnPoints;

    public override State Enter(State previousState)
    {
        _drill = Context as Character;
        _curTime = 0.0;
        _shook = false;
        _spawned = false;

        _start = _drill.GlobalPosition;

        // Get external nodes
        _holder = GetTree().GetFirstNodeInGroup("DrillBossHolder") as Node2D;

        _ceilingPosition = _holder.GetNodeOrNull<Node2D>("CeilingPosition");

        _leftSpawnPoints = GetChildrenAsNode2D(_holder.FindChild("LeftSpawnPoints"));
        _rightSpawnPoints = GetChildrenAsNode2D(_holder.FindChild("RightSpawnPoints"));

        if (_drill != null && _ceilingPosition != null)
        {
            _tween = _drill.CreateTween();
            _tween.TweenProperty(_drill, "global_position", _ceilingPosition.GlobalPosition, DrillTweenDuration)
                  .SetTrans(Tween.TransitionType.Sine)
                  .SetEase(Tween.EaseType.InOut);
        }

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (_drill == null || (_tween != null && _tween.IsRunning()))
        {
            return base.Process(delta);
        }

        _curTime += delta;

        if (!_shook)
        {
            _shook = true;
            // Shake camera
        }

        if (!_spawned && _curTime >= SpawnDelay)
        {
            _spawned = true;
            SpawnBees(_leftSpawnPoints);
            SpawnBees(_rightSpawnPoints);
            AttackStateMachine?.ChangeState(AttackIdleState);
        }

        return base.Process(delta);
    }

    public override void Exit(State nextState)
    {
        _tween = _drill.CreateTween();
        _tween.TweenProperty(_drill, "global_position", _start, DrillTweenDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        base.Exit(nextState);
    }

    private void SpawnBees(Array<Node2D> spawnPoints)
    {
        if (BeeScene == null || spawnPoints.Count == 0)
        {
            return;
        }


        for (int i = 0; i < spawnPoints.Count; i++)
        {
            var spawnPoint = spawnPoints[i];
            int amount = _rng.RandiRange(Minbees, MaxBees);

            for (int j = 0; j < amount; j++)
            {
                var bee = BeeScene.Instantiate() as Character;
                _drill.GetParent().AddChild(bee);
                bee.GlobalPosition = spawnPoint.GlobalPosition;
            }
        }
    }

    private Array<Node2D> GetChildrenAsNode2D(Node parent)
    {
        var result = new Array<Node2D>();
        foreach (var child in parent.GetChildren())
        {
            result.Add((Node2D)child);
        }
        return result;
    }

}
