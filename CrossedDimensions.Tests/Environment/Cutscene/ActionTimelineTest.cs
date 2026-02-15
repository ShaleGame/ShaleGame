using CrossedDimensions.Environment.Cutscene;
using Shouldly;
using Xunit;

namespace CrossedDimensions.Tests.Environment.Cutscene;

[Collection("GodotHeadless")]
public class ActionTimelineTest
{
    public ActionTimelineTest(GodotHeadlessFixedFpsFixture godot)
    {

    }

    [Fact]
    public void StartTimeline_ShouldSetTimelineRunningTrue()
    {
        var timeline = new ActionTimeline
        {
            TimelineRunning = false
        };

        timeline.StartTimeline();

        timeline.TimelineRunning.ShouldBeTrue();
    }

    [Fact]
    public void StopTimeline_ShouldSetTimelineRunningFalse()
    {
        var timeline = new ActionTimeline
        {
            TimelineRunning = true
        };

        timeline.StopTimeline();

        timeline.TimelineRunning.ShouldBeFalse();
    }

    [Fact]
    public void ResetTimeline_ShouldResetTimelinePosition()
    {
        var timeline = new ActionTimeline
        {
            TimelineRunning = true,
            TimelinePosition = 5
        };

        timeline.ResetTimeline();

        timeline.TimelinePosition.ShouldBe(0);
    }

    [Fact]
    public void ResetTimeline_ShouldStopTimeline()
    {
        var timeline = new ActionTimeline
        {
            TimelineRunning = true,
            TimelinePosition = 2
        };

        timeline.ResetTimeline();

        timeline.TimelineRunning.ShouldBeFalse();
    }

    [Fact]
    public void EndTimeline_ShouldSetFinishedAndReset()
    {
        var timeline = new ActionTimeline
        {
            TimelineRunning = true,
            TimelinePosition = 3,
        };

        timeline.EndTimeline();

        timeline.TimelineFinished.ShouldBeTrue();
        timeline.TimelinePosition.ShouldBe(0);
        timeline.TimelineRunning.ShouldBeFalse();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void EndTimeline_ShouldEmitFinishedSignal()
    {
        var timeline = new ActionTimeline
        {
            TimelineRunning = true,
            TimelinePosition = 3,
            TimelineFinished = false,
        };

        bool fired = false;
        timeline.Finished += () => fired = true;

        timeline.EndTimeline();

        AssertThat(fired).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void TimelineRunning_ShouldAdvanceTimelineCounter()
    {
        var timeline = new ActionTimeline
        {
            TimelineRunning = false,
            TimelinePosition = 0,
        };
        timeline.StartTimeline();

        //simulate waiting frames
        timeline._Process(0.4);

        AssertThat(timeline.TimelinePosition).IsNotEqual(0);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void TimelineRunning_ShouldExecuteTimelineStep()
    {
        var timeline = new ActionTimeline
        {
            TimelineRunning = true,
            TimelinePosition = 0
        };
        //simulate waiting frames
        timeline._Process(0.1);

        AssertThat(timeline.TimelinePosition).IsNotEqual(0);
        AssertThat(timeline.moment_name).IsEqual("moment_" + timeline.TimelinePosition);
    }
}
