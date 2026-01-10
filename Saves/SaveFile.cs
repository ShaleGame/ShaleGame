using Godot;

namespace CrossedDimensions.Saves;

/// <summary>
/// Represents a persisted savefile for the game.
/// Stored as a <see cref="Resource"/> so it can be saved/loaded with
/// <see cref="Godot.ResourceSaver"/> and edited in-editor if necessary.
/// </summary>
public partial class SaveFile : Resource
{
    /// <summary>
    /// Friendly name for the save. Created automatically by the <see cref="SaveManager"/>
    /// using a UTC timestamp (format: yyyy-MM-dd_HH-mm-ss).
    /// </summary>
    [Export]
    public string SaveName { get; set; } = "";

    /// <summary>
    /// Schema version of the savefile structure. Increment when the save
    /// format changes after initial non-development release.
    /// </summary>
    [Export]
    public int Version { get; set; }= 1;

    /// <summary>
    /// ISO 8601 timestamp of when the save was created or last written.
    /// </summary>
    [Export]
    public string Timestamp { get; set; } = "";

    /// <summary>
    /// Whether this save represents an autosave. Autosaves are written to a
    /// known autosave path by convention.
    /// </summary>
    [Export]
    public bool IsAutoSave { get; set; } = false;

    /// <summary>
    /// The scene path (for example, "res://levels/room01.tscn") where the
    /// player created this save or the checkpoint associated with it.
    /// </summary>
    [Export]
    public string ScenePath { get; set; } = "";

    /// <summary>
    /// Generic key/value store for arbitrary game flags and small pieces of state.
    /// Keys are strings and values are Godot Variants (bool, int, float, string, etc.).
    /// Use a naming convention to help organize key names.
    /// </summary>
    [Export]
    public Godot.Collections.Dictionary KeyValue { get; } = new();
}
