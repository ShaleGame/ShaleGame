using Godot;

namespace CrossDimensions.Items;

public partial class ItemInstance : Node2D
{
    [Export]
    public Characters.Character OwnerCharacter { get; set; }

    public virtual void PrimaryUseStart()
    {

    }

    public virtual void PrimaryUseEnd()
    {

    }
}
