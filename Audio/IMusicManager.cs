using Godot;

namespace CrossedDimensions.Audio;

/// <summary>
/// Interface for managing the game music. The music manager should handle
/// smooth transitions when a higher-priority track starts or ends.
/// </summary>
public interface IMusicManager
{
    /// <summary>
    /// Starts playing the interactive stream at the specified priority level.
    /// </summary>
    /// <param name="stream">The interactive music stream to play.</param>
    /// <param name="priority">The priority level for the stream.</param>
    /// <param name="clipName">Optional clip name to start from.</param>
    public void PlayTrack(
        AudioStreamInteractive stream,
        MusicPriority priority,
        string clipName = null);

    /// <summary>
    /// Stops playing the track on the specified priority level.
    /// </summary>
    /// <param name="priority">The priority level to stop a track on.</param>
    public void StopTrack(MusicPriority priority);
}
