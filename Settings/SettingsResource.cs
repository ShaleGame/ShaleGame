using Godot;

namespace CrossedDimensions.Settings;

/// <summary>
/// Global game settings that can be configured in-editor and at runtime.
/// Stored as a Resource for easy serialization and editor integration.
/// </summary>
public partial class SettingsResource : Resource
{
    private float _masterVolume = 1.0f;
    private float _musicVolume = 1.0f;
    private float _sfxVolume = 1.0f;

    /// <summary>
    /// Master volume level (0.0 to 1.0)
    /// </summary>
    [Export]
    public float MasterVolume
    {
        get => _masterVolume;
        set => _masterVolume = Mathf.Clamp(value, 0.0f, 1.0f);
    }

    /// <summary>
    /// Music volume level (0.0 to 1.0)
    /// </summary>
    [Export]
    public float MusicVolume
    {
        get => _musicVolume;
        set => _musicVolume = Mathf.Clamp(value, 0.0f, 1.0f);
    }

    /// <summary>
    /// Sound effects volume level (0.0 to 1.0)
    /// </summary>
    [Export]
    public float SfxVolume
    {
        get => _sfxVolume;
        set => _sfxVolume = Mathf.Clamp(value, 0.0f, 1.0f);
    }

    /// <summary>
    /// Whether the game should run in fullscreen mode
    /// </summary>
    [Export]
    public bool Fullscreen { get; set; } = false;
}