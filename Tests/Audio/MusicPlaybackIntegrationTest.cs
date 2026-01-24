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
        MultilayerTrack track = new();
        track.Tracks = new AudioStream[] { new AudioStream() };

        musicManager.PlayTrack(track, MusicPriority.Background);

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
        MultilayerTrack track = new();
        track.Tracks = new AudioStream[] { new AudioStream(), new AudioStream() };

        musicManager.PlayTrack(track, MusicPriority.Background);
        await runner.SimulateFrames(4, 250);

        AssertThat(musicManager.GetChildCount()).IsGreater(0);

        // the first child should be the playback node added by the manager
        var playback = (MultilayerTrackPlayback)musicManager.GetChild(0);
        AssertThat(playback.IsPlaying).IsTrue();
        // playback should have created per-layer AudioStreamPlayers as children
        AssertThat(playback.GetChildCount()).IsEqual(track.Tracks.Length);
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task MusicManagerStopTrack_ShouldQueueFreePlaybackAfterFade()
    {
        var runner = ISceneRunner
            .Load("res://Tests/Audio/MusicPlaybackIntegrationTestScene.tscn");

        var scene = runner.Scene();

        var musicManager = scene.GetNode<MusicManager>("MusicManager");
        MultilayerTrack track = new();
        track.Tracks = new AudioStream[] { new AudioStream() };

        musicManager.PlayTrack(track, MusicPriority.Background);
        await runner.SimulateFrames(4, 250);

        // stop the track; it should fade and queue_free after FadeDuration (default 1s)
        musicManager.StopTrack(MusicPriority.Background);
        // wait longer than FadeDuration to allow the playback to free itself
        await runner.SimulateFrames(4, 250);

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

        var lowTrack = new MultilayerTrack();
        lowTrack.Tracks = new AudioStream[] { new AudioStream() };

        var highTrack = new MultilayerTrack();
        highTrack.Tracks = new AudioStream[] { new AudioStream() };

        musicManager.PlayTrack(lowTrack, MusicPriority.Low);
        await runner.SimulateFrames(4, 250);

        musicManager.PlayTrack(highTrack, MusicPriority.Boss);
        await runner.SimulateFrames(4, 250);

        // find playback instances for each track and assert their playing states
        var playbacks = musicManager.GetChildren()
            .Where((p) => p is MultilayerTrackPlayback)
            .Select((p) => p as MultilayerTrackPlayback);
        MultilayerTrackPlayback lowPlayback = playbacks
            .Where((p) => p.Track == lowTrack)
            .First();
        MultilayerTrackPlayback highPlayback = playbacks
            .Where((p) => p.Track == highTrack)
            .First();

        AssertThat(lowPlayback.IsPlaying).IsFalse();
        AssertThat(highPlayback.IsPlaying).IsTrue();
    }
}
