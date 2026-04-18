using Godot;

namespace CrossedDimensions.Environment;

[GlobalClass]
public partial class AreaManager : Node
{
    public static AreaManager Instance { get; private set; }

    [Signal]
    public delegate void AreaTriggerEnteredEventHandler(AreaData data);

    public override void _Ready()
    {
        Instance = this;
    }

    public void NotifyAreaTitleTriggerEntered(AreaData data)
    {
        if (data is null)
        {
            return;
        }

        EmitSignal(SignalName.AreaTriggerEntered, data);
    }
}
