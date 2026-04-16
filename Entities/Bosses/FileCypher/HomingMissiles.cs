using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class HomingMissiles : State
{
    [Export]
    public PackedScene MissileScene { get; set; }

    [Export]
    public Marker2D ArenaTopLeft { get; set; }

    [Export]
    public Marker2D ArenaBottomRight { get; set; }

    [Export]
    public int LaneCount { get; set; } = 5;

    [Export]
    public float SpawnPadding { get; set; } = 48f;

    [Export]
    public double CastTime { get; set; } = 0.6;

    private Character _boss;
    private AnimationPlayer _anim;
    private State _attackIdle;

    [Export]
    private int _safeLaneIndex = 0;

    private bool _fireLeftToRight = true;
    private bool _fired;
    private double _timer;

    public override State Enter(State previousState)
    {
        _boss = Context as Character;
        _anim = _boss?.GetNode<AnimationPlayer>("AnimationPlayer");
        _attackIdle = GetParent().GetNode<State>("AttackIdle");

        _fired = false;
        _timer = CastTime;

        _anim?.Play("HomingMissiles");

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        _timer -= delta;

        if (!_fired && _timer <= 0)
        {
            FireLanePattern();
            _fired = true;
            _timer = 0.4;
            return null;
        }

        if (_fired && _timer <= 0)
        {
            return _attackIdle;
        }

        return null;
    }

    private void FireLanePattern()
    {
        if (_boss == null || MissileScene == null || ArenaTopLeft == null || ArenaBottomRight == null || LaneCount <= 0)
        {
            return;
        }

        float topY = ArenaTopLeft.GlobalPosition.Y;
        float bottomY = ArenaBottomRight.GlobalPosition.Y;
        float leftX = ArenaTopLeft.GlobalPosition.X;
        float rightX = ArenaBottomRight.GlobalPosition.X;

        float arenaHeight = bottomY - topY;
        float laneHeight = arenaHeight / LaneCount;

        var direction = _fireLeftToRight ? Vector2.Right : Vector2.Left;
        float spawnX = _fireLeftToRight ? leftX - SpawnPadding : rightX + SpawnPadding;

        for (int i = 0; i < LaneCount; i++)
        {
            if (i == _safeLaneIndex)
            {
                continue;
            }

            float laneCenterY = topY + laneHeight * (i + 0.5f);

            var missile = MissileScene.Instantiate<StripeMissile>();
            missile.GlobalPosition = new Vector2(spawnX, laneCenterY);
            missile.Direction = direction;
            missile.OwnerCharacter = _boss;
            missile.Rotation = direction.Angle() - Mathf.Pi / 2f;

            GetTree().CurrentScene.AddChild(missile);
        }

        _safeLaneIndex = (_safeLaneIndex + 1) % LaneCount;
        _fireLeftToRight = !_fireLeftToRight;
    }
}
