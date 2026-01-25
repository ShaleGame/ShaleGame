using Godot;

namespace CrossedDimensions.Entities.Enemies.Walker;

public partial class WalkingEnemy : CharacterBody2D
{
    [Export]
    public float MovementSpeed { get; set; } = 100f;

    [Export]
    public RayCast2D FloorCheck { get; set; }

    [Export]
    public RayCast2D WallCheck { get; set; }

    [Export]
    public AnimatedSprite2D Sprite { get; set; }

    [Export]
    public Components.HealthComponent HealthComponent { get; set; }

    private int _direction = -1;

    public override void _Process(double delta)
    {
        MoveBehavior((float)delta);

        if (Sprite is not null && Sprite.Animation != "Walk")
        {
            Sprite.Play("Walk");
        }
    }

    private void MoveBehavior(float delta)
    {
        Velocity = new Vector2(_direction * MovementSpeed, Velocity.Y);

        if (WallCheck is not null && WallCheck.IsColliding())
        {
            Flip();
        }

        if (FloorCheck is not null && !FloorCheck.IsColliding())
        {
            Flip();
        }

        MoveAndSlide();
    }

    private void Flip()
    {
        _direction *= -1;

        if (Sprite is not null)
        {
            var scale = Sprite.Scale;
            Sprite.Scale = new Vector2(scale.X * -1f, scale.Y);
            Sprite.Play("Walk");
        }

        if (FloorCheck is not null)
        {
            var position = FloorCheck.Position;
            FloorCheck.Position = new Vector2(position.X * -1f, position.Y);
        }

        if (WallCheck is not null)
        {
            var position = WallCheck.Position;
            var target = WallCheck.TargetPosition;
            WallCheck.Position = new Vector2(position.X * -1f, position.Y);
            WallCheck.TargetPosition = new Vector2(target.X * -1f, target.Y);
        }
    }
}
