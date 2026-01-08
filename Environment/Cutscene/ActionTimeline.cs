using Godot;

namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// Class for cutscene timelines
/// </summary>

public partial class ActionTimeline : Node
{
    int TimelinePosition { get; set; }
    bool TimelineRunning { get; set; }
    //dictionary contains key (timeline frame at which to execute) and GDScript that contains the function moment_execute()
    [Export(PropertyHint.DictionaryType)]
    public Godot.Collections.Dictionary<int, GDScript> TimelineMoments { get; set; }

    public void ExecuteTimelineStep() 
    {
        if (TimelineRunning) {
            DoAtMoment( TimelinePosition, "moment_execute" );
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
        if (TimelineMoments.ContainsKey(moment))
        {
            try
            {
                GDScript script = TimelineMoments[moment];
                GodotObject obj = (GodotObject)script.New();
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
