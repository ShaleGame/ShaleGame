using System.Collections.Generic;
using CrossedDimensions.Entities;
using CrossedDimensions.Extensions;
using Godot;

namespace CrossedDimensions.Components;

/// <summary>
/// Tracks <see cref="FreezableComponent"/>s that have been frozen by a weapon
/// and provides a way to unfreeze all of them at once.
/// </summary>
[GlobalClass]
public partial class FreezeTrackerComponent : Node
{
    private readonly List<FreezableComponent> _frozenComponents = new();

    /// <summary>
    /// Registers a <see cref="FreezableComponent"/> as frozen by this tracker.
    /// The component will automatically be removed from the list when it
    /// thaws on its own.
    /// </summary>
    public void TrackFrozen(FreezableComponent freezable)
    {
        if (_frozenComponents.Contains(freezable))
        {
            return;
        }

        _frozenComponents.Add(freezable);
        freezable.Unfrozen += () => _frozenComponents.Remove(freezable);
    }

    /// <summary>
    /// Unfreezes all currently tracked <see cref="FreezableComponent"/>s.
    /// Components that have already been freed from the scene tree are
    /// skipped safely.
    /// </summary>
    public void UnfreezeAll()
    {
        // toarray to avoid modifying the original list while iterating
        foreach (var freezable in _frozenComponents.ToArray())
        {
            if (GodotObject.IsInstanceValid(freezable))
            {
                freezable.Unfreeze();
            }
        }
    }

    public void OnProjectileFired(Projectile projectile)
    {
        if (projectile.HasNode<IceCrystalHitHandler>("HitHandler", out var hitHandler))
        {
            hitHandler.FreezeTracker = this;
        }
    }
}
