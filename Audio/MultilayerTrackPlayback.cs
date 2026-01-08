using System;
using Godot;

namespace CrossedDimensions.Audio;

/// <summary>
/// Concrete implementation of <see cref="IMultilayerTrackPlayback"/>.
/// Manages playback for an <see cref="IMultilayerTrack"/> by creating and
/// controlling one <see cref="AudioStreamPlayer"/> per audio layer. This
/// class handles starting and stopping playback, fading layers in and out,
/// reacting to changes in the track's <c>CurrentLayer</c>, and freeing the
/// playback node when it is no longer needed.
/// </summary>
public partial class MultilayerTrackPlayback : Node, IMultilayerTrackPlayback
{
    /// <summary>
    /// The multi-layered track that this playback instance controls. The
    /// implementation will create <see cref="AudioStreamPlayer"/> nodes based
    /// on <see cref="IMultilayerTrack.Tracks"/> and will observe
    /// <c>CurrentLayer</c> to determine which layers should be audible.
    /// </summary>
    public IMultilayerTrack Track { get; private set; }

    private int _currentLayer = 0;

    /// <summary>
    /// The current active layer. Changing the value will make it so only the
    /// layers at or below the current layer are playing. A negative value means
    /// that no layers are audible.
    /// </summary>
    public int CurrentLayer
    {
        get => _currentLayer;
        set
        {
            _currentLayer = Math.Clamp(value, -1, Track.Tracks.Length - 1);
        }
    }

    /// <summary>
    /// Determines whether the track is currently playing.
    /// </summary>
    public bool IsPlaying { get; private set; } = false;

    private AudioStreamPlayer[] _layerPlaybacks;

    /// <summary>
    /// The duration in seconds for fade-in and fade-out transitions, with a
    /// full fade from silent to full volume (or vice versa) taking this long.
    /// Partial volume changes will scale proportionally.
    /// </summary>
    [Export]
    public float FadeDuration { get; set; } = 1f;

    public override void _Process(double delta)
    {
        for (int i = 0; i < _layerPlaybacks.Length; i++)
        {
            var player = _layerPlaybacks[i];

            // target volume is 1 if playing and layer is at or below current layer
            float targetVolume = IsPlaying && i <= _currentLayer ? 1 : 0;

            // delta volume: 1s = 100% volume change
            // therefore we use delta / FadeDuration to get the fraction
            float change = (float)(delta / FadeDuration);
            player.VolumeLinear = Mathf.MoveToward(player.VolumeLinear, targetVolume, change);
        }
    }

    /// <summary>
    /// Starts playback of the track, fading all layers up to the current
    /// layer in.
    /// </summary>
    public void Play()
    {
        // create per-layer AudioStreamPlayers if they do not exist.
        if (_layerPlaybacks is null || _layerPlaybacks.Length == 0)
        {
            var tracks = Track?.Tracks;
            if (tracks is null || tracks.Length == 0)
            {
                return;
            }

            _layerPlaybacks = new AudioStreamPlayer[tracks.Length];

            for (int i = 0; i < tracks.Length; i++)
            {
                var player = new AudioStreamPlayer
                {
                    Stream = tracks[i],
                    VolumeDb = float.NegativeInfinity,
                    Autoplay = true,
                };
                AddChild(player);
                _layerPlaybacks[i] = player;
            }
        }

        foreach (var player in _layerPlaybacks)
        {
            if (!player.Playing)
            {
                player.Play();
            }
        }

        IsPlaying = true;
    }

    /// <summary>
    /// Stops playback of all layers, fading them out.
    /// </summary>
    public void Stop()
    {
        IsPlaying = false;
    }

    /// <summary>
    /// Stops playback of all layers with optional fade-out duration and frees
    /// this playback instance.
    /// </summary>
    public async void StopAndQueueFree()
    {
        Stop();
        await ToSignal(GetTree().CreateTimer(FadeDuration), "timeout");
        QueueFree();
    }

    /// <summary>
    /// Seeks all layers to the specified position in seconds.
    /// </summary>
    public void Seek(float position)
    {
        foreach (var player in _layerPlaybacks)
        {
            player.Seek(position);
        }
    }
}
