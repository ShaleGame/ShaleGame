using Godot;
using System;

namespace CrossDimensions.Environment.Puzzles;

public partial class SwitchDoor : Node
{
    [Export]
    public Godot.Collections.Array<SwitchButton> Switches { get; set; } = new();

    public void Activate()
    {
        
    }
}
