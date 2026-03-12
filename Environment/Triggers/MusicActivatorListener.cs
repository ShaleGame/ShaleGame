using Godot;

namespace CrossedDimensions.Environment.Triggers;

[GlobalClass]
public partial class MusicActivatorListener : Node
{
    /// <summary>
    /// The activator to listen to for state changes.
    /// </summary>
    [Export]
    public Activator Activator { get; set; }

    /// <summary>
    /// The music stream to play when the activator is activated. If null,
    /// will stop music at this priority instead.
    /// </summary>
    [Export]
    public AudioStreamInteractive Stream { get; set; }

    /// <summary>
    /// The priority level for the music stream. Determines how it interacts with other
    /// active streams. Higher priority streams will pause or stop lower priority ones.
    /// </summary>
    [Export]
    public Audio.MusicPriority MusicPriority { get; set; } = Audio.MusicPriority.Background;

    [Export]
    public string ClipName { get; set; }

    [Export(PropertyHint.Range, "0,1,0.01")]
    public float Volume { get; set; } = 1f;

    private bool _previousState;

    public override void _Ready()
    {
        if (Activator == null)
        {
            GD.PushError($"ActivatorVisualListener '{Name}' requires an Activator to be set.");
            return;
        }

        Activator.Activated += OnActivated;
        Activator.Deactivated += OnDeactivated;

        // Set initial state
        _previousState = Activator.IsActivated;
        UpdateState(Activator.IsActivated, isInitial: true);
    }

    public override void _ExitTree()
    {
        if (Activator != null)
        {
            Activator.Activated -= OnActivated;
            Activator.Deactivated -= OnDeactivated;
        }

        base._ExitTree();
    }

    private void OnActivated()
    {
        UpdateState(true);
    }

    private void OnDeactivated()
    {
        UpdateState(false);
    }

    private void UpdateState(bool isActivated, bool isInitial = false)
    {
        if (isActivated == _previousState && isInitial)
        {
            return;
        }

        _previousState = isActivated;

        if (isActivated)
        {
            Audio.MusicManager.Instance?.PlayTrack(Stream, MusicPriority, ClipName, Volume);
        }
        else
        {
            Audio.MusicManager.Instance?.StopTrack(MusicPriority);
        }
    }
}
