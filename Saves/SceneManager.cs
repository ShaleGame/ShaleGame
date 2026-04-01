using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrossedDimensions.Saves;
using CrossedDimensions.UI;
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

    public void LoadSceneSync(string scenePath, bool fade = true)
    {
        _ = LoadScene(scenePath, fade: fade);
    }

    /// <summary>
    /// Load a scene by path, using the cache when available.
    /// </summary>
    public async Task LoadScene(string scenePath, bool forceReload = false, bool fade = true)
    {
        await ChangeScene(scenePath, forceReload, fade);
    }

    public void LoadSceneFromSave(SaveFile save, bool fade = true)
    {
        _ = LoadSceneFromSaveAsync(save, fade);
    }

    public void ReloadCurrentSceneFromSave(SaveFile save)
    {
        _ = ReloadCurrentSceneFromSaveAsync(save);
    }

    /// <summary>
    /// Load the scene stored in <paramref name="save"/> and move the player to
    /// the saved position. If the saved scene is already active, only the player
    /// is moved.
    /// </summary>
    private async Task LoadSceneFromSaveAsync(SaveFile save, bool fade = true)
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

        await ChangeScene(
            scenePath,
            forceReload: true,
            fade: fade,
            onSceneReady: () => MovePlayer(position));
    }

    private async Task ReloadCurrentSceneFromSaveAsync(SaveFile save)
    {
        if (save == null)
        {
            GD.PushWarning("SceneManager.ReloadCurrentSceneFromSave: save is null.");
            return;
        }

        if (!save.TryGetKey<Vector2>("player_position", out var position))
        {
            GD.PushWarning("SceneManager.ReloadCurrentSceneFromSave: 'player_position' key not found.");
            return;
        }

        string currentScene = GetTree().CurrentScene?.SceneFilePath ?? "";
        if (string.IsNullOrEmpty(currentScene))
        {
            GD.PushWarning("SceneManager.ReloadCurrentSceneFromSave: current scene path is empty.");
            return;
        }

        await LoadScene(currentScene, fade: false);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        MovePlayer(position);
    }

    public void LoadSceneWithMarker(string scenePath, string markerName, bool fade = true)
    {
        _ = LoadSceneWithMarkerAsync(scenePath, markerName, fade);
    }

    public async Task LoadSceneWithMarkerAsync(string scenePath, string markerName, bool fade = true)
    {
        string currentScene = GetTree().CurrentScene?.SceneFilePath ?? "";

        Marker2D marker = null;

        if (currentScene != scenePath)
        {
            await ChangeScene(
                scenePath,
                forceReload: false,
                fade: fade,
                onSceneReady: () =>
                {
                    marker = FindSceneMarker(markerName);
                    if (marker is not null)
                    {
                        MovePlayer(marker.GlobalPosition);
                    }
                });
        }
        else
        {
            marker = FindSceneMarker(markerName);
            if (marker is not null)
            {
                MovePlayer(marker.GlobalPosition);
            }
        }

        if (marker == null)
        {
            string warning = "SceneManager.LoadSceneWithMarker: no marker " +
                $"named '{markerName}' found in group 'SceneMarkers'.";
            GD.PushWarning(warning);
            return;
        }

    }

    private async Task ChangeScene(
        string scenePath,
        bool forceReload,
        bool fade,
        Action onSceneReady = null)
    {
        if (string.IsNullOrEmpty(scenePath))
        {
            GD.PushWarning("SceneManager.LoadScene: scenePath is empty.");
            return;
        }

        PackedScene packed;

        if (forceReload || !_cache.TryGetValue(scenePath, out packed))
        {
            GD.Print($"SceneManager: loading scene '{scenePath}' from disk.");
            packed = ResourceLoader.Load<PackedScene>(scenePath);
            if (packed == null)
            {
                GD.PushError($"SceneManager.LoadScene: failed to load '{scenePath}'.");
                return;
            }
            _cache[scenePath] = packed;
        }
        else
        {
            GD.Print($"SceneManager: using cached scene '{scenePath}'.");
        }
        if (fade)
        {
            GetTree().Paused = true;
            await ScreenOverlayManager.Instance.FadeIn();
        }

        PreviousScene = GetTree().CurrentScene?.SceneFilePath ?? "";
        GetTree().ChangeSceneToPacked(packed);
        await ToSignal(GetTree(), SceneTree.SignalName.SceneChanged);

        if (fade)
        {
            GetTree().Paused = false;
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        onSceneReady?.Invoke();

        if (fade)
        {
            await ScreenOverlayManager.Instance.FadeOut();
        }
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

    public Vector2 GetPlayerPosition()
    {
        var player = GetTree()
            .GetNodesInGroup("Player")
            .OfType<Characters.Character>()
            .FirstOrDefault();

        return player?.GlobalPosition ?? Vector2.Zero;
    }

    private Marker2D FindSceneMarker(string markerName)
    {
        return GetTree()
            .GetNodesInGroup("SceneMarkers")
            .OfType<Marker2D>()
            .FirstOrDefault(m => m.Name == markerName);
    }
}
