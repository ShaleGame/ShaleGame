using System;
using System.Collections.Generic;

namespace CrossedDimensions.Audio;

public class MusicManager : IMusicManager
{
    private readonly Dictionary<MusicPriority, IMultilayerTrackPlayback> _activeTracks = new();

    private IMultilayerTrackPlayback GeneratePlayback(IMultilayerTrack track)
    {
        throw new NotImplementedException();
    }

    public void PlayTrack(IMultilayerTrack track, MusicPriority priority)
    {
        // put onto priority list
        if (_activeTracks.ContainsKey(priority))
        {
            _activeTracks[priority].Stop();
        }

        _activeTracks[priority] = GeneratePlayback(track);
        UpdateActiveTrack();
    }

    public void StopTrack(MusicPriority priority)
    {
        // remove from priority list
        if (_activeTracks.ContainsKey(priority))
        {
            _activeTracks[priority].FadeOutAndQueueFree();
            _activeTracks.Remove(priority);
        }
        UpdateActiveTrack();
    }

    private void UpdateActiveTrack()
    {
        // find highest priority track. if it is playing, do nothing. if not,
        // start playing it.
        foreach (var priority in Enum.GetValues<MusicPriority>())
        {
            if (_activeTracks.TryGetValue(priority, out var track))
            {
                // start playing this track
                track.Play();
            }

            // stop lower priority tracks
            foreach (var lowerPriority in Enum.GetValues<MusicPriority>())
            {
                if (lowerPriority < priority && _activeTracks.ContainsKey(lowerPriority))
                {
                    _activeTracks[lowerPriority].Stop();
                }
            }
        }
    }
}
