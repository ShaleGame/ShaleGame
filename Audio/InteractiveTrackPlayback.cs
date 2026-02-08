using System;
using Godot;

namespace CrossedDimensions.Audio;

/// <summary>
/// Playback wrapper for a single <see cref="AudioStreamInteractive"/> stream.
/// Handles fade-in/out and optional clip selection by name.
/// </summary>
public partial class InteractiveTrackPlayback : Node
{
    /// <summary>
    /// The interactive stream resource to play.
    /// </summary>
    public AudioStreamInteractive Stream { get; private set; }

    /// <summary>
    /// Optional clip name to start from when playback begins.
    /// </summary>
    public string ClipName { get; private set; }

    /// <summary>
    /// Determines whether the track is currently intended to be audible.
    /// </summary>
    public bool IsPlaying { get; private set; }

    /// <summary>
    /// The duration in seconds for fade-in and fade-out transitions.
    /// </summary>
    [Export]
    public double FadeDuration { get; set; } = 3.0;

    /// <summary>
    /// The grace period in seconds to keep the playback alive after stopping.
    /// Allows resuming without recreating the playback instance.
    /// </summary>
    [Export]
    public double GraceDuration { get; set; }

    private AudioStreamPlayer _player;
    private AudioStreamInteractive _streamInstance;
    private int _stopToken;
    private bool _pauseWhenSilent;
    private float _volumeTarget = 1f;

    /// <summary>
    /// Default constructor required by Godot for deserialization.
    /// </summary>
    public InteractiveTrackPlayback()
    {
        Stream = null;
        ClipName = null;
    }

    /// <summary>
    /// Creates a new playback instance bound to the provided stream and clip.
    /// </summary>
    public InteractiveTrackPlayback(AudioStreamInteractive stream, string clipName = null)
    {
        Stream = stream;
        ClipName = clipName;
    }

    public override void _Process(double delta)
    {
        if (_player is null)
        {
            return;
        }

        float targetVolume = IsPlaying ? _volumeTarget : 0f;
        float change = (float)(delta / FadeDuration);
        _player.VolumeLinear = Mathf.MoveToward(_player.VolumeLinear, targetVolume, change);

        if (!IsPlaying && _pauseWhenSilent && Mathf.IsEqualApprox(_player.VolumeLinear, 0f))
        {
            _player.StreamPaused = true;
        }
    }

    /// <summary>
    /// Starts playback of the interactive stream, fading in to full volume.
    /// </summary>
    public void Play()
    {
        if (Stream is null)
        {
            return;
        }

        _stopToken++;
        EnsurePlayer();
        if (_streamInstance is null)
        {
            _streamInstance = Stream;
            _player.Stream = _streamInstance;
        }

        _player.StreamPaused = false;
        _pauseWhenSilent = false;
        if (!_player.Playing)
        {
            _player.Play();
        }

        if (!string.IsNullOrWhiteSpace(ClipName))
        {
            SwitchClip(ClipName);
        }

        IsPlaying = true;
    }

    /// <summary>
    /// The target linear volume multiplier (0..1). The player will fade toward
    /// this value over <see cref="FadeDuration"/> when playing.
    /// </summary>
    public float TargetVolume
    {
        get => _volumeTarget;
        set => _volumeTarget = Mathf.Clamp(value, 0f, 1f);
    }

    /// <summary>
    /// Backwards-compatible setter.
    /// </summary>
    public void SetVolume(float volume) => TargetVolume = volume;

    /// <summary>
    /// Updates the current clip name and switches playback if possible.
    /// </summary>
    public void SetClipName(string clipName)
    {
        ClipName = clipName;
        if (_streamInstance is null || string.IsNullOrWhiteSpace(clipName))
        {
            return;
        }

        SwitchClip(clipName);
    }

    /// <summary>
    /// Fades out the track but leaves the player alive for potential resume.
    /// </summary>
    public void Stop()
    {
        IsPlaying = false;
        _pauseWhenSilent = true;
    }

    /// <summary>
    /// Fades out the track and queues the node for freeing.
    /// </summary>
    public async void StopAndQueueFree()
    {
        Stop();
        int token = ++_stopToken;
        await ToSignal(GetTree().CreateTimer(GraceDuration), "timeout");
        if (token != _stopToken || IsPlaying)
        {
            return;
        }

        _player?.Stop();
        QueueFree();
    }

    private void EnsurePlayer()
    {
        if (_player is not null)
        {
            return;
        }

        _player = new AudioStreamPlayer
        {
            Autoplay = false,
            VolumeDb = float.NegativeInfinity,
        };
        AddChild(_player);
    }

    private static int FindClipIndex(AudioStreamInteractive stream, string clipName)
    {
        int clipCount = stream.ClipCount;
        for (int i = 0; i < clipCount; i++)
        {
            var name = stream.GetClipName(i);
            if (string.Equals(name.ToString(), clipName, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }

    private void SwitchClip(string clipName)
    {
        if (_player is null)
        {
            return;
        }

        var playback = _player.GetStreamPlayback() as AudioStreamPlaybackInteractive;
        if (playback is null)
        {
            return;
        }

        int clipIndex = FindClipIndex(_streamInstance, clipName);
        if (clipIndex < 0)
        {
            return;
        }

        if (playback.GetCurrentClipIndex() == clipIndex)
        {
            return;
        }

        playback.SwitchToClip(clipIndex);
    }
}
