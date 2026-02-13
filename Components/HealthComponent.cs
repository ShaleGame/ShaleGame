using Godot;

namespace CrossedDimensions.Components;

[GlobalClass]
public partial class HealthComponent : Node
{
    /// <summary>
    /// The maximum health value.
    /// </summary>
    [Export]
    public int MaxHealth { get; set; } // TODO: could use character resource stat

    private int _currentHealth;

    [Export]
    public int CurrentHealth
    {
        get => _currentHealth;
        set
        {
            int oldHealth = _currentHealth;
            _currentHealth = Mathf.Clamp(value, 0, MaxHealth);
            EmitSignal(SignalName.HealthChanged, oldHealth);
        }
    }

    public bool IsAlive => CurrentHealth > 0;

    [Signal]
    public delegate void HealthChangedEventHandler(int oldHealth);

    /// <summary>
    /// Sets the health stats without emitting a signal. Used when forcefully
    /// changing health, such as when splitting or merging characters.
    /// </summary>
    public void SetStats(int health, int maxHealth)
    {
        MaxHealth = maxHealth;
        _currentHealth = Mathf.Clamp(health, 0, MaxHealth);
    }
}
