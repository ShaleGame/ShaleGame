using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Godot;

namespace CrossedDimensions.Tests;

public class GodotHeadlessFixedFpsFixture : IDisposable
{
    // .NET's Environment.SetEnvironmentVariable does not propagate to native getenv()
    // on Linux/.NET 8+. We must call setenv directly for Godot's native code to see it.
    // On Windows, Environment.SetEnvironmentVariable works fine.
    [DllImport("libc", SetLastError = true)]
    private static extern int setenv(string name, string value, int overwrite);

    public twodog.Engine Engine { get; }

    public GodotInstance GodotInstance { get; }

    public SceneTree Tree => Engine.Tree;

    public GodotHeadlessFixedFpsFixture()
    {
        Console.WriteLine("Initializing Godot...");
        Console.WriteLine("cwd: " + System.Environment.CurrentDirectory);

        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? throw new InvalidOperationException("Could not determine assembly directory from GetExecutingAssembly().Location.");
        var projectPath = twodog.Engine.ResolveProjectDir();

        // Pre-load game assemblies into Default context BEFORE starting Godot.
        // This prevents type identity issues where Godot loads assemblies into
        // PluginLoadContext while test code expects them in Default context.
        var preloaderType = typeof(twodog.xunit.GodotHeadlessFixture).Assembly
            .GetType("twodog.xunit.AssemblyPreloader");
        var preloadMethod = preloaderType?.GetMethod("PreloadGameAssemblies", BindingFlags.Public | BindingFlags.Static);
        if (preloadMethod is null)
            Console.WriteLine("[GodotHeadlessFixedFpsFixture] Warning: Could not locate twodog.xunit.AssemblyPreloader.PreloadGameAssemblies via reflection. Game assemblies will not be pre-loaded.");
        else
            preloadMethod.Invoke(null, new object[] { projectPath });

        // Set GODOTSHARP_DIR so Godot finds GodotPlugins.dll in the output directory.
        // When running via dotnet test, the host process is /usr/share/dotnet/dotnet,
        // so Godot's exe_dir fallback resolves to the wrong directory.
        // On Linux/.NET 8+, must use native setenv() because .NET's SetEnvironmentVariable
        // doesn't propagate to native getenv(). On Windows, the .NET API works fine.
        if (File.Exists(Path.Combine(assemblyDir, "GodotPlugins.dll")))
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                System.Environment.SetEnvironmentVariable("GODOTSHARP_DIR", assemblyDir);
            else
                setenv("GODOTSHARP_DIR", assemblyDir, 1);
        }

        Console.WriteLine("Godot project: " + projectPath);
        Engine = new twodog.Engine("twodog.tests", projectPath, "--headless", "--fixed-fps", "60");
        GodotInstance = Engine.Start();
        Console.WriteLine("Godot initialized successfully.");

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

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Console.WriteLine("Shutting down Godot...");
        GodotInstance.Dispose();
        Engine.Dispose();
        Console.WriteLine("Godot shut down successfully.");
    }
}
