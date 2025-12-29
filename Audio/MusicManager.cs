using System;
using System.Collections.Generic;
using Godot;

namespace CrossedDimensions.Audio;

public partial class MusicManager : Node, IMusicManager
{
    private readonly Dictionary<MusicPriority, IMultilayerTrackPlayback> _activeTracks = new();

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
