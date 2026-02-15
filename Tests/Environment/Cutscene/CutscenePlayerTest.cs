using CrossedDimensions.Environment.Cutscene;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.Environment.Cutscene;

[TestSuite]
public partial class CutscenePlayerTest
{
    [TestCase]
    [RequireGodotRuntime]
    public void StartScene_ShouldSetSceneActiveTrue()
    {
        var _sceneplayer = new CutscenePlayer
        {
            SceneActive = false
        };

        var _scenetimeline = new ActionTimeline();

        _sceneplayer.StartScene(_scenetimeline);

        AssertThat(_sceneplayer.SceneActive).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void StartScene_ShouldSendSceneStartSignal()
    {
        var _sceneplayer = new CutscenePlayer
        {
            SceneActive = false
        };

        bool fired = false;
        _sceneplayer.StartingScene += () => fired = true;

        var _scenetimeline = new ActionTimeline();

        _sceneplayer.StartScene(_scenetimeline);

        AssertThat(fired).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void EndScene_ShouldSetSceneActiveFalse()
    {
        var _sceneplayer = new CutscenePlayer
        {
            SceneActive = true
        };

        _sceneplayer.EndScene();
        AssertThat(_sceneplayer.SceneActive).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void EndScene_ShouldSendSceneEndSignal()
    {
        var _sceneplayer = new CutscenePlayer
        {
            SceneActive = true
        };

        bool fired = false;
        _sceneplayer.EndingScene += () => fired = true;

        var _scenetimeline = new ActionTimeline();

        _sceneplayer.EndScene();

        AssertThat(fired).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void StartScene_ShouldSetTimelineResource()
    {
        var _sceneplayer = new CutscenePlayer
        {
            SceneActive = false
        };

        var _scenetimeline = new ActionTimeline();

        _sceneplayer.StartScene(_scenetimeline);

        AssertThat(_sceneplayer.Timeline).IsEqual(_scenetimeline);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void StartScene_ShouldStartTimeline()
    {
        var _sceneplayer = new CutscenePlayer
        {
            SceneActive = false
        };

        var _scenetimeline = new ActionTimeline();

        _sceneplayer.StartScene(_scenetimeline);

        //simulate waiting frames
        _sceneplayer._Process(0.1);

        AssertThat(_sceneplayer.Timeline.TimelineRunning).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void EndScene_ShouldEndTimeline()
    {
        var _sceneplayer = new CutscenePlayer
        {
            SceneActive = true
        };

        var _scenetimeline = new ActionTimeline
        {
            TimelineRunning = true
        };

        _sceneplayer.Timeline = _scenetimeline;

        _sceneplayer.EndScene();

        //simulate waiting frames
        _sceneplayer._Process(0.1);

        AssertThat(_sceneplayer.Timeline.TimelineRunning).IsFalse();
    }
}