using CrossedDimensions.Components;
using CrossedDimensions.Entities;
using Godot;

namespace CrossedDimensions.States.Weapons.Behaviors;

public partial class FireProjectileBehavior : WeaponState
{
    [Export]
    public PackedScene ProjectileScene { get; set; }

    [Export]
    public Vector2 Offset { get; set; } = new(0, -4);

    [Signal]
    public delegate void ProjectileFiredEventHandler(Projectile projectile);

    public override State Enter(State previousState)
    {
        var projectile = ProjectileScene.Instantiate<Entities.Projectile>();
        projectile.OwnerCharacter = Weapon.OwnerCharacter;
        projectile.GlobalPosition = Weapon.GlobalPosition
            + Offset;
        projectile.Rotation = Weapon.Target.Angle();

        EmitSignal(SignalName.ProjectileFired, projectile);

        // TODO: use a World class to add the projectile to the scene
        projectile.OwnerCharacter.GetParent().AddChild(projectile);

        return null;
    }
}
