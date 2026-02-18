namespace CrossedDimensions.Environment.Triggers;

/// <summary>
/// Defines standard activation logic patterns for ActivationLogicActivator.
/// </summary>
public enum ActivationLogic
{
    /// <summary>
    /// Activate when ALL triggers are active (AND logic).
    /// </summary>
    RequireAll,

    /// <summary>
    /// Activate when ANY trigger is active (OR logic).
    /// </summary>
    RequireAny,

    /// <summary>
    /// Activate when EXACTLY ONE trigger is active (XOR logic).
    /// </summary>
    RequireOne,

    /// <summary>
    /// Activate when NO triggers are active (NOT logic).
    /// </summary>
    RequireNone
}
