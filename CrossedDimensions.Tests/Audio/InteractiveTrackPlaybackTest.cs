using Godot;
using System;
using CrossedDimensions.Audio;

namespace CrossedDimensions.Tests.Audio;

[Collection("GodotHeadless")]
public class InteractiveTrackPlaybackTest(GodotHeadlessFixture godot)
{
    [Fact]
    public void Play_WhenNotPlaying_FadesInAudio()
    {
        var playback = new InteractiveTrackPlayback(new AudioStreamInteractive());
        godot.Tree.Root.AddChild(playback);

        playback.FadeDuration = 1f;
        playback.Play();

        var player = playback.GetChild<AudioStreamPlayer>(0);

        // simulate 1 second of processing -> move to 1.0 volume
        playback._Process(1.0);
        Math.Abs(player.VolumeLinear - 1f).ShouldBeLessThan(0.001f);
    }

    [Fact]
    public void Stop_WhenPlaying_FadesOutAudio()
    {
        var playback = new InteractiveTrackPlayback(new AudioStreamInteractive());
        godot.Tree.Root.AddChild(playback);

        playback.FadeDuration = 1f;
        playback.Play();
        playback._Process(1.0);

        var player = playback.GetChild<AudioStreamPlayer>(0);

        // now stop and simulate 1 second -> should move to 0
        playback.Stop();
        playback._Process(1.0);
        Math.Abs(player.VolumeLinear - 0f).ShouldBeLessThan(0.001f);
    }
}
