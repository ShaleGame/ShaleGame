using System.Linq;
using Godot;

namespace CrossedDimensions.Environment.Triggers;

/// <summary>
/// Concrete activator that uses standard enum-based activation logic.
/// Pure logic component - visual/physics components should listen to Activated/Deactivated signals.
/// </summary>
[GlobalClass]
public partial class ActivationLogicActivator : Activator
{
    /// <summary>
    /// The activation logic pattern to use for determining when to activate.
    /// </summary>
    [Export]
    public ActivationLogic Logic { get; set; } = ActivationLogic.RequireAll;

    protected override bool ShouldActivate()
    {
        return Logic switch
        {
            ActivationLogic.RequireAll => Triggers.Count > 0 && Triggers.All(t => t.IsActive),
            ActivationLogic.RequireAny => Triggers.Any(t => t.IsActive),
            ActivationLogic.RequireOne => Triggers.Count(t => t.IsActive) == 1,
            ActivationLogic.RequireNone => Triggers.All(t => !t.IsActive),
            _ => false
        };
    }
}
