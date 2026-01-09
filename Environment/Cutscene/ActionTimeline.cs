using Godot;

namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// Class for cutscene timelines
/// </summary>

public partial class ActionTimeline : Resource
{
    int TimelinePosition { get; set; }
    bool TimelineRunning { get; set; }
    [Export]
    GDScript Timeline { get; set; }

    public void ExecuteTimelineStep() 
    {
        if (TimelineRunning) {
            //format for moment method names is moment_####, where #### is the 'frame' it takes place at
            DoAtMoment( TimelinePosition, "moment_" + TimelinePosition.ToString() );
            TimelinePosition++;
        }
    }

    public void ResetTimeline() 
    {
        TimelineRunning = false;
        TimelinePosition = 0;
    }

    private void DoAtMoment( int moment, string method ) 
    {
        //if the moment exists, attempt to find and execute the associated GDScript
        if (Timeline.HasMethod(method))
        {
            GodotObject obj = (GodotObject)Timeline.New();
            try
            {
                obj.Call(method);
                obj.Free();   
            }
            catch
            {
                GD.PrintErr($"Timeline moment {moment} could not be loaded!");
            }
        }
    }
}
