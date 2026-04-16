using Godot;
using System.Collections.Generic;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class SecuritySystem : State
{
    [Export]
    public PackedScene TurretScene { get; set; }

    [Export]
    public Node CeilingTurretSpawns { get; set; }

    [Export]
    public double ActiveDuration { get; set; } = 5.0;

    private Character _boss;
    private State _attackIdle;
    private readonly List<Character> _spawnedTurrets = new();
    private double _timer;

    public override State Enter(State previousState)
    {
        _boss = Context as Character;
        _attackIdle = GetParent().GetNode<State>("AttackIdle");

        _timer = ActiveDuration;

        CleanupTurrets();
        SpawnTurrets();

        var anim = _boss?.GetNode<AnimationPlayer>("AnimationPlayer");
        anim?.Play("SecuritySystem");

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        _timer -= delta;
        if (_timer <= 0)
        {
            return _attackIdle;
        }

        return null;
    }

    public override void Exit(State nextState)
    {
        CleanupTurrets();
        base.Exit(nextState);
    }

    private void SpawnTurrets()
    {
        if (_boss == null || TurretScene == null || CeilingTurretSpawns == null)
        {
            return;
        }

        foreach (var child in CeilingTurretSpawns.GetChildren())
        {
            if (child is not Marker2D spawn)
            {
                continue;
            }

            var turret = TurretScene.Instantiate<Character>();
            _boss.AddChild(turret);
            turret.TopLevel = true;
            turret.GlobalPosition = spawn.GlobalPosition;
            turret.RotationDegrees = 180f;

            _spawnedTurrets.Add(turret);
        }
    }

    private void CleanupTurrets()
    {
        foreach (var turret in _spawnedTurrets)
        {
            if (GodotObject.IsInstanceValid(turret))
            {
                turret.QueueFree();
            }
        }

        _spawnedTurrets.Clear();
    }
}
