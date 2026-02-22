using System;
using System.Collections.Generic;
using Godot;

namespace CrossedDimensions.Saves;

/// <summary>
/// Represents a persisted savefile for the game.
/// Stored as a <see cref="Resource"/> so it can be saved/loaded with
/// <see cref="Godot.ResourceSaver"/> and edited in-editor if necessary.
/// </summary>
[GlobalClass]
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
    public int Version { get; set; } = 1;

    /// <summary>
    /// ISO 8601 timestamp of when the save was created or last written.
    /// </summary>
    [Export]
    public string Timestamp { get; set; } = "";

    /// <summary>
    /// Whether this save represents an autosave. Autosaves are written to a
    /// known autosave path by convention.
    /// </summary>
    /// <remarks>
    /// <b>Deprecated.</b> Autosaves are no longer used. Only manual saves are supported.
    /// </remarks>
    [Obsolete("Autosaves are no longer used. Only manual saves are supported.")]
    [Export]
    public bool IsAutoSave { get; set; } = false;

    /// <summary>
    /// The scene path (for example, "res://levels/room01.tscn") where the
    /// player created this save or the checkpoint associated with it.
    /// </summary>
    [Export(PropertyHint.File, "*.tscn")]
    public string ScenePath { get; set; } = "";

    /// <summary>
    /// Generic key/value store for arbitrary game flags and small pieces of state.
    /// Keys are strings and values are Godot Variants (bool, int, float, string, etc.).
    /// Use a naming convention to help organize key names.
    /// </summary>
    [Export]
    public Godot.Collections.Dictionary KeyValue { get; set; } = new();

    /// <summary>
    /// Store a Variant value into the KeyValue store.
    /// </summary>
    public void SetKey(string key, Variant value)
    {
        KeyValue[key] = value;
    }

    /// <summary>
    /// Retrieve a typed value from the KeyValue store. Throws if the key is
    /// missing or the stored value cannot be converted to
    /// <typeparamref name="T"/>.
    /// </summary>
    public T GetKey<[MustBeVariant] T>(string key)
    {
        if (!KeyValue.ContainsKey(key))
        {
            throw new KeyNotFoundException($"Key '{key}' not found in save KeyValue.");
        }

        return KeyValue[key].As<T>();
    }

    /// <summary>
    /// Try to get a typed key. Returns false if missing or wrong type instead
    /// of throwing.
    /// </summary>
    public bool TryGetKey<[MustBeVariant] T>(string key, out T value)
    {
        value = default;

        if (!KeyValue.ContainsKey(key))
        {
            return false;
        }

        value = KeyValue[key].As<T>();
        return true;
    }

    /// <summary>
    /// Get a typed key or return a provided default value if missing. Throws
    /// if present but wrong type.
    /// </summary>
    public T GetKeyOrDefault<[MustBeVariant] T>(string key, T defaultValue)
    {
        if (!KeyValue.ContainsKey(key))
        {
            return defaultValue;
        }

        return KeyValue[key].As<T>();
    }
}
