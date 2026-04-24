using Godot;

namespace CrossedDimensions.UI.UISettings;

[GlobalClass]
public partial class SettingsManager : Node
{
    private const string SettingsPath = "user://settings.tres";

    public SettingsResource Current { get; private set; }

    public override void _Ready()
    {
        LoadOrCreateSettings();
        ApplyWindowMode(Current.WindowMode);
        ApplyHdr(Current.HdrEnabled);
    }

    public Error Save()
    {
        if (Current is null)
        {
            return Error.Failed;
        }

        return ResourceSaver.Save(Current, SettingsPath);
    }

    public void ApplyWindowMode(int modeIndex)
    {
        int clampedMode = Mathf.Clamp(modeIndex, 0, 2);
        DisplayServer.WindowSetMode(WindowModeToDisplayServerMode(clampedMode));
    }

    public bool IsScreenShakeEnabled()
    {
        return Current?.ScreenShakeEnabled ?? true;
    }

    public bool IsVisualEffectsEnabled()
    {
        return Current?.VisualEffectsEnabled ?? true;
    }

    public void ApplyHdr(bool enabled)
    {
        ProjectSettings.SetSetting("rendering/viewport/hdr_2d", enabled);
        if (GetTree() is not null)
        {
            GetTree().Root.UseHdr2D = enabled;
        }
    }

    private void LoadOrCreateSettings()
    {
        var loadedSettings = ResourceLoader.Load<SettingsResource>(SettingsPath);
        if (loadedSettings is not null)
        {
            Current = loadedSettings;
            return;
        }

        Current = new SettingsResource();
        Save();
    }

    private static DisplayServer.WindowMode WindowModeToDisplayServerMode(int modeIndex)
    {
        return modeIndex switch
        {
            0 => DisplayServer.WindowMode.ExclusiveFullscreen,
            1 => DisplayServer.WindowMode.Fullscreen,
            2 => DisplayServer.WindowMode.Windowed,
            _ => DisplayServer.WindowMode.ExclusiveFullscreen
        };
    }
}
