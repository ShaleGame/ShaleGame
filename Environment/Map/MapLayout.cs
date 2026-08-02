using Godot;

namespace CrossedDimensions.Environment.Map;

/// <summary>
/// Maps each level scene to its offset (in map cells) within the single shared
/// map coordinate space. Populated by the offset-solver editor tool from the
/// scene-transition graph, and read by <see cref="MapManager"/> to place a
/// level's tiles and to convert player world positions into map cells.
/// </summary>
[GlobalClass]
public partial class MapLayout : Resource
{
    /// <summary>
    /// Level scene path (for example "res://Scenes/CaveLevel.tscn") to its cell
    /// offset within the shared map. Keys are scene paths; values are Vector2I.
    /// </summary>
    [Export]
    public Godot.Collections.Dictionary<string, Vector2I> Offsets { get; set; } = new();

    /// <summary>
    /// Get the map-cell offset for a level scene path, or the zero vector if the
    /// level has no recorded offset.
    /// </summary>
    public Vector2I GetOffset(string levelScenePath)
    {
        return Offsets.TryGetValue(levelScenePath, out var offset)
            ? offset
            : Vector2I.Zero;
    }
}
