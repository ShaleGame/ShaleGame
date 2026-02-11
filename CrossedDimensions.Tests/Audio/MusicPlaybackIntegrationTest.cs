using System.Linq;
using CrossedDimensions.Audio;
using Godot;

namespace CrossedDimensions.Tests.Audio;

[Collection("GodotHeadless")]
public class MusicPlaybackIntegrationTest : System.IDisposable
{
    private const string ScenePath = $"{Paths.TestPath}/Audio/MusicPlaybackIntegrationTestScene.tscn";

    private readonly GodotHeadlessFixedFpsFixture _godot;
    private Node _scene;

    public MusicPlaybackIntegrationTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;
        _scene = null;

        var packed = ResourceLoader.Load<PackedScene>(ScenePath);
        _scene = packed.Instantiate() as Node;
        _godot.Tree.Root.AddChild(_scene);
    }

    public void Dispose()
    {
        _scene?.QueueFree();
        _scene = null;
    }

    [Fact]
    public void MusicManagerPlayTrack_ShouldAddPlaybackNode()
    {
        var musicManager = _scene.GetNode<MusicManager>("MusicManager");
        var stream = new AudioStreamInteractive();

        musicManager.PlayTrack(stream, MusicPriority.Background);

        musicManager.GetChildCount().ShouldBeGreaterThan(0);
    }

    [Fact]
    public void MusicManagerPlayTrack_ShouldCreateAudioPlayersAndBePlaying()
    {
        var musicManager = _scene.GetNode<MusicManager>("MusicManager");
        var stream = new AudioStreamInteractive();

        musicManager.PlayTrack(stream, MusicPriority.Background);

        // simulate a few engine iterations to allow playback to initialize
        for (int i = 0; i < 4; i++)
            _godot.GodotInstance.Iteration(1);

        musicManager.GetChildCount().ShouldBeGreaterThan(0);

        var playback = (InteractiveTrackPlayback)musicManager.GetChild(0);
        playback.IsPlaying.ShouldBeTrue();
        playback.GetChildCount().ShouldBe(1);
    }

    [Fact]
    public void GivenStopTrack_WhenWithinGraceDuration_PlaybackShouldRemainAlive()
    {
        var musicManager = _scene.GetNode<MusicManager>("MusicManager");
        musicManager.GraceDuration = 1f;

        var stream = new AudioStreamInteractive();

        musicManager.PlayTrack(stream, MusicPriority.Background);

        // stop the track; it should remain alive for GraceDuration
        musicManager.StopTrack(MusicPriority.Background);

        // wait less than GraceDuration -> playback should still be alive
        _godot.GodotInstance.Iteration(1);

        musicManager.GetChildCount().ShouldBe(1);
    }

    [Fact]
    public void MusicManagerStopTrack_ShouldQueueFreePlaybackAfterFade()
    {
        var musicManager = _scene.GetNode<MusicManager>("MusicManager");
        musicManager.GraceDuration = 0.05f;

        var stream = new AudioStreamInteractive();

        musicManager.PlayTrack(stream, MusicPriority.Background);

        // stop the track; it should fade and queue_free after GraceDuration
        musicManager.StopTrack(MusicPriority.Background);

        // wait longer than GraceDuration -> playback should be freed
        for (int i = 0; i < 5; i++)
            _godot.GodotInstance.Iteration(1);

        musicManager.GetChildCount().ShouldBe(0);
    }

    [Fact]
    public void MusicManagerPriority_PlayingHigherPriorityShouldStopLower()
    {
        var musicManager = _scene.GetNode<MusicManager>("MusicManager");

        var lowStream = new AudioStreamInteractive();
        var highStream = new AudioStreamInteractive();

        musicManager.PlayTrack(lowStream, MusicPriority.Low);
        _godot.GodotInstance.Iteration(1);

        musicManager.PlayTrack(highStream, MusicPriority.Boss);
        _godot.GodotInstance.Iteration(1);

        var playbacks = musicManager.GetChildren()
            .Where((p) => p is InteractiveTrackPlayback)
            .Select((p) => p as InteractiveTrackPlayback);
        InteractiveTrackPlayback lowPlayback = playbacks
            .Where((p) => p.Stream == lowStream)
            .First();
        InteractiveTrackPlayback highPlayback = playbacks
            .Where((p) => p.Stream == highStream)
            .First();

        lowPlayback.IsPlaying.ShouldBeFalse();
        highPlayback.IsPlaying.ShouldBeTrue();
    }
}
