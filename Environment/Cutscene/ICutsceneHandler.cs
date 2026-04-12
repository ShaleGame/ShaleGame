using Godot;

namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// Defines the contract for cutscene playback handlers.
/// </summary>
public interface ICutsceneHandler
{
    bool SceneActive { get; set; }
    AnimationPlayer AnimationPlayer { get; set; }
    string AnimationName { get; set; }

    void StartScene(
        AnimationPlayer animationPlayer = null,
        string animationName = "");
    void EndScene();
}
