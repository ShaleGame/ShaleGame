using Godot;
namespace CrossDimensions.Environment.Puzzles;
public partial class SwitchDoor : Node
{
    [Export]
    public Godot.Collections.Array<SwitchButton> Switches { get; set; } = new();
    public bool Open { get; set; } = false;
    /// <summary>
    /// Checks if all switches are pressed, and opens the door if so
    /// </summary>
    public void Activate()
    {
        var pressed = 0;
        foreach ( SwitchButton button in Switches )
        {
            if (button.SwitchPressed)
            {
                pressed++;
            }
        }
        if ( pressed == Switches.Count )
        {
            Open = true;
        }
    }
}