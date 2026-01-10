using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace CrossedDimensions.Saves;

/// <summary>
/// Responsible for creating, holding and persisting the active <see cref="SaveFile"/>.
/// Designed to be used as an autoload singleton.
/// </summary>
/// <remarks>
/// Save files are written to `user://saves/` by default. Manual saves are written
/// to `save_{SaveName}.tres` (where SaveName is the timestamp). Autosave is
/// written to `autosave.tres`.
/// </remarks>
public partial class SaveManager : Node
{
    /// <summary>
    /// Static instance used when this node is registered as an AutoLoad singleton.
    /// Set in <see cref="_Ready"/>.
    /// </summary>
    public static SaveManager Instance { get; private set; }

    /// <summary>
    /// The currently-active in-memory SaveFile. Call <see cref="CreateNewSave"/>
    /// or <see cref="ReadPersistent"/> to initialize this value.
    /// </summary>
    public SaveFile CurrentSave { get; private set; }

    private const int CurrentVersion = 1;
    private const string SaveFolder = "user://saves/";
    private const string AutosaveFilename = "autosave.tres";

    public override void _Ready()
    {
        Instance = this;
    }

    /// <summary>
    /// Create a new <see cref="SaveFile"/> with an automatic UTC timestamp
    /// friendly name. The returned resource is also stored in
    /// <see cref="CurrentSave"/>.
    /// </summary>
    /// <param name="isAutoSave">Whether this save will be treated as an autosave.</param>
    /// <param name="scenePath">Scene path to store.</param>
    public SaveFile CreateNewSave(bool isAutoSave = false, string scenePath = "")
    {
        var now = DateTime.UtcNow;
        var name = now.ToString("yyyy-MM-dd_HH-mm-ss");

        var save = new SaveFile();
        save.SaveName = name;
        save.Version = CurrentVersion;
        save.Timestamp = now.ToString("o");
        save.IsAutoSave = isAutoSave;
        save.ScenePath = scenePath;

        CurrentSave = save;
        return save;
    }

    /// <summary>
    /// Synchronously writes the provided save (or <see cref="CurrentSave"/>) to disk.
    /// Returns the path written on success. Throws on error.
    /// </summary>
    public string WritePersistent(SaveFile save = null)
    {
        if (save == null)
        {
            save = CurrentSave;
        }

        if (save == null)
        {
            throw new InvalidOperationException("No save provided.");
        }

        string path = save.IsAutoSave
            ? SaveFolder + AutosaveFilename
            : SaveFolder + $"save_{save.SaveName}.tres";

        // update timestamp
        save.Timestamp = DateTime.UtcNow.ToString("o");

        var err = ResourceSaver.Save(save, path);
        return path;
    }

    /// <summary>
    /// Load a SaveFile from disk. If <paramref name="path"/> is empty, load the autosave.
    /// On success sets <see cref="CurrentSave"/> and returns the loaded resource.
    /// </summary>
    public SaveFile ReadPersistent(string path = "")
    {
        string loadPath = string.IsNullOrEmpty(path)
            ? (SaveFolder + AutosaveFilename)
            : path;

        var loaded = ResourceLoader.Load(loadPath);
        if (loaded == null)
        {
            throw new Exception($"Failed to load SaveFile from '{loadPath}'.");
        }

        if (loaded is not SaveFile save)
        {
            throw new Exception($"Resource at '{loadPath}' is not a SaveFile.");
        }

        CurrentSave = save;
        return save;
    }

    /// <summary>
    /// Load a SaveFile by its SaveName.
    /// </summary>
    public SaveFile ReadPersistentFromName(string name)
    {
        string path = SaveFolder + $"save_{name}.tres";
        return ReadPersistent(path);
    }

    /// <summary>
    /// Store a Variant value into the current save's KeyValue store.
    /// </summary>
    public void SetKey(string key, Variant value)
    {
        if (CurrentSave is null)
        {
            throw new InvalidOperationException("No save loaded.");
        }

        CurrentSave.KeyValue[key] = value;
    }

    /// <summary>
    /// Retrieve a typed value from the save's KeyValue store. Throws if the
    /// key is missing or the stored value cannot be converted to
    /// <typeparamref name="T"/>.
    /// </summary>
    public T GetKey<T>(string key)
    {
        if (CurrentSave == null)
        {
            throw new InvalidOperationException("No save loaded.");
        }

        if (!CurrentSave.KeyValue.ContainsKey(key))
        {
            throw new KeyNotFoundException($"Key '{key}' not found in save KeyValue.");
        }

        return CurrentSave.KeyValue[key].As<T>();
    }

    /// <summary>
    /// Try to get a typed key. Returns false if missing or wrong type instead
    /// of throwing.
    /// </summary>
    public bool TryGetKey<T>(string key, out T value)
    {
        value = default;

        if (CurrentSave is null)
        {
            return false;
        }

        if (!CurrentSave.KeyValue.ContainsKey(key))
        {
            return false;
        }

        value = CurrentSave.KeyValue[key].As<T>();
        return true;
    }

    /// <summary>
    /// Get a typed key or return a provided default value if missing. Throws
    /// if present but wrong type.
    /// </summary>
    public T GetKeyOrDefault<T>(string key, T defaultValue)
    {
        if (CurrentSave is null)
        {
            throw new Exception();
        }

        if (!CurrentSave.KeyValue.ContainsKey(key))
        {
            return defaultValue;
        }

        return CurrentSave.KeyValue[key].As<T>();
    }
}
