using Godot;

namespace CrossedDimensions.Audio;

[GlobalClass]
public partial class Area2DMusicTrigger : Area2D
{
    [Export]
    public AudioStreamInteractive Stream { get; set; }

    [Export]
    public MusicPriority MusicPriority { get; set; } = MusicPriority.Background;

    [Export]
    public string ClipName { get; set; }

    [Export(PropertyHint.Range, "0,1,0.01")]
    public float Volume { get; set; } = 1f;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (Stream is null)
        {
            MusicManager.Instance?.StopTrack(MusicPriority);
        }
        else
        {
            MusicManager.Instance?.PlayTrack(Stream, MusicPriority, ClipName, Volume);
        }
    }
}
