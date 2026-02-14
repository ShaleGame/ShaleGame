using Godot;

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

        int cloneHealth = Character.Health.CurrentHealth / 2;
        int cloneMaxHealth = Character.Health.MaxHealth / 2;

        int originalHealth = Character.Health.CurrentHealth - cloneHealth;
        int originalMaxHealth = Character.Health.MaxHealth - cloneMaxHealth;

        Character.Health.SetStats(originalHealth, originalMaxHealth);
        clone.Health.SetStats(cloneHealth, cloneMaxHealth);

        // add clone to the same parent as the original character
        // so that they are siblings in the scene tree
        Character.GetParent().AddChild(clone);

        EmitSignal(SignalName.CharacterSplitPost, Character, clone);

        return clone;
    }

    /// <summary>
    /// Merges the clone back into the original character. If there is no
    /// clone, this method does nothing.
    /// </summary>
    public void Merge()
    {
        if (Mirror is null)
        {
            return;
        }

        int maxHealth = Character.Health.MaxHealth + Mirror.Health.MaxHealth;
        int health = Character.Health.CurrentHealth + Mirror.Health.CurrentHealth;

        if (IsClone)
        {
            Original.Health.SetStats(health, maxHealth);

            Original.Cloneable.Clone = null;
            Character.QueueFree();
        }
        else
        {
            Character.Health.SetStats(health, maxHealth);
            Clone.QueueFree();
            Clone = null;
        }

        EmitSignal(SignalName.CharacterMerged, Original);
    }
}
