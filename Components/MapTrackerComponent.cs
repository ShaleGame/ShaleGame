using CrossedDimensions.Characters;
using CrossedDimensions.Environment.Map;
using Godot;

namespace CrossedDimensions.Components;

/// <summary>
/// Reports the owning character's world position to <see cref="MapManager"/>
/// each physics frame so map sections reveal as the player explores. Attach as
/// a child of the player <see cref="Character"/>; clones are ignored so only the
/// real player drives map exploration.
/// </summary>
[GlobalClass]
public partial class MapTrackerComponent : Node
{
    /// <summary>
    /// The character whose position is tracked. Defaults to the parent node when
    /// left unassigned.
    /// </summary>
    [Export]
    public Character Character { get; set; }

    public override void _Ready()
    {
        Character ??= GetParent() as Character;

        if (Character is null)
        {
            GD.PushWarning(
                "MapTrackerComponent: no Character assigned or found on parent; "
                    + "disabling.");
            SetPhysicsProcess(false);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (MapManager.Instance is null
            || (Character.Cloneable?.IsClone ?? false))
        {
            return;
        }

        string levelPath = GetTree().CurrentScene?.SceneFilePath ?? "";
        MapManager.Instance.UpdatePlayerPosition(
            Character.GlobalPosition, levelPath);
    }
}
