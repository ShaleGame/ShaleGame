using System.Collections.Generic;
using Godot;
using CrossedDimensions.Extensions;

namespace CrossedDimensions.Characters;

[GlobalClass]
public partial class MergeEffectSource : Node
{
    [Export]
    public CloneableComponent Cloneable { get; set; }

    [Export]
    public Node2D OriginalSpriteAnchor { get; set; }

    [Export]
    public Shader GhostShader { get; set; }

    [Export]
    public Color GhostColor { get; set; }

    [Export]
    public float TravelDuration { get; set; } = 0.5f;

    [Export]
    public float FadeStartProgress { get; set; } = 0.25f;

    [Export]
    public int GhostCount { get; set; } = 4;

    [Export]
    public float SpawnStagger { get; set; } = 0.03f;

    [Export]
    public float FastMergeFadeDuration { get; set; } = 1.0f;

    [Export]
    public int FastMergeGhostCount { get; set; } = 1;

    [Export]
    public int ZIndexOffset { get; set; } = -1;

    [Export]
    public AudioStreamPlayer2D MergeSound { get; set; }

    private static readonly StringName GhostColorParam = "ghost_color";

    private readonly List<GhostMotion> _activeGhosts = new();

    public override void _Ready()
    {
        Cloneable ??= GetParentOrNull<CloneableComponent>();

        if (Cloneable is not null)
        {
            Cloneable.CharacterMerging += OnCharacterMerging;
        }
    }

    public override void _ExitTree()
    {
        if (Cloneable is not null)
        {
            Cloneable.CharacterMerging -= OnCharacterMerging;
        }

        base._ExitTree();
    }

    public override void _Process(double delta)
    {
        if (_activeGhosts.Count == 0)
        {
            return;
        }

        float step = (float)delta;
        for (int i = _activeGhosts.Count - 1; i >= 0; i--)
        {
            var motion = _activeGhosts[i];

            bool isInvalid = !GodotObject.IsInstanceValid(motion.Ghost);
            if (isInvalid || motion.Ghost.IsQueuedForDeletion())
            {
                _activeGhosts.RemoveAt(i);
                continue;
            }

            motion.Elapsed += step;

            if (motion.Elapsed < motion.StartDelay)
            {
                motion.Ghost.Visible = false;
                _activeGhosts[i] = motion;
                continue;
            }

            motion.Ghost.Visible = true;

            float effectiveElapsed = motion.Elapsed - motion.StartDelay;
            float t = Mathf.Clamp(effectiveElapsed / motion.Duration, 0f, 1f);
            if (motion.IsTraveling)
            {
                float eased = LogisticEaseInOut(t);
                Vector2 targetPosition = motion.TargetAnchor is not null
                    ? motion.TargetAnchor.GlobalPosition
                    : motion.EndPosition;
                motion.Ghost.GlobalPosition = motion.StartPosition.Lerp(targetPosition, eased);
            }

            float fadeStart = Mathf.Clamp(FadeStartProgress, 0f, 0.999f);
            float fadeT = Mathf.Clamp(Mathf.InverseLerp(fadeStart, 1f, t), 0f, 1f);
            Color modulate = motion.Ghost.Modulate;
            modulate.A = 1f - fadeT;
            motion.Ghost.Modulate = modulate;

            if (t >= 1f)
            {
                motion.Ghost.QueueFree();
                _activeGhosts.RemoveAt(i);
            }
            else
            {
                _activeGhosts[i] = motion;
            }
        }
    }

    private void OnCharacterMerging(Character original, Character mirror, bool fastMerge)
    {
        if (Cloneable is null || Cloneable.IsClone || original is null || mirror is null)
        {
            return;
        }

        TriggerMergeEffects(original, mirror, fastMerge);
    }

    private void TriggerMergeEffects(Character original, Character mirror, bool fastMerge)
    {
        PlayMergeVisuals(original, mirror, fastMerge);
        PlayMergeSound(original, mirror);
    }

    private void PlayMergeVisuals(Character original, Character mirror, bool fastMerge)
    {
        Node2D sourceAnchor = mirror.SpriteAnchor;
        Node2D targetAnchor = OriginalSpriteAnchor ?? original.SpriteAnchor;
        Node parent = original.GetParent();

        if (sourceAnchor is null || targetAnchor is null || parent is null)
        {
            return;
        }

        int ghostCount = Mathf.Max(1, fastMerge ? FastMergeGhostCount : GhostCount);
        float duration = Mathf.Max(0.001f, fastMerge ? FastMergeFadeDuration : TravelDuration);
        float spawnStagger = fastMerge ? 0f : Mathf.Max(0f, SpawnStagger);

        for (int i = 0; i < ghostCount; i++)
        {
            var ghost = sourceAnchor.Duplicate() as Node2D;
            if (ghost is null)
            {
                continue;
            }

            parent.AddChild(ghost);
            ghost.GlobalTransform = sourceAnchor.GlobalTransform;
            ghost.ZAsRelative = false;
            ghost.ZIndex = original.ZIndex + ZIndexOffset;
            ghost.Modulate = new Color(1f, 1f, 1f, 1f);

            ApplyGhostMaterialRecursive(ghost);

            _activeGhosts.Add(new GhostMotion
            {
                Ghost = ghost,
                StartPosition = ghost.GlobalPosition,
                EndPosition = targetAnchor.GlobalPosition,
                TargetAnchor = targetAnchor,
                Duration = duration,
                Elapsed = 0f,
                StartDelay = i * spawnStagger,
                IsTraveling = !fastMerge,
            });
        }
    }

    private void PlayMergeSound(Character original, Character mirror)
    {
        if (MergeSound is null)
        {
            return;
        }

        var audio = MergeSound.PlayOneShot();
        audio.GlobalPosition = original.GlobalPosition;
    }

    private void ApplyGhostMaterialRecursive(Node node)
    {
        if (node is Sprite2D sprite)
        {
            var shader = GhostShader;
            if (shader is not null)
            {
                var material = new ShaderMaterial { Shader = shader };
                material.SetShaderParameter(GhostColorParam, GhostColor);
                sprite.Material = material;
            }
        }

        foreach (var child in node.EnumerateChildren())
        {
            ApplyGhostMaterialRecursive(child);
        }
    }

    private static float LogisticEaseInOut(float t)
    {
        float x = Mathf.Clamp(t, 0f, 1f) * 2f - 1f;
        const float steepness = 8f;

        float y = 1f / (1f + Mathf.Exp(-steepness * x));
        float yMin = 1f / (1f + Mathf.Exp(steepness));
        float yMax = 1f / (1f + Mathf.Exp(-steepness));

        return Mathf.Clamp((y - yMin) / (yMax - yMin), 0f, 1f);
    }

    private struct GhostMotion
    {
        public Node2D Ghost;
        public Vector2 StartPosition;
        public Vector2 EndPosition;
        public Node2D TargetAnchor;
        public float Duration;
        public float Elapsed;
        public float StartDelay;
        public bool IsTraveling;
    }
}
