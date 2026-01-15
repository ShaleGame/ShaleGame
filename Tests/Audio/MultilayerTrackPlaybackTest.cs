using CrossedDimensions.Audio;
using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Audio.Tests;

[TestSuite]
public partial class MultilayerTrackPlaybackTest
{
    private class SimpleTrack : IMultilayerTrack
    {
        public AudioStream[] Tracks { get; } = new AudioStream[] { new AudioStream() };
        public IMultilayerTrackPlayback CreatePlayback() => new MultilayerTrackPlayback(this);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Play_WhenNotPlaying_FadesInAudio()
    {
        var playback = new MultilayerTrackPlayback(new SimpleTrack());
        AddNode(playback);

        // ensure playback created its players when Play is called
        playback.FadeDuration = 1f;
        playback.CurrentLayer = 0;
        playback.Play();

        var player = playback.GetChild<AudioStreamPlayer>(0);

        // simulate 1 second of processing -> move to 1.0 volume
        playback._Process(1.0);
        AssertThat(Mathf.IsEqualApprox(player.VolumeLinear, 1f)).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Stop_WhenPlaying_FadesOutAudio()
    {
        var playback = new MultilayerTrackPlayback(new SimpleTrack());
        AddNode(playback);

        playback.FadeDuration = 1f;
        playback.CurrentLayer = 0;
        playback.Play();
        playback._Process(1.0);

        var player = playback.GetChild<AudioStreamPlayer>(0);

        // now stop and simulate 1 second -> should move to 0
        playback.Stop();
        playback._Process(1.0);
        AssertThat(Mathf.IsEqualApprox(player.VolumeLinear, 0f)).IsTrue();
    }
}
