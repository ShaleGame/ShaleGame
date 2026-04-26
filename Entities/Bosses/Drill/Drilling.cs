using Godot;
using System;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using System.Reflection.PortableExecutable;
using Castle.Components.DictionaryAdapter.Xml;

namespace CrossedDimensions.Entities.Bosses.Drill;

/// <summary>
/// Goes fully down and drills the floor. Aether lava flows up and moving platforms do too. The platforms first and then the lava. Player must do enough damage to the drill to stop the drilling and end the attack.
/// </summary>

public partial class Drilling : State
{
    
    [Export] public float DrillTweenDuration {get; set;} = 1.5f;
    [Export] public float PlatformDelay {get; set;} = 1f;
    [Export] public float LavaDelay {get; set;} = 3f;
    [Export] public float DamageThresholdPercent = 0.25f;
    [Export] public StateMachine AttackStateMachine {get; set;}
    [Export] public State AttackIdleState {get; set;}
    [Export] public Components.HealthComponent DrillHealth {get; set;}

    private Character _drill;
    private double _curTime = 0.0;
    private bool _platformsEnabled = false;
    private bool  _lavaEnabled = false;
    private Tween _tween;
    private Tween _platformTween;
    private Tween _lavaTween;
    private int _healthAtStart;

    // External nodes
    private Node2D _holder;
    private Vector2 _start;
    private Node2D _drillDownPosition;
    private Node2D _platforms;
    private Node2D _platformsUp;
    private Node2D _platformsDown;
    private Node2D _lava;
    private Node2D _lavaUp;
    private Node2D _lavaDown;

    public override State Enter(State previousState)
    {
        _drill = Context as Character;
        _curTime = 0.0;
        _platformsEnabled = false;
        _lavaEnabled = false;

        _start = _drill.GlobalPosition;

        // Get external nodes
        _holder = GetTree().GetFirstNodeInGroup("DrillBossHolder") as Node2D;

        _drillDownPosition = _holder.GetNodeOrNull<Node2D>("DownPosition");

        _platforms = _holder.GetNodeOrNull<Node2D>("PlatformsContainer/Platforms");
        _platformsUp = _holder.GetNodeOrNull<Node2D>("PlatformsContainer/Up");
        _platformsDown = _holder.GetNodeOrNull<Node2D>("PlatformsContainer/Down");

        _lava = _holder.GetNodeOrNull<Node2D>("LavaContainer/Lava");
        _lavaUp = _holder.GetNodeOrNull<Node2D>("LavaContainer/Up");
        _lavaDown = _holder.GetNodeOrNull<Node2D>("LavaContainer/Down");

        if (DrillHealth != null)
        {
            _healthAtStart = DrillHealth.CurrentHealth;
            DrillHealth.HealthChanged  += OnHealthChanged;
        }

        // Tween drill to  drilling position
        if (_drill != null && _drillDownPosition != null)
        {
            _tween = _drill.CreateTween();
            _tween.TweenProperty(_drill, "global_position",  _drillDownPosition.GlobalPosition, DrillTweenDuration)
                .SetTrans(Tween.TransitionType.Bounce)
                .SetEase(Tween.EaseType.Out);
        }

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (_drill == null)
        {
            return base.Process(delta);
        }

        // Only start counting once the tween is done
        if (_tween != null && !_tween.IsRunning())
        {
            _curTime += delta;

            if (!_platformsEnabled && _curTime >= PlatformDelay)
            {
                _platformsEnabled = true;
                if (_platforms != null)
                {
                    _platformTween = _platforms.CreateTween();
                    _platformTween.TweenProperty(_platforms, "global_position",  _platformsUp.GlobalPosition, DrillTweenDuration)
                        .SetTrans(Tween.TransitionType.Cubic)
                        .SetEase(Tween.EaseType.Out);
                }
            }

            if (!_lavaEnabled && _curTime >= LavaDelay)
            {
                _lavaEnabled = true;
                if (_lava != null)
                {
                    _lavaTween = _lava.CreateTween();
                    _lavaTween.TweenProperty(_lava, "global_position",  _lavaUp.GlobalPosition, DrillTweenDuration)
                        .SetTrans(Tween.TransitionType.Cubic)
                        .SetEase(Tween.EaseType.Out);
                }
            }
        }

        return base.Process(delta);
    }

    public override void Exit(State nextState)
    {
        
        _tween = _drill.CreateTween();
        _tween.TweenProperty(_drill, "global_position", _start, DrillTweenDuration)
            .SetTrans(Tween.TransitionType.Bounce)
            .SetEase(Tween.EaseType.Out);

        if (DrillHealth != null)
        {
            DrillHealth.HealthChanged -= OnHealthChanged;
        }

        if (_platforms != null)
        {
            _platformTween = _platforms.CreateTween();
            _platformTween.TweenProperty(_platforms, "global_position",  _platformsDown.GlobalPosition, DrillTweenDuration)
                        .SetTrans(Tween.TransitionType.Cubic)
                        .SetEase(Tween.EaseType.Out);
        }

        if (_lava != null)
        {
            _lavaTween = _lava.CreateTween();
            _lavaTween.TweenProperty(_lava, "global_position",  _lavaDown.GlobalPosition, DrillTweenDuration)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.Out);
        }

        _tween?.Kill();
        _lavaTween?.Kill();
        _platformTween?.Kill();

        base.Exit(nextState);
    }

    private void OnHealthChanged(int oldHealth)
    {
        int damageTaken = _healthAtStart - DrillHealth.CurrentHealth;
        float percentLost = (float)damageTaken / DrillHealth.MaxHealth;

        if (percentLost >= DamageThresholdPercent)
        {
            AttackStateMachine?.ChangeState(AttackIdleState);
        }
    }

}
