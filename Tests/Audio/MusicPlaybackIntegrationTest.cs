using System.Linq;
using System.Threading.Tasks;
using CrossedDimensions.Audio;
using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.Audio;

[TestSuite]
[TestCategory("Integration")]
public partial class MusicPlaybackIntegrationTest
{
    [TestCase]
    [RequireGodotRuntime]
    public void MusicManagerPlayTrack_ShouldAddPlaybackNode()
    {
        var runner = ISceneRunner
            .Load("res://Tests/Audio/MusicPlaybackIntegrationTestScene.tscn");

        var scene = runner.Scene();

        var musicManager = scene.GetNode<MusicManager>("MusicManager");
        var stream = new AudioStreamInteractive();

        musicManager.PlayTrack(stream, MusicPriority.Background);

        AssertThat(musicManager.GetChildCount()).IsGreater(0);
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task MusicManagerPlayTrack_ShouldCreateAudioPlayersAndBePlaying()
    {
        var runner = ISceneRunner
            .Load("res://Tests/Audio/MusicPlaybackIntegrationTestScene.tscn");

        var scene = runner.Scene();

        var musicManager = scene.GetNode<MusicManager>("MusicManager");
        var stream = new AudioStreamInteractive();

        musicManager.PlayTrack(stream, MusicPriority.Background);
        await runner.SimulateFrames(4, 250);

        AssertThat(musicManager.GetChildCount()).IsGreater(0);

        // the first child should be the playback node added by the manager
        var playback = (InteractiveTrackPlayback)musicManager.GetChild(0);
        AssertThat(playback.IsPlaying).IsTrue();
        // playback should have created a single AudioStreamPlayer as a child
        AssertThat(playback.GetChildCount()).IsEqual(1);
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task GivenStopTrack_WhenWithinGraceDuration_PlaybackShouldRemainAlive()
    {
        var runner = ISceneRunner
            .Load("res://Tests/Audio/MusicPlaybackIntegrationTestScene.tscn");

        var scene = runner.Scene();

        var musicManager = scene.GetNode<MusicManager>("MusicManager");
        musicManager.GraceDuration = 1f;

        var stream = new AudioStreamInteractive();

        musicManager.PlayTrack(stream, MusicPriority.Background);

        // stop the track; it should remain alive for GraceDuration
        musicManager.StopTrack(MusicPriority.Background);

        // wait less than GraceDuration -> playback should still be alive
        await runner.SimulateFrames(1);

        AssertThat(musicManager.GetChildCount()).IsEqual(1);
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task MusicManagerStopTrack_ShouldQueueFreePlaybackAfterFade()
    {
        var runner = ISceneRunner
            .Load("res://Tests/Audio/MusicPlaybackIntegrationTestScene.tscn");

        var scene = runner.Scene();

        var musicManager = scene.GetNode<MusicManager>("MusicManager");
        musicManager.GraceDuration = 0.05f;

        var stream = new AudioStreamInteractive();

        musicManager.PlayTrack(stream, MusicPriority.Background);

        // stop the track; it should fade and queue_free after GraceDuration
        musicManager.StopTrack(MusicPriority.Background);

        // wait longer than GraceDuration -> playback should be freed
        await runner.SimulateFrames(2, 50);
        AssertThat(musicManager.GetChildCount()).IsEqual(0);
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task MusicManagerPriority_PlayingHigherPriorityShouldStopLower()
    {
        var runner = ISceneRunner
            .Load("res://Tests/Audio/MusicPlaybackIntegrationTestScene.tscn");

        var scene = runner.Scene();

        var musicManager = scene.GetNode<MusicManager>("MusicManager");

        var lowStream = new AudioStreamInteractive();
        var highStream = new AudioStreamInteractive();

        musicManager.PlayTrack(lowStream, MusicPriority.Low);
        await runner.SimulateFrames(1);

        musicManager.PlayTrack(highStream, MusicPriority.Boss);
        await runner.SimulateFrames(1);

        // find playback instances for each track and assert their playing states
        var playbacks = musicManager.GetChildren()
            .Where((p) => p is InteractiveTrackPlayback)
            .Select((p) => p as InteractiveTrackPlayback);
        InteractiveTrackPlayback lowPlayback = playbacks
            .Where((p) => p.Stream == lowStream)
            .First();
        InteractiveTrackPlayback highPlayback = playbacks
            .Where((p) => p.Stream == highStream)
            .First();

        AssertThat(lowPlayback.IsPlaying).IsFalse();
        AssertThat(highPlayback.IsPlaying).IsTrue();
    }
}
