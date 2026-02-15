using Godot;

namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// Class for cutscene timelines
/// </summary>

public partial class ActionTimeline : Resource
{
    public int TimelinePosition { get; set; }
    public bool TimelineRunning { get; set; }
    public bool TimelineFinished { get; set; } = false;

    [Export]
    GDScript Timeline { get; set; }
    public string moment_name = "";

    public void _Process( double delta )
    {
        if (TimelineRunning) {
            ExecuteTimelineStep();   
        }
    }
    public void ExecuteTimelineStep()
    {
        if (TimelineRunning)
        {
            TimelinePosition++;
            moment_name = "moment_" + TimelinePosition.ToString();
            //format for moment method names is moment_####, where #### is the 'frame' it takes place at
            DoAtMoment(TimelinePosition, moment_name);
        }
    }

    public void StartTimeline()
    {
        TimelineRunning = true;
    }
    public void StopTimeline()
    {
        TimelineRunning = false;
    }

    public void ResetTimeline()
    {
        StopTimeline();
        TimelinePosition = 0;
    }

    public void EndTimeline()
    {
        ResetTimeline();
        TimelineFinished = true;
    }

    private void DoAtMoment(int moment, string method)
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
