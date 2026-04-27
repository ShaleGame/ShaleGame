using Godot;
using CrossedDimensions.States;

namespace CrossedDimensions.Entities.Bosses.Drill;

/// <summary>
/// The vent is dead and cannot be interacted with.
/// </summary>
public partial class Dead : State
{
    [Export] public CollisionShape2D HurtboxCollision {get; set;}
    [Export] public  CollisionShape2D FrozenBoxCollision {get; set;}
    [Export] AnimatedSprite2D Sprite {get; set;}

    [Signal] public delegate void VentDiedEventHandler();

    public override State Enter(State previousState)
    {
        HurtboxCollision.Disabled = true;
        FrozenBoxCollision.Disabled = true;

        Sprite.Play("Broken");

        EmitSignal("VentDied");

        return base.Enter(previousState);
    }

}
