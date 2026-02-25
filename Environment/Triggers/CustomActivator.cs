using System.Linq;
using Godot;

namespace CrossedDimensions.Environment.Triggers;

/// <summary>
/// Base class for activators with custom activation logic.
/// Override ShouldActivate() to implement your own logic.
/// Pure logic component - visual/physics components should listen to Activated/Deactivated signals.
/// </summary>
[GlobalClass]
public partial class CustomActivator : Activator
{
    protected override bool ShouldActivate()
    {
        // Default implementation: require all triggers to be active
        // Override this method in subclasses for custom logic
        return Triggers.All(t => t.IsActive);
    }
}
