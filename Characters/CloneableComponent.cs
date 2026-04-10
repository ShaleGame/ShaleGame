using Godot;
using CrossedDimensions.Extensions;

namespace CrossedDimensions.Characters;

/// <summary>
/// Component that allows a character to be cloned and merged.
/// </summary>
public sealed partial class CloneableComponent : Node
{
    public Character Character => GetParent<Character>();

    /// <summary>
    /// Emitted when a character has been split, before the clone is added to
    /// the scene tree.
    /// </summary>
    [Signal]
    public delegate void CharacterSplitEventHandler(Character original, Character clone);

    /// <summary>
    /// Emitted after a character has been split, after the clone has been
    /// added to the scene tree.
    /// </summary>
    [Signal]
    public delegate void CharacterSplitPostEventHandler(Character original, Character clone);

    [Signal]
    public delegate void CharacterMergedEventHandler(Character character);

    [Signal]
    public delegate void CharacterMergingEventHandler(Character original, Character mirror, bool fastMerge);

    [Signal]
    public delegate void HealingPoolChangedEventHandler(float current, float max);

    /// <summary>
    /// The original character if this is a clone, otherwise null.
    /// </summary>
    public Character Original { get; set; }

    /// <summary>
    /// The cloned character if this is the original, otherwise null.
    /// </summary>
    public Character Clone { get; set; }

    /// <summary>
    /// Gets the mirror of this character: the original if this is a clone,
    /// or the clone if this is the original.
    /// </summary>
    public Character Mirror => Original ?? Clone;

    /// <summary>
    /// The instance ID of the most recent mirror before the last merge.
    /// Used to prevent self-damage from in-flight projectiles fired just
    /// before merging, whose OwnerCharacter node may now be freed.
    /// </summary>
    public ulong LastMirrorId { get; private set; } = ulong.MaxValue;

    /// <returns>
    /// <c>true</c> if this character is a clone; otherwise, <c>false</c>.
    /// </returns>
    public bool IsClone => Original is not null;

    /// <summary>
    /// The initial velocity magnitude applied to the character when splitting.
    /// </summary>
    [Export]
    public float SplitForce { get; set; } = 768f;

    /// <summary>
    /// The minimum amount of time between split attempts, in seconds.
    /// </summary>
    [Export]
    public double SplitCooldownDuration { get; set; } = 0.5;

    /// <summary>
    /// The time window after splitting in which a release input can merge,
    /// in seconds.
    /// </summary>
    [Export]
    public double SplitMergeWindowDuration { get; set; } = 0.25;

    /// <summary>
    /// The percentage of damage dealt by the clone that is converted to
    /// healing for the original character and stored in the healing pool.
    /// </summary>
    [Export]
    public float HealEfficiency { get; set; } = 0.5f;

    /// <summary>
    /// The time it takes to apply the entire healing pool to the original
    /// character when merging, in seconds.
    /// </summary>
    [Export]
    public float HealTime { get; set; } = 2.0f;

    private float _healingPool;

    public float HealingPool
    {
        get => _healingPool;
        private set
        {
            System.Diagnostics.Debug.Assert(
                Character?.Health is not null, "Character or Health component is null");

            float maxPool = Character.Health.MaxHealth;
            _healingPool = Mathf.Clamp(value, 0f, maxPool);
            EmitSignal(SignalName.HealingPoolChanged, _healingPool, maxPool);
        }
    }

    public void AddToHealingPool(float amount)
    {
        System.Diagnostics.Debug.Assert(
            Character?.Health is not null, "Character or Health component is null");
        float maxPool = Character.Health.MaxHealth;
        HealingPool = Mathf.Min(_healingPool + amount, maxPool);
    }

    public void ClearHealingPool()
    {
        HealingPool = 0f;
    }

    public void DrainHealingPool(float amount)
    {
        HealingPool = _healingPool - amount;
    }

    private double _splitMergeWindowEndTime;

    private double CurrentTime => Time.GetTicksMsec() / 1000.0;

    public override void _PhysicsProcess(double delta)
    {
        if (Character?.Controller is null)
        {
            return;
        }

        if (Character.Controller.IsSplitReleased)
        {
            TryMergeOnSplitRelease();
        }
    }

    /// <summary>
    /// Splits the character, creating a mirrored clone. The character can
    /// only be split if there is no existing clone.
    /// </summary>
    /// <returns>The cloned character, or null if split failed.</returns>
    public Character Split()
    {
        if (Mirror is not null)
        {
            // can't split if there is already a clone
            return null;
        }

        LastMirrorId = ulong.MaxValue;
        HealingPool = 0f;

        var clone = (Character)Character.Duplicate();

        var clonesComponent = clone
            .GetNode<CloneableComponent>("%CloneableComponent");
        clonesComponent.Original = Character;
        clone.Controller.XScale *= -1;

        Clone = clone;

        // set clone state to split state
        // NOTE: this uses GetNode with a hardcoded name. We could use Exports
        // to make this more flexible if needed, especially since the state
        // name uses spaces, which may or may not need to follow a convention.
        clone.MovementStateMachine.InitialState = clone
            .MovementStateMachine
            .GetNode<States.Characters.CharacterSplitState>("Split State");

        EmitSignal(SignalName.CharacterSplit, Character, clone);

        SplitHealth(Character, clone);
        RemoveCameraFromClone(clone);

        // add clone to the same parent as the original character
        // so that they are siblings in the scene tree
        Character.GetParent().AddChild(clone);

        EmitSignal(SignalName.CharacterSplitPost, Character, clone);

        _splitMergeWindowEndTime = CurrentTime + SplitMergeWindowDuration;

        return clone;
    }

    private void SplitHealth(Character original, Character clone)
    {
        int cloneHealth = original.Health.CurrentHealth / 2;
        int cloneMaxHealth = original.Health.MaxHealth / 2;

        int originalHealth = original.Health.CurrentHealth - cloneHealth;
        int originalMaxHealth = original.Health.MaxHealth - cloneMaxHealth;

        original.Health.SetStats(originalHealth, originalMaxHealth);
        clone.Health.SetStats(cloneHealth, cloneMaxHealth);
    }

    private void RemoveCameraFromClone(Character clone)
    {
        if (clone.HasNode<Camera2D>("Camera2D", out var camera))
        {
            camera.QueueFree();
        }
    }

    public void TryMergeOnSplitRelease()
    {
        if (Mirror is null || IsClone)
        {
            return;
        }

        if (CurrentTime <= _splitMergeWindowEndTime)
        {
            Merge(fastMerge: true);
        }
    }

    /// <summary>
    /// Merges the clone back into the original character. If there is no
    /// clone, this method does nothing.
    /// </summary>
    public void Merge(bool fastMerge = false)
    {
        if (Mirror is null)
        {
            return;
        }

        if (IsClone)
        {
            Original.Cloneable.Merge(fastMerge);
        }
        else
        {
            var mirror = Mirror;
            if (mirror is null)
            {
                return;
            }

            EmitSignal(SignalName.CharacterMerging, Character, mirror, fastMerge);

            int maxHealth = Character.Health.MaxHealth + mirror.Health.MaxHealth;
            int health = Character.Health.CurrentHealth + mirror.Health.CurrentHealth;

            Character.Health.SetStats(health, maxHealth);
            LastMirrorId = mirror.GetInstanceId();
            Clone.QueueFree();
            Clone = null;
        }

        EmitSignal(SignalName.CharacterMerged, Original);
    }
}
