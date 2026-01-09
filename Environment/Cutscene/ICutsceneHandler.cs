namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// Interface for handling cutscene triggers
/// </summary>


public interface ICutsceneHandler
{
    bool SceneActive { get; set; }
    ActionTimeline Timeline { get; set; }

    void StartScene( ActionTimeline timeline );
    void Process();
    void EndScene();
}

