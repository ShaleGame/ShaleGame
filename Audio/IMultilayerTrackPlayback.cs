namespace CrossedDimensions.Audio;

public interface IMultilayerTrackPlayback
{
    /// <summary>
    /// The <see cref="IMultilayerTrack"/> instance that this playback controls.
    /// Implementations should expose the track so callers can inspect metadata
    /// such as available layers.
    /// </summary>
    public IMultilayerTrack Track { get; }

    /// <summary>
    /// Starts playing the multi-layered track from the beginning. All layers up to
    /// the current active layer will be audible.
    /// </summary>
    public void Play();

    /// <summary>
    /// Stops playing the multi-layered track. All layers will cease playback.
    /// </summary>
    public void Stop();

    /// <summary>
    /// Fades out the track over the specified duration and then frees the
    /// playback object.
    /// </summary>
    public void StopAndQueueFree();
}
