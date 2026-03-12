using Godot;

namespace CrossedDimensions.UI;

public partial class MainMenuMusicPlayer : Node
{
    [Export]
    public AudioStreamInteractive Stream { get; set; }

    [Export]
    public Audio.MusicPriority MusicPriority { get; set; } = Audio.MusicPriority.Background;

    [Export]
    public string ClipName { get; set; }

    public Audio.MusicManager MusicManager { get; set; }

    public override void _Ready()
    {
        MusicManager ??= Audio.MusicManager.Instance;

        GetTree().CreateTimer(1.0).Timeout += () =>
        {
            float volume = 0.5f;
            float initialVolume = 1f;
            MusicManager?.PlayTrack(Stream, MusicPriority, ClipName, volume, initialVolume);
        };

    }
}
