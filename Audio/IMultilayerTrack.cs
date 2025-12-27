using Godot;

namespace CrossedDimensions.Audio;

/// <summary>
/// Represents an audio track composed of multiple layers that can be
/// selectively activated. Allows dynamic control over which layers are audible
/// based on the current active layer setting.
/// </summary>
public interface IMultilayerTrack
{
    /// <summary>
    /// Individual audio layers that make up this multi-layered track.
    /// </summary>
    public AudioStream[] Tracks { get; }
    
    /// <summary>
    /// The current active layer. Changing the value will make it so only the
    /// layers at or below the current layer are playing.
    /// </summary>
    public int CurrentLayer { get; set; }
}
