using System.Collections.Generic;
using Godot;

namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// Class for cutscene timelines
/// </summary>

public class ActionTimeline 
{
    int TimelinePosition { get; set; }
    bool TimelineRunning { get; set; }
    //dictionary contains key (timeline frame at which to execute) and string (path to GDScript)
    //are we able to link GDScripts directly in the editor (which would be more ideal) if dictionary type is <int, GDscript>?
    Dictionary<int, string> TimelineMoments { get; set; }

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
                string path = TimelineMoments[moment];
                GDScript script = GD.Load<GDScript>(path);
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
