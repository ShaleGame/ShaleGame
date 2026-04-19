using Godot;
using System;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using System.Reflection.PortableExecutable;

namespace CrossedDimensions.Entities.Bosses.Drill;

/// <summary>
/// Goes fully down and drills the floor. Aether lava flows up and moving platforms do too. The platforms first and then the lava. Player must do enough damage to the drill to stop the drilling and end the attack.
/// </summary>

public partial class Drilling : State
{
    
    [Export] public Node2D DrillDownPosition {get; set;}
    [Export] public float DrillTweenDuration {get; set;} = 1.5f;
    [Export] public Node2D Platforms {get; set;}
    [Export] public Node2D Lava {get; set;}
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
    private int _healthAtStart;

    public override State Enter(State previousState)
    {
        _drill = Context as Character;
        _curTime = 0.0;
        _platformsEnabled = false;
        _lavaEnabled = false;

        if (Platforms != null)
        {
            Platforms.ProcessMode = ProcessModeEnum.Disabled;
        }

        if (Lava != null)
        {
            Lava.ProcessMode = ProcessModeEnum.Disabled;
        }

        if (DrillHealth != null)
        {
            _healthAtStart = DrillHealth.CurrentHealth;
            DrillHealth.HealthChanged  += OnHealthChanged;
        }

        // Tween drill to  drilling position
        if (_drill != null && DrillDownPosition != null)
        {
            _tween = _drill.CreateTween();
            _tween.TweenProperty(_drill, "global_position",  DrillDownPosition.GlobalPosition, DrillTweenDuration)
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
                if (Platforms != null)  {
                    Platforms.ProcessMode = ProcessModeEnum.Inherit;
                }
            }

            if (_lavaEnabled && _curTime >= LavaDelay)
            {
                _lavaEnabled = true;
                if (Lava != null)
                {
                    Lava.ProcessMode = ProcessModeEnum.Inherit;
                }
            }
        }

        return base.Process(delta);
    }

    public override void Exit(State nextState)
    {
        _tween?.Kill();

        if (DrillHealth != null)
        {
            DrillHealth.HealthChanged -= OnHealthChanged;
        }

        if (Platforms != null)
        {
            Platforms.ProcessMode = ProcessModeEnum.Disabled;
        }

        if (Lava != null)
        {
            Lava.ProcessMode = ProcessModeEnum.Disabled;
        }

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
