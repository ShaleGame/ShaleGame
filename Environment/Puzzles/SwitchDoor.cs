using Godot;
namespace CrossDimensions.Environment.Puzzles;
public partial class SwitchDoor : Node
{
    [Export]
    public Godot.Collections.Array<Node2D> Switches { get; set; } = [];
    private Godot.Collections.Array<SwitchButton> Buttons { get; set; } = [];
    [Export]
    public CollisionShape2D DoorCollider { get; set; }
    [Export]
    public Sprite2D Sprite { get; set; }
    [Export]
    private Texture2D ClosedTexture;
    [Export]
    private Texture2D OpenTexture;
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
        var pressed = 0;
        foreach ( SwitchButton button in Buttons )
        {
            if (button.SwitchPressed)
            {
                pressed++;
            }
        }
        GD.Print($"Pressed switches: {pressed}");
        bool allPressed = pressed == Buttons.Count;
        if ( allPressed && !Open )
        {
            OpenDoor();
        } 
        else if ( !allPressed && Open )
        {
            //use this if the door is to close when either switch *isn't* pressed
            //if it's supposed to stay open after both switches pressed, don't
            //CloseDoor();
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