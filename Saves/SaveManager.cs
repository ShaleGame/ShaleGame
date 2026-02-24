using System;
using Godot;
using Godot.Collections;

namespace CrossedDimensions.Saves;

/// <summary>
/// Responsible for creating, holding and persisting the active <see cref="SaveFile"/>.
/// Designed to be used as an autoload singleton.
/// </summary>
/// <remarks>
/// Save files are written to `user://saves/` by default. Manual saves are written
/// to `save_{SaveName}.tres` (where SaveName is the timestamp).
/// </remarks>
[GlobalClass]
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
    [Export]
    public SaveFile CurrentSave { get; private set; }

    private const int CurrentVersion = 1;
    private const string SaveFolder = "user://saves/";
    private const string DeveloperSaveName = "developer";
    private const string DeveloperDefaultPath = "res://Assets/Saves/default-developer-save.tres";

    /// <summary>
    /// Get the expected path for a save name.
    /// </summary>
    public static string SavePathForName(string name)
    {
        return SaveFolder + $"save_{name}.tres";
    }

    public override void _Ready()
    {
        Instance = this;

        if (OS.IsDebugBuild())
        {
            CurrentSave = LoadDeveloperSave();
        }
    }

    /// <summary>
    /// Create a new <see cref="SaveFile"/> with an automatic UTC timestamp
    /// friendly name. The returned resource is also stored in
    /// <see cref="CurrentSave"/>.
    /// </summary>
    /// <param name="scenePath">Scene path to store.</param>
    public SaveFile CreateNewSave(string scenePath = "")
    {
        var now = DateTime.UtcNow;
        var name = now.ToString("yyyy-MM-dd_HH-mm-ss");

        var save = new SaveFile();
        save.SaveName = name;
        save.Version = CurrentVersion;
        save.Timestamp = now.ToString("o");
        save.ScenePath = scenePath;

        CurrentSave = save;
        return save;
    }

    [Obsolete("Autosaves are no longer used. Use CreateNewSave(scenePath) instead.")]
    public SaveFile CreateNewSave(bool isAutoSave, string scenePath = "")
    {
        var save = CreateNewSave(scenePath);
        save.IsAutoSave = isAutoSave;
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

        // ensure save directory exists
        DirAccess.MakeDirRecursiveAbsolute(SaveFolder);

        string path = SavePathForName(save.SaveName);

        // update timestamp
        save.Timestamp = DateTime.UtcNow.ToString("o");

        var err = ResourceSaver.Save(save, path);

        GD.Print($"Saving to '{path}' with result: {err}");
        return path;
    }

    /// <summary>
    /// Load a SaveFile from disk.
    /// On success sets <see cref="CurrentSave"/> and returns the loaded resource.
    /// </summary>
    /// <param name="path">Path to the save file to load.</param>
    public SaveFile ReadPersistent(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Path cannot be empty. Provide a path to the save file.", nameof(path));
        }

        var loaded = ResourceLoader.Load(path);
        if (loaded == null)
        {
            throw new Exception($"Failed to load SaveFile from '{path}'.");
        }

        if (loaded is not SaveFile save)
        {
            throw new Exception($"Resource at '{path}' is not a SaveFile.");
        }

        CurrentSave = save;
        return save;
    }

    /// <summary>
    /// Load a SaveFile by its SaveName.
    /// </summary>
    public SaveFile ReadPersistentFromName(string name)
    {
        string path = SavePathForName(name);
        return ReadPersistent(path);
    }

    public SaveFile LoadDeveloperSave()
    {
        string devPath = SavePathForName(DeveloperSaveName);

        try
        {
            if (FileAccess.FileExists(devPath))
            {
                return ReadPersistentFromName(DeveloperSaveName);
            }
            else
            {
                GD.Print($"Developer save not found at '{devPath}'. Falling back to default.");
            }
        }
        catch (Exception e)
        {
            GD.Print($"Unable to load developer save: {e.Message}");
        }

        GD.Print($"Loading developer default save from '{DeveloperDefaultPath}'.");

        var defaultResource = ResourceLoader.Load(DeveloperDefaultPath);
        if (defaultResource is not SaveFile template)
        {
            throw new Exception($"Developer default save not found at '{DeveloperDefaultPath}'.");
        }

        return (SaveFile)template.Duplicate();
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

        CurrentSave.SetKey(key, value);
    }

    /// <summary>
    /// Retrieve a typed value from the save's KeyValue store. Throws if the
    /// key is missing or the stored value cannot be converted to
    /// <typeparamref name="T"/>.
    /// </summary>
    public T GetKey<[MustBeVariant] T>(string key)
    {
        if (CurrentSave == null)
        {
            throw new InvalidOperationException("No save loaded.");
        }

        return CurrentSave.GetKey<T>(key);
    }

    /// <summary>
    /// Try to get a typed key. Returns false if missing or wrong type instead
    /// of throwing.
    /// </summary>
    public bool TryGetKey<[MustBeVariant] T>(string key, out T value)
    {
        value = default;

        if (CurrentSave is null)
        {
            return false;
        }

        return CurrentSave.TryGetKey<T>(key, out value);
    }

    /// <summary>
    /// Get a typed key or return a provided default value if missing. Throws
    /// if present but wrong type.
    /// </summary>
    public T GetKeyOrDefault<[MustBeVariant] T>(string key, T defaultValue)
    {
        if (CurrentSave is null)
        {
            throw new Exception();
        }

        return CurrentSave.GetKeyOrDefault<T>(key, defaultValue);
    }

    /// <summary>
    /// Lists all savefiles in the save directory.
    /// </summary>
    /// <returns>A list of <see cref="SaveFile" /> resources.</returns>
    public Godot.Collections.Array<SaveFile> ListAllSaves()
    {
        var saves = new Godot.Collections.Array<SaveFile>();

        var dir = DirAccess.Open(SaveFolder);

        dir.ListDirBegin();

        while (true)
        {
            string fileName = dir.GetNext();
            if (string.IsNullOrEmpty(fileName))
            {
                break;
            }

            if (fileName.EndsWith(".tres") && fileName.StartsWith("save_"))
            {
                string path = SaveFolder + fileName;
                var loaded = ResourceLoader.Load(path);
                if (loaded is SaveFile save)
                {
                    saves.Add(save);
                }
            }
        }

        return saves;
    }
}
