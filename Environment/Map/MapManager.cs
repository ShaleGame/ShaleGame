using CrossedDimensions.Saves;
using Godot;

namespace CrossedDimensions.Environment.Map;

/// <summary>
/// Tracks which map sections the player has explored and owns the authoritative
/// painted map (<see cref="MapSource"/>). Exploration is detected by converting
/// the player's world position into a cell on the shared map and reading that
/// cell's <c>section_id</c> custom data; entering any cell of a section reveals
/// the whole section.
/// </summary>
/// <remarks>
/// Designed to be registered as an autoload singleton. The map UI reads section
/// state from this node so that exploration is tracked even while the map screen
/// is closed.
/// </remarks>
[GlobalClass]
public partial class MapManager : Node
{
    public static MapManager Instance { get; private set; }

    /// <summary>
    /// Emitted the first time a section is explored, with the section id.
    /// </summary>
    [Signal]
    public delegate void SectionExploredEventHandler(string sectionId);

    /// <summary>
    /// Emitted when the section the player currently occupies changes, with the
    /// new section id (empty string when the player is on unpainted map).
    /// </summary>
    [Signal]
    public delegate void CurrentSectionChangedEventHandler(string sectionId);

    /// <summary>
    /// The painted whole-world map, instanced hidden as the authoritative source
    /// of section data. Assign a scene whose root is (or contains) the map
    /// <see cref="TileMapLayer"/>.
    /// </summary>
    [Export]
    public PackedScene MapSourceScene { get; set; }

    /// <summary>
    /// Per-level cell offsets within the shared map space.
    /// </summary>
    [Export]
    public MapLayout Layout { get; set; }

    /// <summary>
    /// How many world units correspond to one map cell along each axis.
    /// </summary>
    [Export]
    public float WorldUnitsPerMapCell { get; set; } = 256.0f;

    /// <summary>
    /// The custom data layer name on the map tileset that identifies a section.
    /// </summary>
    public const string SectionDataLayer = "section_id";

    /// <summary>
    /// The authoritative map tile layer, populated from
    /// <see cref="MapSourceScene"/> in <see cref="_Ready"/>.
    /// </summary>
    public TileMapLayer MapSource { get; private set; }

    public string CurrentSectionId { get; private set; } = "";

    private readonly System.Collections.Generic.HashSet<string> _explored = new();

    public override void _Ready()
    {
        Instance = this;

        InstanceMapSource();

        if (SaveManager.Instance is not null)
        {
            SaveManager.Instance.CurrentSaveChanged += OnCurrentSaveChanged;
            LoadFromSave(SaveManager.Instance.CurrentSave);
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete && SaveManager.Instance is not null)
        {
            SaveManager.Instance.CurrentSaveChanged -= OnCurrentSaveChanged;
        }
    }

    /// <summary>
    /// Whether the given section has been explored.
    /// </summary>
    public bool IsExplored(string sectionId)
    {
        return _explored.Contains(sectionId);
    }

    /// <summary>
    /// A snapshot of all explored section ids.
    /// </summary>
    public System.Collections.Generic.IReadOnlyCollection<string> ExploredSections
        => _explored;

    /// <summary>
    /// Feed the player's current world position and level so the manager can
    /// resolve the occupied section, revealing it on first entry. Intended to be
    /// called each physics frame by a component on the player.
    /// </summary>
    public void UpdatePlayerPosition(Vector2 worldPosition, string levelScenePath)
    {
        string section = SectionAtWorldPosition(worldPosition, levelScenePath);

        if (!string.IsNullOrEmpty(section))
        {
            RevealSection(section);
        }

        if (section != CurrentSectionId)
        {
            CurrentSectionId = section;
            EmitSignal(SignalName.CurrentSectionChanged, section);
        }
    }

    /// <summary>
    /// Convert a world position in the given level into its map cell.
    /// </summary>
    public Vector2I WorldToCell(Vector2 worldPosition, string levelScenePath)
    {
        var local = worldPosition / WorldUnitsPerMapCell;
        var cell = new Vector2I(Mathf.FloorToInt(local.X), Mathf.FloorToInt(local.Y));
        return cell + (Layout?.GetOffset(levelScenePath) ?? Vector2I.Zero);
    }

    /// <summary>
    /// Resolve the section id painted at the given world position, or empty
    /// string if there is no map data there.
    /// </summary>
    public string SectionAtWorldPosition(Vector2 worldPosition, string levelScenePath)
    {
        if (MapSource is null)
        {
            return "";
        }

        Vector2I cell = WorldToCell(worldPosition, levelScenePath);
        TileData data = MapSource.GetCellTileData(cell);

        if (data is null)
        {
            return "";
        }

        return data.GetCustomData(SectionDataLayer).AsString();
    }

    /// <summary>
    /// Mark a section explored and persist it. Emits <see cref="SectionExplored"/>
    /// only on the first reveal.
    /// </summary>
    public void RevealSection(string sectionId)
    {
        if (string.IsNullOrEmpty(sectionId) || !_explored.Add(sectionId))
        {
            return;
        }

        PersistToSave(SaveManager.Instance?.CurrentSave);
        EmitSignal(SignalName.SectionExplored, sectionId);
    }

    private void InstanceMapSource()
    {
        if (MapSourceScene is null)
        {
            return;
        }

        Node root = MapSourceScene.Instantiate();
        MapSource = root as TileMapLayer ?? root.GetNodeOrNull<TileMapLayer>("%MapSource");

        if (MapSource is null)
        {
            GD.PushWarning(
                "MapManager: MapSourceScene root is not a TileMapLayer and has "
                    + "no '%MapSource' unique node.");
            root.QueueFree();
            return;
        }

        MapSource.Visible = false;
        AddChild(root);
    }

    private void OnCurrentSaveChanged(SaveFile previous, SaveFile current)
    {
        LoadFromSave(current);
    }

    private void LoadFromSave(SaveFile save)
    {
        _explored.Clear();

        if (save is not null)
        {
            foreach (string id in save.ExploredSections)
            {
                _explored.Add(id);
            }
        }

        CurrentSectionId = "";
    }

    private void PersistToSave(SaveFile save)
    {
        if (save is null)
        {
            return;
        }

        var array = new Godot.Collections.Array<string>();

        foreach (string id in _explored)
        {
            array.Add(id);
        }

        save.ExploredSections = array;
    }
}
