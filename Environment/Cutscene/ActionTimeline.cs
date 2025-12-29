namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// Class for cutscene timelines
/// </summary>

public class ActionTimeline 
{
    int TimelinePosition { get; set; }
    bool TimelineRunning { get; set; }

    public void ExecuteTimelineStep() 
    {
        if (TimelineRunning) {
            TimelinePosition++;
            DoAtPosition( TimelinePosition );
        }
    }

    public void ResetTimeline() 
    {
        TimelineRunning = false;
        TimelinePosition = 0;
    }

    private void DoAtPosition( int position ) 
    {
        switch (position) {
            //not really sure how to handle "execute an arbitrary block of code", might need help
            default:
                break;
        }
    }
}
