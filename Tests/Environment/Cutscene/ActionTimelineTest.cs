using CrossedDimensions.Environment.Cutscene;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.Environment.Cutscene;

[TestSuite]
public partial class ActionTimelineTest
{
    [TestCase]
    [RequireGodotRuntime]
    public void StartTimeline_ShouldSetTimelineRunningTrue()
    {
        var timeline = new ActionTimeline
        {
            TimelineRunning = false
        };

        timeline.StartTimeline();

        AssertThat(timeline.TimelineRunning).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void StopTimeline_ShouldSetTimelineRunningFalse()
    {
        var timeline = new ActionTimeline
        {
            TimelineRunning = true
        };

        timeline.StopTimeline();

        AssertThat(timeline.TimelineRunning).IsFalse();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void ResetTimeline_ShouldResetTimelinePosition()
    {
        var timeline = new ActionTimeline
        {
            TimelineRunning = true,
            TimelinePosition = 5
        };

        timeline.ResetTimeline();

        AssertThat(timeline.TimelinePosition).IsEqual(0);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void ResetTimeline_ShouldStopTimeline()
    {
        var timeline = new ActionTimeline
        {
            TimelineRunning = true,
            TimelinePosition = 2
        };

        timeline.ResetTimeline();

        AssertThat(timeline.TimelineRunning).IsFalse();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void EndTimeline_ShouldReset()
    {
        var timeline = new ActionTimeline
        {
            TimelineRunning = true,
            TimelinePosition = 3,
        };

        timeline.EndTimeline();

        AssertThat(timeline.TimelinePosition).IsEqual(0);
        AssertThat(timeline.TimelineRunning).IsFalse();
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
