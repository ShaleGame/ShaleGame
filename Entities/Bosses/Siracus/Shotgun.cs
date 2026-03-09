using Godot;
using System;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using System.Numerics;

namespace CrossedDimensions.Entities.Bosses.Siracus;

// Spits out burst of projectiles in a cone in front of Siracus

public partial class Shotgun : State
{
    [Export]
    public PackedScene icicleProjectile;

    [Export]
    public Node2D icicleSpawn;

    [Export]
    public float coneAngleDegrees = 45;

    [Export]
    public int maxWaves = 3;
    private int _curWaves = 0;
    
    [Export]
    public int maxIcicles = 3;

    [Export]
    public double refreshTime = 1;
    private double _time = 0;

    private Character _siracus;
    private Character _player;

    private State _attackIdle;

    private AnimatedSprite2D _animSprite;

    public override State Enter(State previousState)
    {
        _siracus = Context as Character;

        _player = GetTree().GetFirstNodeInGroup("Player") as Character;

        _time = 0;
        _curWaves = 0;

        _attackIdle = GetParent().FindChild("Idle") as State;

        _animSprite = _siracus.FindChild("AnimatedSprite2D") as AnimatedSprite2D;
        _animSprite.Play("Shotgun");

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        _time += delta;

        if (_time >= refreshTime)
        {
            Godot.Vector2 targetDirection = _siracus.GlobalPosition.DirectionTo(_player.GlobalPosition);

            float coneRad = Mathf.DegToRad(coneAngleDegrees);

            // With 1 projectile, shoot staight. Otherwise spread evenly

            for (int i = 0; i < maxIcicles; i++)
            {
                float t = maxIcicles == 1
                    ? 0f
                    : (float)i / (maxIcicles - 1); // 0.0 to 1.0
                
                // Map t from [0, 1] to [-half, +half] of the cone
                float angleOffset = Mathf.Lerp(-coneRad / 2f, coneRad / 2f, t);

                Godot.Vector2 direction = targetDirection.Rotated(angleOffset).Normalized();
                
                SpawnProjectile(direction);
            }

            _curWaves++;
            _time = 0;

            if (_curWaves >= maxWaves)
            {
                return _attackIdle;
            }
        }

        return base.Process(delta);
    }

    private void SpawnProjectile(Godot.Vector2 direction)
    {
        var projectile = icicleProjectile.Instantiate<Node2D>();

        GetTree().CurrentScene.AddChild(projectile);

        projectile.GlobalPosition = icicleSpawn.GlobalPosition;
    }
}
