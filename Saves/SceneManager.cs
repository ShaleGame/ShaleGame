using System.Collections.Generic;
using System.Threading.Tasks;
using CrossedDimensions.Saves;
using Godot;
using System.Linq;

namespace CrossedDimensions.Saves;

/// <summary>
/// Singleton responsible for loading and transitioning between scenes.
/// Maintains a packed-scene cache to avoid reloading from disk on repeated visits.
/// </summary>
[GlobalClass]
public partial class SceneManager : Node
{
    public static SceneManager Instance { get; private set; }

    private readonly Dictionary<string, PackedScene> _cache = new();

    public string PreviousScene { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }
    public void LoadSceneSync(string scenePath)
    {
        _ = LoadScene(scenePath);
    }
    /// <summary>
    /// Load a scene by path, using the cache when available.
    /// </summary>
    public async Task LoadScene(string scenePath)
    {
        if (string.IsNullOrEmpty(scenePath))
        {
            GD.PushWarning("SceneManager.LoadScene: scenePath is empty.");
            return;
        }

        if (!_cache.TryGetValue(scenePath, out var packed))
        {
            packed = ResourceLoader.Load<PackedScene>(scenePath);
            if (packed == null)
            {
                GD.PushError($"SceneManager.LoadScene: failed to load '{scenePath}'.");
                return;
            }
            _cache[scenePath] = packed;
        }
        PreviousScene = GetTree().CurrentScene?.SceneFilePath ?? "";
        GetTree().ChangeSceneToPacked(packed);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    public void LoadSceneFromSave(SaveFile save)
    {
        _ = LoadSceneFromSaveAsync(save);
    }

    /// <summary>
    /// Load the scene stored in <paramref name="save"/> and move the player to
    /// the saved position. If the saved scene is already active, only the player
    /// is moved.
    /// </summary>
    private async Task LoadSceneFromSaveAsync(SaveFile save)
    {
        if (save == null)
        {
            GD.PushWarning("SceneManager.LoadSceneFromSave: save is null.");
            return;
        }

        if (!save.TryGetKey<string>("player_scene", out var scenePath))
        {
            GD.PushWarning("SceneManager.LoadSceneFromSave: 'player_scene' key not found.");
            GD.Print(save.KeyValue);
            return;
        }

        if (!save.TryGetKey<Vector2>("player_position", out var position))
        {
            GD.PushWarning("SceneManager.LoadSceneFromSave: 'player_position' key not found.");
            return;
        }

        string currentScene = GetTree().CurrentScene?.SceneFilePath ?? "";

        if (currentScene != scenePath)
        {
            await LoadScene(scenePath);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        MovePlayer(position);
    }

    private void MovePlayer(Vector2 position)
    {
        var players = GetTree()
            .GetNodesInGroup("Player")
            .OfType<Characters.Character>()
            .ToList();

        if (players.Count == 0)
        {
            GD.PushWarning("SceneManager: no node found in group 'Player'.");
            return;
        }

        GD.Print($"Moving {players.Count} player(s) to position {position}.");
        foreach (var player in players)
        {
            player.GlobalPosition = position;
        }
    }

    public void LoadSceneWithMarker(string scenePath, string markerName)
    {
        _ = LoadSceneWithMarkerAsync(scenePath, markerName);
    }

    public async Task LoadSceneWithMarkerAsync(string scenePath, string markerName)
    {
        string currentScene = GetTree().CurrentScene?.SceneFilePath ?? "";

        if (currentScene != scenePath)
        {
            await LoadScene(scenePath);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        var marker = GetTree()
            .GetNodesInGroup("SceneMarkers")
            .OfType<Marker2D>()
            .FirstOrDefault(m => m.Name == markerName);

        if (marker == null)
        {
            string warning = "SceneManager.LoadSceneWithMarker: no marker " +
                $"named '{markerName}' found in group 'SceneMarkers'.";
            GD.PushWarning(warning);
            return;
        }

        MovePlayer(marker.GlobalPosition);
    }
}
