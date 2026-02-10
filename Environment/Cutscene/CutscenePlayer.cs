namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// Interface for handling cutscene triggers
/// </summary>


public partial class CutscenePlayer : ICutsceneHandler
{
    public bool SceneActive { get; set; }
    public ActionTimeline Timeline { get; set; }

    public void StartScene(ActionTimeline timeline)
    {
        
    }
    public void Process()
    {
        
    }
    public void EndScene()
    {
        
    }
}
