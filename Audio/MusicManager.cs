using System;
using System.Collections.Generic;
using Godot;

namespace CrossedDimensions.Audio;

/// <summary>
/// Manages music playback across multiple priority levels. Tracks are added
/// per-priority and the manager ensures that only the highest-priority active
/// track is playing while lower-priority tracks are paused or stopped.
/// </summary>
public partial class MusicManager : Node, IMusicManager
{
    private readonly Dictionary<MusicPriority, IMultilayerTrackPlayback> _activeTracks = new();

    /// <summary>
    /// Starts playing the specified <paramref name="track"/> at the given
    /// <paramref name="priority"/>. If a track is already present at that
    /// priority, it will be stopped before the new track is added.
    /// </summary>
    /// <param name="track">The multi-layer track to play.</param>
    /// <param name="priority">The priority level for the track.</param>
    public void PlayTrack(IMultilayerTrack track, MusicPriority priority)
    {
        // put onto priority list
        if (_activeTracks.ContainsKey(priority))
        {
            _activeTracks[priority].Stop();
        }

        var playback = (MultilayerTrackPlayback)track.CreatePlayback();
        AddChild(playback);
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
}
