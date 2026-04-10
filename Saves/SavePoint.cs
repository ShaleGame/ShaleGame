using Godot;
using CrossedDimensions.Characters;
using CrossedDimensions.Environment.Cutscene.Interactables;
using System.Collections.Generic;
using System.Linq;

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

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            Interactable.Interacted -= OnInteracted;
        }
    }

    private void OnInteracted()
    {
        Save();
    }

    public void Save()
    {
        var players = GetTree()
            .GetNodesInGroup("Player")
            .OfType<Characters.Character>()
            .ToList();

        RestorePlayerHealth(players);
        SavePlayerPosition(players);
    }

    private void RestorePlayerHealth(List<Characters.Character> players)
    {
        foreach (var player in players)
        {
            player.Health.CurrentHealth = player.Health.MaxHealth;
        }
    }

    private void SavePlayerPosition(List<Characters.Character> players)
    {
        var main = players
            .Where(p => p.Cloneable.Original is null)
            .FirstOrDefault();

        string scenePath = GetTree().CurrentScene?.SceneFilePath ?? "";

        GD.Print($"Saving player position {main.GlobalPosition} in scene {scenePath}");

        SaveManager.SetKey("player_scene", scenePath);
        SaveManager.SetKey("player_position", main.GlobalPosition);
        SaveManager.WritePersistent();
    }
}
