using Godot;

namespace CrossDimensions.Characters;

public sealed partial class CloneableComponent : Node
{
    public Character Character => GetParent<Character>();

    public Character Original { get; set; }

    public Character Clone { get; set; }

    public Character Mirror => Original ?? Clone;

    public bool IsClone => Original is not null;

    [Export]
    public float SplitForce { get; set; } = 768f;

    public bool Split()
    {
        if (Mirror is not null)
        {
            // can't split if there is already a clone
            return false;
        }

        var clone = (Character)Character.Duplicate();

        var clonesComponent = clone
            .GetNode<CloneableComponent>("%CloneableComponent");
        clonesComponent.Original = Character;
        clone.Controller.XScale *= -1;

        Clone = clone;

        Character.GetParent().AddChild(clone);

        return true;
    }
}
