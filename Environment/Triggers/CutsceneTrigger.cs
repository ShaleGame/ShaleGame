using CrossedDimensions.Environment.Cutscene;
using CrossedDimensions.Saves;
using Godot;

namespace CrossedDimensions.Environment.Triggers;

[GlobalClass]
public partial class CutsceneTrigger : Area2D
{
    [Export]
    public CutsceneMetadata Cutscene { get; set; }

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    public override void _ExitTree()
    {
        BodyEntered -= OnBodyEntered;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is not Characters.Character character)
        {
            return;
        }

        if (!character.IsInGroup("Player"))
        {
            return;
        }

        if (character.Cloneable?.IsClone ?? false)
        {
            character.Cloneable.Merge();
            return;
        }

        if (Cutscene is null)
        {
            GD.PushWarning(
                $"CutsceneTrigger '{Name}' does not have cutscene " +
                "metadata assigned.");
            return;
        }

        SceneManager.Instance?.CallDeferred(
            nameof(SceneManager.PlayCutsceneSync),
            Cutscene,
            true);
    }
}
