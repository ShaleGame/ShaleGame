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

    private void DoAtMoment( int position, string method ) 
    {
        if (TimelineMoments.ContainsKey(position))
        {
            try
            {
                string path;
                GDScript script;
                GodotObject node;
                path = TimelineMoments[position];
                script = GD.Load<GDScript>(path);
                node = (GodotObject)script.New();
                node.Call(method);
            } 
            catch
            {
                GD.PrintErr($"Timeline moment {position} could not be loaded!");
            }
        }
    }
}
