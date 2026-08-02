using CrossedDimensions.Environment.Map;
using Godot;

namespace CrossedDimensions.UI;

/// <summary>
/// Full-screen explored-areas map. Reads revealed-section state from
/// <see cref="MapManager"/> and renders those sections into its own display
/// layer, with a marker at the player's current cell. Toggled with the
/// <c>open_map</c> action; overlays live without pausing the game.
/// </summary>
/// <remarks>
/// Expected scene layout (this script on the root Control, process mode
/// Always): <c>MapView</c> (Node2D) containing <c>MapDisplay</c>
/// (TileMapLayer) and <c>PlayerMarker</c> (Node2D).
/// </remarks>
[GlobalClass]
public partial class MapScreen : Control
{
    /// <summary>
    /// Container transformed for pan/zoom; holds the display layer and marker.
    /// </summary>
    [Export]
    public Node2D MapView { get; set; }

    /// <summary>
    /// Layer that revealed section tiles are copied into for display.
    /// </summary>
    [Export]
    public TileMapLayer MapDisplay { get; set; }

    /// <summary>
    /// Marker positioned at the player's current map cell.
    /// </summary>
    [Export]
    public Node2D PlayerMarker { get; set; }

    /// <summary>
    /// Display scale applied to <see cref="MapView"/> when the map opens.
    /// </summary>
    [Export]
    public float ViewScale { get; set; } = 4.0f;

    /// <summary>
    /// Pan speed in screen pixels per second while the map is open.
    /// </summary>
    [Export]
    public float PanSpeed { get; set; } = 600.0f;

    public override void _Ready()
    {
        Hide();

        if (MapManager.Instance is not null)
        {
            MapManager.Instance.SectionExplored += OnSectionExplored;
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete && MapManager.Instance is not null)
        {
            MapManager.Instance.SectionExplored -= OnSectionExplored;
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("open_map"))
        {
            Toggle();
        }

        if (!Visible)
        {
            return;
        }

        if (Input.IsActionJustPressed("escape"))
        {
            Close();
            return;
        }

        UpdateMarker();
        Pan((float)delta);
    }

    private void Toggle()
    {
        if (Visible)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    private void Open()
    {
        Rebuild();
        Show();

        if (MapView is not null)
        {
            MapView.Scale = new Vector2(ViewScale, ViewScale);
        }

        CenterOnPlayer();
    }

    private void Close()
    {
        Hide();
    }

    /// <summary>
    /// Clear and repopulate the display from every explored section.
    /// </summary>
    private void Rebuild()
    {
        if (MapDisplay is null || MapManager.Instance is null)
        {
            return;
        }

        MapDisplay.Clear();

        foreach (string sectionId in MapManager.Instance.ExploredSections)
        {
            MapManager.Instance.CopySectionInto(MapDisplay, sectionId);
        }
    }

    private void OnSectionExplored(string sectionId)
    {
        if (Visible && MapDisplay is not null)
        {
            MapManager.Instance?.CopySectionInto(MapDisplay, sectionId);
        }
    }

    private void UpdateMarker()
    {
        if (PlayerMarker is null || MapDisplay is null
            || MapManager.Instance is null)
        {
            return;
        }

        PlayerMarker.Position =
            MapDisplay.MapToLocal(MapManager.Instance.PlayerCell);
    }

    private void CenterOnPlayer()
    {
        if (MapView is null || PlayerMarker is null || MapManager.Instance is null)
        {
            return;
        }

        UpdateMarker();
        Vector2 screenCenter = GetViewportRect().Size / 2f;
        MapView.Position = screenCenter - (PlayerMarker.Position * MapView.Scale);
    }

    private void Pan(float delta)
    {
        if (MapView is null)
        {
            return;
        }

        // TODO: pan with movement keys, can probably have like a pan with
        // mouse for that to let player move while map is open
        Vector2 dir = Input.GetVector(
            "move_right", "move_left", "move_down", "move_up");

        if (dir != Vector2.Zero)
        {
            MapView.Position += dir * PanSpeed * delta;
        }
    }
}
