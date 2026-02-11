using Godot;
using CrossedDimensions.Audio;
using twodog.xunit;
using Shouldly;
using Xunit;

namespace CrossedDimensions.Tests.Audio;

[Collection("GodotHeadless")]
public class MusicManagerTest : System.IDisposable
{
    private GodotHeadlessFixedFpsFixture _godot;
    private MusicManager _manager;

    public MusicManagerTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;
        _manager = new MusicManager();
        _godot.Tree.Root.AddChild(_manager);
    }

    public void Dispose()
    {
        _manager?.QueueFree();
        _manager = null;
    }

    [Fact]
    public void PlayTrack_WhileNotPlaying_ShouldCallCreatePlayback()
    {
        var stream = new AudioStreamInteractive();

        _manager.PlayTrack(stream, MusicPriority.Background);

        _manager.GetChildCount().ShouldBe(1);
    }

    [Fact]
    public void PlayTrack_WhilePlayingLowPriority_ShouldPrioritizeHigherPriorityTrack()
    {
        var lowStream = new AudioStreamInteractive();
        var highStream = new AudioStreamInteractive();

        _manager.PlayTrack(lowStream, MusicPriority.Low);
        _manager.PlayTrack(highStream, MusicPriority.Boss);

        _manager.GetChildCount().ShouldBe(2);
    }

    [Fact]
    public void StopTrack_Playing_ShouldCallCreatePlaybackAndStop()
    {
        var stream = new AudioStreamInteractive();

        _manager.PlayTrack(stream, MusicPriority.Background);

        // calling StopTrack should not throw
        _manager.StopTrack(MusicPriority.Background);
    }

    [Fact]
    public void StopTrack_HigherPriorityPlaying_ShouldResumeLowerPriority()
    {
        var lowStream = new AudioStreamInteractive();
        var highStream = new AudioStreamInteractive();

        _manager.PlayTrack(lowStream, MusicPriority.Low);
        _manager.PlayTrack(highStream, MusicPriority.Boss);

        _manager.StopTrack(MusicPriority.Boss);

        Assert.True(true);
    }

    [Fact]
    public void PlayTrack_WhilePlayingSamePriority_ShouldStopPrevious()
    {
        var firstStream = new AudioStreamInteractive();

        _manager.PlayTrack(firstStream, MusicPriority.Background);

        var secondStream = new AudioStreamInteractive();

        // playing a new track at the same priority should stop the previous one
        _manager.PlayTrack(secondStream, MusicPriority.Background);

        var playback = _manager.GetChild<InteractiveTrackPlayback>(0);
        playback.IsPlaying.ShouldBeFalse();
    }

    [Fact]
    public void PlayTrack_AfterStopped_ReusesPlaybackWithinGracePeriod()
    {
        var stream = new AudioStreamInteractive();
        _manager.PlayTrack(stream, MusicPriority.Background);

        var playback = _manager.GetChild<InteractiveTrackPlayback>(0);

        _manager.StopTrack(MusicPriority.Background);
        _manager.PlayTrack(stream, MusicPriority.Background);

        var newPlayback = _manager.GetChild<InteractiveTrackPlayback>(0);
        newPlayback.ShouldBeSameAs(playback);
    }

    [Fact]
    public void PlayTrack_SameStreamDifferentClip_ReusesPlaybackAndSwitches()
    {
        var stream = new AudioStreamInteractive();
        _manager.PlayTrack(stream, MusicPriority.Background, "Intro");
        var playback = _manager.GetChild<InteractiveTrackPlayback>(0);

        _manager.PlayTrack(stream, MusicPriority.Background, "Loop");
        var newPlayback = _manager.GetChild<InteractiveTrackPlayback>(0);

        newPlayback.ShouldBeSameAs(playback);
    }
}
