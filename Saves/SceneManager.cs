using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrossedDimensions.Environment.Cutscene;
using CrossedDimensions.Environment.Triggers;
using CrossedDimensions.UI;
using Godot;

namespace CrossedDimensions.Saves;

/// <summary>
/// Singleton responsible for loading and transitioning between scenes.
/// Maintains a packed-scene cache to avoid reloading from disk on repeated visits.
/// </summary>
[GlobalClass]
public partial class SceneManager : Node
{
    [Signal]
    public delegate void CutsceneLoadedEventHandler(string scenePath);

    [Signal]
    public delegate void GameplayResumedEventHandler(string scenePath);

    public static SceneManager Instance { get; private set; }

    private readonly Dictionary<string, PackedScene> _cache = new();

    public string PreviousScene { get; private set; }

    public Node SuspendedScene { get; private set; }

    public Node ActiveCutsceneScene { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public void LoadSceneSync(string scenePath, bool fade = true)
    {
        _ = LoadScene(scenePath, fade: fade);
    }

    public void PlayCutsceneSync(CutsceneMetadata metadata, bool fade = true)
    {
        _ = PlayCutscene(metadata, fade);
    }

    /// <summary>
    /// Load a scene by path, using the cache when available.
    /// </summary>
    public async Task LoadScene(string scenePath, bool forceReload = false, bool fade = true)
    {
        await ChangeScene(scenePath, forceReload, fade);
    }

    public async Task PlayCutscene(CutsceneMetadata metadata, bool fade = true)
    {
        if (ActiveCutsceneScene is not null && !GodotObject.IsInstanceValid(ActiveCutsceneScene))
        {
            ActiveCutsceneScene = null;
        }

        if (SuspendedScene is not null && !GodotObject.IsInstanceValid(SuspendedScene))
        {
            SuspendedScene = null;
        }

        if (metadata is null)
        {
            GD.PushWarning("SceneManager.PlayCutscene: metadata is null.");
            return;
        }

        if (ActiveCutsceneScene is not null)
        {
            GD.PushWarning("SceneManager.PlayCutscene: a cutscene is already active.");
            return;
        }

        var currentScene = GetTree().CurrentScene;
        if (currentScene is null)
        {
            GD.PushWarning("SceneManager.PlayCutscene: current scene is null.");
            return;
        }

        var shouldFade = fade && ScreenOverlayManager.Instance is not null;
        if (shouldFade)
        {
            GetTree().Paused = true;
            await ScreenOverlayManager.Instance.FadeIn();
        }

        PreviousScene = currentScene.SceneFilePath ?? "";
        SuspendedScene = currentScene;

        var packed = GetPackedScene(metadata.CutsceneScenePath, forceReload: false);
        if (packed is null)
        {
            SuspendedScene = null;
            if (shouldFade)
            {
                GetTree().Paused = false;
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                await ScreenOverlayManager.Instance.FadeOut();
            }
            return;
        }

        GetTree().CurrentScene = null;
        GetTree().Root.RemoveChild(currentScene);

        var cutsceneScene = packed.Instantiate<Node>();
        ActiveCutsceneScene = cutsceneScene;

        GetTree().Root.AddChild(cutsceneScene);
        GetTree().CurrentScene = cutsceneScene;
        EmitSignal(SignalName.CutsceneLoaded, metadata.CutsceneScenePath);

        if (shouldFade)
        {
            GetTree().Paused = false;
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ScreenOverlayManager.Instance.FadeOut();
        }

        if (cutsceneScene is CutsceneScene cutsceneRoot)
        {
            try
            {
                cutsceneRoot.IsStarted = true;
                cutsceneRoot.IsFinished = false;

                var animationName = cutsceneRoot.StartAnimation;
                if (cutsceneRoot.AnimationPlayer is not null
                    && !string.IsNullOrEmpty(animationName))
                {
                    var animation = cutsceneRoot.AnimationPlayer.GetAnimation(animationName);
                    if (animation is not null)
                    {
                        cutsceneRoot.AnimationPlayer.Play(animationName);
                        await ToSignal(
                            cutsceneRoot.AnimationPlayer,
                            AnimationPlayer.SignalName.AnimationFinished);
                    }
                }

                cutsceneRoot.IsFinished = true;
            }
            catch (Exception ex)
            {
                GD.PushError($"SceneManager.PlayCutscene cutscene phase failed: {ex}");
                throw;
            }
        }

        if (shouldFade)
        {
            GetTree().Paused = true;
            await ScreenOverlayManager.Instance.FadeIn();
        }

        GetTree().CurrentScene = null;
        GetTree().Root.RemoveChild(cutsceneScene);
        cutsceneScene.QueueFree();
        ActiveCutsceneScene = null;

        var gameplayScene = SuspendedScene;
        SuspendedScene = null;
        GetTree().Root.AddChild(gameplayScene);
        GetTree().CurrentScene = gameplayScene;

        if (metadata.RepositionPlayerOnReturn)
        {
            MovePlayer(metadata.ReturnPlayerPosition);
        }

        ConsumeCutsceneTrigger(gameplayScene, metadata);

        EmitSignal(
            SignalName.GameplayResumed,
            gameplayScene.SceneFilePath ?? "");

        if (shouldFade)
        {
            GetTree().Paused = false;
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ScreenOverlayManager.Instance.FadeOut();
        }
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

    private static void ConsumeCutsceneTrigger(Node gameplayScene, CutsceneMetadata metadata)
    {
        if (gameplayScene is null || metadata is null)
        {
            return;
        }

        if (!metadata.DisableTriggerAfterPlaying
            && !metadata.DestroyTriggerAfterPlaying)
        {
            return;
        }

        var trigger = gameplayScene
            .GetNodeOrNull<CutsceneTrigger>(metadata.TriggerNodePath);
        trigger?.ConsumeAfterPlaying(
            metadata.DestroyTriggerAfterPlaying,
            metadata.DisableTriggerAfterPlaying);
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
            GD.PushWarning(
                "SceneManager.ReloadCurrentSceneFromSave: " +
                "'player_position' key not found.");
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

    public async Task LoadSceneWithMarkerAsync(
        string scenePath,
        string markerName,
        bool fade = true)
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

        var packed = GetPackedScene(scenePath, forceReload);
        if (packed is null)
        {
            return;
        }

        var shouldFade = fade && ScreenOverlayManager.Instance is not null;
        if (shouldFade)
        {
            GetTree().Paused = true;
            await ScreenOverlayManager.Instance.FadeIn();
        }

        PreviousScene = GetTree().CurrentScene?.SceneFilePath ?? "";
        GetTree().ChangeSceneToPacked(packed);
        await ToSignal(GetTree(), SceneTree.SignalName.SceneChanged);

        onSceneReady?.Invoke();

        if (shouldFade)
        {
            GetTree().Paused = false;
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        if (shouldFade)
        {
            await ScreenOverlayManager.Instance.FadeOut();
        }
    }

    private PackedScene GetPackedScene(string scenePath, bool forceReload)
    {
        if (forceReload || !_cache.TryGetValue(scenePath, out var packed))
        {
            GD.Print($"SceneManager: loading scene '{scenePath}' from disk.");
            packed = ResourceLoader.Load<PackedScene>(scenePath);
            if (packed == null)
            {
                GD.PushError($"SceneManager.LoadScene: failed to load '{scenePath}'.");
                return null;
            }

            _cache[scenePath] = packed;
        }
        else
        {
            GD.Print($"SceneManager: using cached scene '{scenePath}'.");
        }

        return packed;
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
