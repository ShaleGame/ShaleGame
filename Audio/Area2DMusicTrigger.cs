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

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (Stream is null)
        {
            return;
        }

        MusicManager.Instance?.Play(Stream, MusicPriority, ClipName);
    }
}
