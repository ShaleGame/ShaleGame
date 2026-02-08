using Godot;

namespace CrossedDimensions.States.Weapons.Behaviors;

public partial class FireProjectileBehavior : WeaponState
{
    [Export]
    public PackedScene ProjectileScene { get; set; }

    [Export]
    public Vector2 Offset { get; set; } = new(0, -4);

    public override State Enter(State previousState)
    {
        var projectile = ProjectileScene.Instantiate<Entities.Projectile>();
        projectile.OwnerCharacter = Weapon.OwnerCharacter;
        projectile.GlobalPosition = projectile.OwnerCharacter.GlobalPosition
            + Offset;
        projectile.Rotation = Weapon.Target.Angle();

        // TODO: use a World class to add the projectile to the scene
        projectile.OwnerCharacter.GetParent().AddChild(projectile);

        return null;
    }
}
