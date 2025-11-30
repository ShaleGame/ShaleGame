using System.Linq;
using Godot;
namespace CrossDimensions.Environment.Puzzles;
public partial class SwitchDoor : Node
{
    [Export]
    public Godot.Collections.Array<Node2D> Switches { get; set; } = [];
    private Godot.Collections.Array<SwitchButton> Buttons = [];
    [Export]
    public CollisionShape2D DoorCollider { get; set; }
    [Export]
    public Sprite2D Sprite { get; set; }
    [Export]
    private Texture2D ClosedTexture;
    [Export]
    private Texture2D OpenTexture;
    [Export]
    public bool StayOpen { get; set; } = false;
    public bool Open { get; set; } = false;
    public override void _Ready()
    {
        foreach ( Node2D switch_node in Switches )
        {
            Buttons.Add( switch_node.GetNode<SwitchButton>("Area2D") );
        }
    }
    public void Activate()
    {
        bool allPressed = Buttons.All( (button) => button.SwitchPressed );
        if ( allPressed && !Open )
        {
            OpenDoor();
        } 
        else if ( !allPressed && Open && !StayOpen )
        {
            CloseDoor();
        }
    }
    private void OpenDoor()
    {
        GD.Print("Door open!");
        Open = true;
        DoorCollider.CallDeferred("set_disabled", true);
        Sprite.Texture = OpenTexture;
    }
    private void CloseDoor()
    {
        GD.Print("Door closed!");
        Open = false;
        DoorCollider.CallDeferred("set_disabled", false);
        Sprite.Texture = ClosedTexture;
    }
}