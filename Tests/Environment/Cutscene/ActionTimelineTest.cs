using CrossedDimensions.Environment.Cutscene;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.Environment.Cutscene;

[TestSuite]
public partial class ActionTimelineTest
{
    [TestCase]
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
    public void EndTimeline_ShouldSetFinishedAndReset()
    {
        var timeline = new ActionTimeline
        {
            TimelineRunning = true,
            TimelinePosition = 3,
            TimelineFinished = false
        };

        timeline.EndTimeline();

        AssertThat(timeline.TimelineFinished).IsTrue();
        AssertThat(timeline.TimelinePosition).IsEqual(0);
        AssertThat(timeline.TimelineRunning).IsFalse();
    }
}
