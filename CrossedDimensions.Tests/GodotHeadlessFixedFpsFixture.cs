namespace CrossedDimensions.Tests;

public class GodotHeadlessFixedFpsFixture : GodotHeadlessFixture
{
    public GodotHeadlessFixedFpsFixture() : base()
    {
        // Set max FPS after Godot has initialized
        Godot.Engine.Singleton.MaxFps = 60;
        Godot.Engine.PhysicsTicksPerSecond = 60;
    }
}
