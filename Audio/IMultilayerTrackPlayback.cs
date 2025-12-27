namespace CrossedDimensions.Audio;

public interface IMultilayerTrackPlayback
{
    public IMultilayerTrack Track { get; }

    /// <summary>
    /// Starts playing the multi-layered track from the beginning. All layers up to
    /// the current active layer will be audible.
    /// </summary>
    public void Play(float fadeIn = 0f);

    /// <summary>
    /// Stops playing the multi-layered track. All layers will cease playback.
    /// </summary>
    public void Stop(float fadeOut = 0f);

    /// <summary>
    /// Fades out the track over the specified duration and then frees the
    /// playback object.
    /// </summary>
    public void FadeOutAndQueueFree(float fadeOut = 0f);
}