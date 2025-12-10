using Godot;

namespace CrossDimensions.Characters;

/// <summary>
/// Component that allows a character to be cloned and merged.
/// </summary>
public sealed partial class CloneableComponent : Node
{
    public Character Character => GetParent<Character>();

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

        // add clone to the same parent as the original character
        // so that they are siblings in the scene tree
        Character.GetParent().AddChild(clone);

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

        if (IsClone)
        {
            Original.Cloneable.Clone = null;
            QueueFree();
        }
        else
        {
            Clone.QueueFree();
            Clone = null;
        }
    }
}
