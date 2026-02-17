using Godot;

namespace CrossedDimensions.Characters;

public partial class PlayerDeathHandler : Node
{
    [Export]
    public Character Character { get; set; }

    public override void _Ready()
    {
        Character.Health.HealthChanged += OnHealthChanged;
    }

    private void OnHealthChanged(int oldHealth)
    {
        if (Character.Health.IsAlive)
        {
            return;
        }

        if (Character.Cloneable?.IsClone ?? false)
        {
            OnCloneCharacterDeath();
        }
        else
        {
            OnOriginalCharacterDeath();
        }
    }

    private void OnOriginalCharacterDeath()
    {
        const string ScenePath = "res://Scenes/DeathScreen.tscn";
        GetTree().CallDeferred("change_scene_to_file", ScenePath);
    }

    private void OnCloneCharacterDeath()
    {
        Character.Cloneable?.Original?.Cloneable?.Merge();
    }
}
