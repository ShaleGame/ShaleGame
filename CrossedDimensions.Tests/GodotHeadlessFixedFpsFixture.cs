namespace CrossedDimensions.Tests;

public class GodotHeadlessFixedFpsFixture : GodotHeadlessFixture
{
    public GodotHeadlessFixedFpsFixture() : base()
    {
        // Set max FPS after Godot has initialized
        Godot.Engine.Singleton.MaxFps = 60;
        Godot.Engine.PhysicsTicksPerSecond = 60;
        // Speed up animations used by tests (cutscenes, fades). Default to
        // doubling playback speed so tests run faster but still exercise
        // animation-driven flows.
        CrossedDimensions.Saves.SceneManager.CutscenePlaybackSpeed = 16.0;
        var overlay = CrossedDimensions.UI.ScreenOverlayManager.Instance;
        if (overlay is not null && Godot.GodotObject.IsInstanceValid(overlay))
        {
            overlay.FadePlaybackSpeed = 16.0;
        }
    }
}
