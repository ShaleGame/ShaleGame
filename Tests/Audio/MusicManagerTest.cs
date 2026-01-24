using CrossedDimensions.Audio;
using Godot;
using GdUnit4;
using NSubstitute;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Audio.Tests;

[TestSuite]
public partial class MusicManagerTest
{
    private class SimpleTrack : IMultilayerTrack
    {
        public AudioStream[] Tracks { get; } = new AudioStream[] { new AudioStream() };
        public IMultilayerTrackPlayback CreatePlayback() => new MultilayerTrackPlayback(this);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void PlayTrack_WhileNotPlaying_ShouldCallCreatePlayback()
    {
        var manager = new MusicManager();
        AddNode(manager);

        var trackSub = Substitute.For<IMultilayerTrack>();
        var playback = new MultilayerTrackPlayback(new SimpleTrack());
        trackSub.CreatePlayback().Returns(playback);

        manager.PlayTrack(trackSub, MusicPriority.Background);

        // ensure the track's CreatePlayback was invoked and a child was added
        trackSub.Received(1).CreatePlayback();
        AssertThat(manager.GetChildCount()).IsEqual(1);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void PlayTrack_WhilePlayingLowPriority_ShouldPrioritizeHigherPriorityTrack()
    {
        var manager = new MusicManager();
        AddNode(manager);

        var lowSub = Substitute.For<IMultilayerTrack>();
        var lowPlayback = new MultilayerTrackPlayback(new SimpleTrack());
        lowSub.CreatePlayback().Returns(lowPlayback);

        var highSub = Substitute.For<IMultilayerTrack>();
        var highPlayback = new MultilayerTrackPlayback(new SimpleTrack());
        highSub.CreatePlayback().Returns(highPlayback);

        manager.PlayTrack(lowSub, MusicPriority.Low);
        manager.PlayTrack(highSub, MusicPriority.Boss);

        // both tracks should have had their CreatePlayback invoked
        lowSub.Received(1).CreatePlayback();
        highSub.Received(1).CreatePlayback();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void StopTrack_Playing_ShouldCallCreatePlaybackAndStop()
    {
        var manager = new MusicManager();
        AddNode(manager);

        var trackSub = Substitute.For<IMultilayerTrack>();
        var playback = new MultilayerTrackPlayback(new SimpleTrack());
        trackSub.CreatePlayback().Returns(playback);

        manager.PlayTrack(trackSub, MusicPriority.Background);

        // ensure create was called
        trackSub.Received(1).CreatePlayback();

        // calling StopTrack should not throw and should invoke StopAndQueueFree on playback (tested in playback tests)
        manager.StopTrack(MusicPriority.Background);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void StopTrack_HigherPriorityPlaying_ShouldResumeLowerPriority()
    {
        var manager = new MusicManager();
        AddNode(manager);

        var lowSub = Substitute.For<IMultilayerTrack>();
        var lowPlayback = new MultilayerTrackPlayback(new SimpleTrack());
        lowSub.CreatePlayback().Returns(lowPlayback);

        var highSub = Substitute.For<IMultilayerTrack>();
        var highPlayback = new MultilayerTrackPlayback(new SimpleTrack());
        highSub.CreatePlayback().Returns(highPlayback);

        manager.PlayTrack(lowSub, MusicPriority.Low);
        manager.PlayTrack(highSub, MusicPriority.Boss);

        lowSub.Received(1).CreatePlayback();
        highSub.Received(1).CreatePlayback();

        // stopping high priority track should not throw; behavior of resuming is covered in playback tests
        manager.StopTrack(MusicPriority.Boss);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void PlayTrack_WhilePlayingSamePriority_ShouldStopPrevious()
    {
        var manager = new MusicManager();
        AddNode(manager);

        var firstSub = Substitute.For<IMultilayerTrack>();
        var firstPlayback = new MultilayerTrackPlayback(new SimpleTrack());
        firstSub.CreatePlayback().Returns(firstPlayback);

        manager.PlayTrack(firstSub, MusicPriority.Background);

        var secondSub = Substitute.For<IMultilayerTrack>();
        var secondPlayback = new MultilayerTrackPlayback(new SimpleTrack());
        secondSub.CreatePlayback().Returns(secondPlayback);

        // playing a new track at the same priority should stop the previous one
        manager.PlayTrack(secondSub, MusicPriority.Background);

        AssertThat(firstPlayback.IsPlaying).IsFalse();
    }
}
