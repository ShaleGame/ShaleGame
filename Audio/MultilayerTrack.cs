using Godot;

namespace CrossedDimensions.Audio;

/// <summary>
/// A Resource representing a multi-layered audio track. Layers are exposed
/// as exported <see cref="AudioStream" /> entries so they can be assigned in
/// the editor.
/// </summary>
[GlobalClass]
public partial class MultilayerTrack : Resource, IMultilayerTrack
{
    /// <summary>
    /// The individual audio streams that make up this multi-layer track.
    /// </summary>
    [Export]
    public AudioStream[] Tracks { get; set; } = new AudioStream[0];

    /// <summary>
    /// Creates a new playback instance for this track. The playback instance
    /// is initialized with this track so it can access the layers.
    /// </summary>
    public IMultilayerTrackPlayback CreatePlayback()
    {
        return new MultilayerTrackPlayback(this);
    }
}

