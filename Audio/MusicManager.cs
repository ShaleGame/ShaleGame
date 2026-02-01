using System;
using System.Collections.Generic;
using Godot;

namespace CrossedDimensions.Audio;

/// <summary>
/// Manages music playback across multiple priority levels. Tracks are added
/// per-priority and the manager ensures that only the highest-priority active
/// track is playing while lower-priority tracks are paused or stopped.
/// </summary>
[GlobalClass]
public partial class MusicManager : Node, IMusicManager
{
    public static MusicManager Instance { get; private set; }

    private readonly Dictionary<MusicPriority, InteractiveTrackPlayback> _activeTracks = new();
    private readonly Dictionary<MusicPriority, TrackCacheEntry> _trackCache = new();

    public override void _Ready()
    {
        Instance = this;
    }

    /// <summary>
    /// The duration to keep a stopped track alive for potential resumption.
    /// </summary>
    public double GraceDuration { get; set; } = 15;


    private sealed class TrackCacheEntry
    {
        public InteractiveTrackPlayback Playback { get; set; }
        public AudioStreamInteractive Stream { get; set; }
        public string ClipName { get; set; }
    }

    /// <summary>
    /// Starts playing the specified <paramref name="stream"/> at the given
    /// <paramref name="priority"/>. If a stream is already present at that
    /// priority, it will be stopped before the new one is added.
    /// </summary>
    /// <param name="stream">The interactive stream to play.</param>
    /// <param name="priority">The priority level for the stream.</param>
    /// <param name="clipName">Optional clip name to start from.</param>
    public void PlayTrack(
        AudioStreamInteractive stream,
        MusicPriority priority,
        string clipName = null,
        float volume = 1f)
    {
        // put onto priority list
        if (_activeTracks.ContainsKey(priority))
        {
            // stop and remove existing track at this priority
            _activeTracks[priority].StopAndQueueFree();
        }

        InteractiveTrackPlayback playback = null;

        if (stream is not null)
        {
            playback = GetOrCreatePlayback(priority, stream, clipName);
            playback.SetVolume(volume);
        }

        _activeTracks[priority] = playback;
        UpdateActiveTrack();
    }

    /// <summary>
    /// Stops and removes the track assigned to the specified <paramref name="priority"/>.
    /// The track will be asked to fade out and queue itself for freeing.
    /// </summary>
    /// <param name="priority">The priority level whose track should be stopped.</param>
    public void StopTrack(MusicPriority priority)
    {
        // remove from priority list
        if (_activeTracks.ContainsKey(priority))
        {
            _activeTracks[priority].StopAndQueueFree();
            _activeTracks.Remove(priority);
        }
        UpdateActiveTrack();
    }

    /// <summary>
    /// Evaluates the active tracks and ensures only the highest-priority track is
    /// currently playing while lower-priority tracks are stopped.
    /// </summary>
    private void UpdateActiveTrack()
    {
        // Find highest priority track and ensure only it is playing
        MusicPriority? highestPriority = null;

        // Get the highest priority that has an active track
        foreach (var priority in Enum.GetValues<MusicPriority>())
        {
            if (_activeTracks.ContainsKey(priority))
            {
                if ((int?)highestPriority == null || (int)priority > (int)highestPriority)
                {
                    highestPriority = priority;
                }
            }
        }

        if (highestPriority.HasValue)
        {
            // play the highest priority track
            _activeTracks[highestPriority.Value].Play();

            // stop all lower priority tracks
            foreach (var priority in Enum.GetValues<MusicPriority>())
            {
                if (priority < highestPriority.Value && _activeTracks.ContainsKey(priority))
                {
                    _activeTracks[priority].Stop();
                }
            }
        }
    }

    private InteractiveTrackPlayback GetOrCreatePlayback(
        MusicPriority priority,
        AudioStreamInteractive stream,
        string clipName)
    {
        if (_trackCache.TryGetValue(priority, out var cached)
            && cached.Playback is not null
            && IsInstanceValid(cached.Playback)
            && cached.Stream == stream)
        {
            cached.ClipName = clipName;
            cached.Playback.GraceDuration = GraceDuration;
            cached.Playback.SetClipName(clipName);
            return cached.Playback;
        }

        if (cached?.Playback is not null && IsInstanceValid(cached.Playback))
        {
            cached.Playback.StopAndQueueFree();
        }

        var playback = new InteractiveTrackPlayback(stream, clipName);
        playback.GraceDuration = GraceDuration;
        AddChild(playback);
        _trackCache[priority] = new TrackCacheEntry
        {
            Playback = playback,
            Stream = stream,
            ClipName = clipName,
        };
        return playback;
    }
}
