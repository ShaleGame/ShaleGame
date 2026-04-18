using Godot;

namespace CrossedDimensions.Characters;

/// <summary>
/// Handles clone-specific death behavior by merging the clone back into its
/// original character.
/// </summary>
public partial class CloneDeathHandler : Node
{
    [Export]
    public Character Character { get; set; }

    public override void _Ready()
    {
        if (Character?.Health is not null)
        {
            Character.Health.HealthChanged += OnHealthChanged;
        }
    }

    private void OnHealthChanged(int oldHealth)
    {
        if (Character?.Health is null || Character.Health.IsAlive)
        {
            return;
        }

        if (Character.Cloneable?.IsClone != true)
        {
            return;
        }

        var originalCloneable = Character.Cloneable.Original?.Cloneable;
        originalCloneable?.ClearHealingPool();
        originalCloneable?.Merge();
    }
}
