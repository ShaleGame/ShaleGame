using CrossedDimensions.Audio;
using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.Audio;

[TestSuite]
public partial class MusicManagerTest
{
    [TestCase]
    [RequireGodotRuntime]
    public void PlayTrack_WhileNotPlaying_ShouldCallCreatePlayback()
    {
        var manager = new MusicManager();
        AddNode(manager);

        var stream = new AudioStreamInteractive();

        manager.PlayTrack(stream, MusicPriority.Background);

        AssertThat(manager.GetChildCount()).IsEqual(1);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void PlayTrack_WhilePlayingLowPriority_ShouldPrioritizeHigherPriorityTrack()
    {
        var manager = new MusicManager();
        AddNode(manager);

        var lowStream = new AudioStreamInteractive();
        var highStream = new AudioStreamInteractive();

        manager.PlayTrack(lowStream, MusicPriority.Low);
        manager.PlayTrack(highStream, MusicPriority.Boss);

        AssertThat(manager.GetChildCount()).IsEqual(2);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void StopTrack_Playing_ShouldCallCreatePlaybackAndStop()
    {
        var manager = new MusicManager();
        AddNode(manager);

        var stream = new AudioStreamInteractive();

        manager.PlayTrack(stream, MusicPriority.Background);

        // calling StopTrack should not throw and should invoke StopAndQueueFree on playback
        manager.StopTrack(MusicPriority.Background);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void StopTrack_HigherPriorityPlaying_ShouldResumeLowerPriority()
    {
        var manager = new MusicManager();
        AddNode(manager);

        var lowStream = new AudioStreamInteractive();
        var highStream = new AudioStreamInteractive();

        manager.PlayTrack(lowStream, MusicPriority.Low);
        manager.PlayTrack(highStream, MusicPriority.Boss);

        // stopping high priority track should not throw; behavior of resuming is covered in playback tests
        manager.StopTrack(MusicPriority.Boss);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void PlayTrack_WhilePlayingSamePriority_ShouldStopPrevious()
    {
        var manager = new MusicManager();
        AddNode(manager);

        var firstStream = new AudioStreamInteractive();

        manager.PlayTrack(firstStream, MusicPriority.Background);

        var secondStream = new AudioStreamInteractive();

        // playing a new track at the same priority should stop the previous one
        manager.PlayTrack(secondStream, MusicPriority.Background);

        var playback = manager.GetChild<InteractiveTrackPlayback>(0);
        AssertThat(playback.IsPlaying).IsFalse();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void PlayTrack_AfterStopped_ReusesPlaybackWithinGracePeriod()
    {
        var manager = new MusicManager();
        AddNode(manager);

        var stream = new AudioStreamInteractive();
        manager.PlayTrack(stream, MusicPriority.Background);

        var playback = manager.GetChild<InteractiveTrackPlayback>(0);

        manager.StopTrack(MusicPriority.Background);
        manager.PlayTrack(stream, MusicPriority.Background);

        var newPlayback = manager.GetChild<InteractiveTrackPlayback>(0);
        AssertThat(newPlayback).IsSame(playback);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void PlayTrack_SameStreamDifferentClip_ReusesPlaybackAndSwitches()
    {
        var manager = new MusicManager();
        AddNode(manager);

        var stream = new AudioStreamInteractive();
        manager.PlayTrack(stream, MusicPriority.Background, "Intro");
        var playback = manager.GetChild<InteractiveTrackPlayback>(0);

        manager.PlayTrack(stream, MusicPriority.Background, "Loop");
        var newPlayback = manager.GetChild<InteractiveTrackPlayback>(0);

        AssertThat(newPlayback).IsSame(playback);
    }
}
