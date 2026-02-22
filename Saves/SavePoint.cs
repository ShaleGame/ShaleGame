using Godot;
using CrossedDimensions.Environment.Cutscene.Interactables;

namespace CrossedDimensions.Saves;

/// <summary>
/// A save point that records the player's current scene and position when
/// its child <see cref="Interactable"/> is interacted with.
/// </summary>
public partial class SavePoint : Node2D
{
    [Export]
    public Interactable Interactable { get; set; }

    /// <summary>
    /// Injected SaveManager. Defaults to <see cref="SaveManager.Instance"/> in <see cref="_Ready"/>.
    /// Set this in tests to avoid touching the singleton.
    /// </summary>
    public SaveManager SaveManager { get; set; }

    public override void _Ready()
    {
        SaveManager ??= SaveManager.Instance;
        Interactable.Interacted += OnInteracted;
    }

    public override void _ExitTree()
    {
        Interactable.Interacted -= OnInteracted;
    }

    private void OnInteracted()
    {
        var players = GetTree().GetNodesInGroup("Player");

        if (players.Count == 0)
        {
            throw new System.InvalidOperationException("No player found in scene when saving.");
        }

        var player = (Characters.Character)players[0];
        string scenePath = GetTree().CurrentScene?.SceneFilePath ?? "";

        GD.Print($"Saving player position {player.GlobalPosition} in scene {scenePath}");

        SaveManager.SetKey("player_scene", scenePath);
        SaveManager.SetKey("player_position", player.GlobalPosition);
        SaveManager.WritePersistent();
    }
}
