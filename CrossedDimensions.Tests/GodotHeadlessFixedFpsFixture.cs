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

        // If the ScreenOverlayManager singleton has been readied, also bump
        // its fade playback speed. Use the singleton Instance property which
        // is set in ScreenOverlayManager._Ready(). If it's not available yet,
        // tests can still set it later in their setup.
        try
        {
            var overlay = CrossedDimensions.UI.ScreenOverlayManager.Instance;
            if (overlay is not null && Godot.GodotObject.IsInstanceValid(overlay))
            {
                overlay.FadePlaybackSpeed = 16.0;
            }
        }
        catch
        {
            // Ignore any issues here; it's only an opportunistic speedup.
        }
    }
}
