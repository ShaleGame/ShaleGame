using Godot;

namespace CrossedDimensions.Environment;

[GlobalClass]
public partial class AreaManager : Node
{
    public static AreaManager Instance { get; private set; }

    [Signal]
    public delegate void AreaTriggerEnteredEventHandler(AreaData data, bool updateLastShownArea);

    public override void _Ready()
    {
        Instance = this;
    }

    public void NotifyAreaTitleTriggerEntered(AreaData data, bool updateLastShownArea = true)
    {
        if (data is null)
        {
            return;
        }

        EmitSignal(SignalName.AreaTriggerEntered, data, updateLastShownArea);
    }
}
