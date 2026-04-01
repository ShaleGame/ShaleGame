using System;
using Godot;

namespace CrossedDimensions.UI.UIDeathScreen;

public partial class DeathScreen : Control
{
    public override void _Ready()
    {
        var priorities = Enum.GetValues(typeof(Audio.MusicPriority))
            as Audio.MusicPriority[];

        foreach (Audio.MusicPriority priority in priorities)
        {
            Audio.MusicManager.Instance.StopTrack(priority);
        }
    }
}
