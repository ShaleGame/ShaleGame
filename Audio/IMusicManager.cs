namespace CrossedDimensions.Audio;

/// <summary>
/// Interface for managing the game music. The music manager should handle
/// smooth transitions when a higher-priority track starts or ends.
/// </summary>
public interface IMusicManager
{
    /// <summary>
    /// Starts playing the multi-layered track at the specified priority level.
    /// </summary>
    public void PlayTrack(IMultilayerTrack track, MusicPriority priority);

    /// <summary>
    /// Stops playing the track on the specified priority level.
    /// </summary>
    public void StopTrack(MusicPriority priority);
}
