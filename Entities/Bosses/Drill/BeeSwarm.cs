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
    
    [Export] public Node2D CeilingPosition {get; set;}
    [Export] public float DrillTweenDuration {get; set;} = 1.5f;
    [Export] public PackedScene BeeScene {get; set;}
    [Export] public Array<Node2D> LeftSpawnPoints {get; set;} = new Array<Node2D>();
    [Export] public Array<Node2D> RightSpawnPoints {get; set;} = new Array<Node2D>();
    [Export] public int Minbees {get; set;} = 3;
    [Export] public int MaxBees {get; set;} = 6;
    [Export] public float CameraShakeDuration {get; set;} = 1f;
    [Export] public float SpawnDelay {get; set;} = 1f;
    [Export] public StateMachine AttackStateMachine {get; set;}
    [Export] public State AttackIdleState {get; set;}

    private Character _drill;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private Tween _tween;
    private double _curTime = 0.0;
    private bool _shook = false;
    private bool _spawned = false;

    public override State Enter(State previousState)
    {
        _drill = Context as Character;
        _curTime = 0.0;
        _shook = false;
        _spawned = false;

        if (_drill != null && CeilingPosition != null)
        {
            _tween = _drill.CreateTween();
            _tween.TweenProperty(_drill, "global_position", CeilingPosition.GlobalPosition, DrillTweenDuration)
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
            SpawnBees(LeftSpawnPoints);
            SpawnBees(RightSpawnPoints);
            AttackStateMachine?.ChangeState(AttackIdleState);
        }

        return base.Process(delta);
    }

    public override void Exit(State nextState)
    {
        _tween?.Kill();

        base.Exit(nextState);
    }

    private void SpawnBees(Array<Node2D> spawnPoints)
    {
        if (BeeScene == null || spawnPoints.Count == 0)
        {
            return;
        }

        
        for  (int i = 0; i < spawnPoints.Count; i++)
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

}
