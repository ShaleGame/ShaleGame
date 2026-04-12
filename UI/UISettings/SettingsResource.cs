using Godot;

namespace CrossedDimensions.UI.UISettings;

/// <summary>
/// Global game settings that can be configured in-editor and at runtime.
/// Stored as a Resource for easy serialization and editor integration.
/// </summary>
public partial class SettingsResource : Resource
{
    private int _windowMode;

    /// <summary>
    /// Display mode setting index.
    /// 0 = Fullscreen, 1 = Windowed Fullscreen, 2 = Windowed.
    /// </summary>
    [Export]
    public int WindowMode
    {
        get => _windowMode;
        set => _windowMode = Mathf.Clamp(value, 0, 2);
    }

    /// <summary>
    /// Whether camera shake feedback is enabled.
    /// </summary>
    [Export]
    public bool ScreenShakeEnabled { get; set; } = true;

    /// <summary>
    /// Whether non-gameplay visual effects are enabled.
    /// </summary>
    [Export]
    public bool VisualEffectsEnabled { get; set; } = true;

    /// <summary>
    /// Whether 2D HDR rendering is enabled.
    /// </summary>
    [Export]
    public bool HdrEnabled { get; set; } = true;
}
