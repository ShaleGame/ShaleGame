using Godot;
using CrossedDimensions.Characters;
using CrossedDimensions.Extensions;

namespace CrossedDimensions.States.Characters.Behaviors;

[GlobalClass]
public partial class DashGhostTrailBehavior : States.State
{
    [Export]
    public Node2D SpriteAnchor { get; set; }

    [Export]
    public Shader GhostShader { get; set; }

    [Export]
    public Color GhostColor { get; set; } = new(0.92156863f, 0.20392157f, 0.60784316f, 1.0f);

    [Export]
    public float SpawnInterval { get; set; } = 0.025f;

    [Export]
    public float SpawnDuration { get; set; } = 0.1f;

    [Export]
    public float GhostLifetime { get; set; } = 0.18f;

    [Export]
    public int MaxGhostsPerDash { get; set; } = 4;

    [Export]
    public int ZIndexOffset { get; set; } = -1;

    private static readonly StringName GhostColorParam = "ghost_color";

    private Character _character;
    private float _spawnTimeRemaining;
    private float _spawnAccumulator;
    private int _ghostsSpawned;

    public override States.State Enter(States.State previousState)
    {
        _character = Context as Character;

        _spawnTimeRemaining = SpawnDuration;
        _spawnAccumulator = 0f;
        _ghostsSpawned = 0;

        SpawnGhost();

        return base.Enter(previousState);
    }

    public override States.State Process(double delta)
    {
        if (_character is null)
        {
            return base.Process(delta);
        }

        if (_ghostsSpawned >= MaxGhostsPerDash)
        {
            return base.Process(delta);
        }

        _spawnTimeRemaining -= (float)delta;
        _spawnAccumulator += (float)delta;

        while (_spawnAccumulator >= SpawnInterval
            && _spawnTimeRemaining > 0f
            && _ghostsSpawned < MaxGhostsPerDash)
        {
            _spawnAccumulator -= SpawnInterval;
            SpawnGhost();
        }

        return base.Process(delta);
    }

    public override void Exit(States.State nextState)
    {
        _spawnTimeRemaining = 0f;
        _spawnAccumulator = 0f;
        _ghostsSpawned = 0;
        _character = null;

        base.Exit(nextState);
    }

    private void SpawnGhost()
    {
        Node2D sourceAnchor = SpriteAnchor ?? _character?.SpriteAnchor;
        if (sourceAnchor is null)
        {
            return;
        }

        var ghost = sourceAnchor.Duplicate() as Node2D;
        if (ghost is null)
        {
            return;
        }

        _character.AddSibling(ghost);
        ghost.GlobalTransform = sourceAnchor.GlobalTransform;
        ghost.ZIndex = ZIndexOffset;

        ApplyGhostMaterialRecursive(ghost);

        var tween = ghost.CreateTween();
        tween.TweenProperty(ghost, "modulate:a", 0.0f, GhostLifetime);
        tween.Finished += ghost.QueueFree;

        _ghostsSpawned++;
    }

    private void ApplyGhostMaterialRecursive(Node node)
    {
        if (node is Sprite2D sprite)
        {
            if (GhostShader is not null)
            {
                var material = new ShaderMaterial { Shader = GhostShader };
                material.SetShaderParameter(GhostColorParam, GhostColor);
                sprite.Material = material;
            }
        }

        foreach (var child in node.EnumerateChildren())
        {
            ApplyGhostMaterialRecursive(child);
        }
    }
}
