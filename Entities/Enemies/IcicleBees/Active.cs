using CrossedDimensions.Characters;
using Godot;

namespace CrossedDimensions.States.Enemies.IceBees;

/// <summary>
/// Searches for player and if it finds them and is close to the hive, spawns multiple bees and then goes to cooldown. 
/// Once cooldown is done, goes back to active
/// <summary>

public partial class Active : State
{

    private bool _active = true;

    private Character _player;
    private Character _nest;

    [Export] public PackedScene BeeScene { get; set; }

    [Export] public Node2D BeeSpawnPoint { get; set; }
    [Export] public float DetectionRadius { get; set; } = 75f;

    private double _maxTime = 15.0;
    private double _curTime = 0.0;

    private RandomNumberGenerator _rng;

    public override State Enter(State previousState)
    {
        _nest = Context as Character;
        _player = GetTree().GetFirstNodeInGroup("Player") as Character;

        _rng = new RandomNumberGenerator();

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        if (_active && _nest != null && _player != null)
        {

            if (_nest.GlobalPosition.DistanceTo(_player.GlobalPosition) > DetectionRadius)
            {

                return base.Process(delta);
            }

            // Raycast directly upward to find ceiling
            var spaceState = _nest.GetWorld2D().DirectSpaceState;
            var query = PhysicsRayQueryParameters2D.Create(_nest.GlobalPosition, _player.GlobalPosition);
            query.CollideWithAreas = false;
            query.CollideWithBodies = true;
            query.CollisionMask = (1 << 0) | (1 << 1);

            var result = spaceState.IntersectRay(query);

            if (!result.ContainsKey("collider"))
            {
                return base.Process(delta);
            }

            var collider = result["collider"].As<Node>();

            if (collider == _player)
            {

                var amount = _rng.RandiRange(3, 6);

                GD.Print("Spawning ", amount, " bees!!");

                for (int i = 0; i < amount; i++)
                {
                    var bee = BeeScene.Instantiate() as Character;

                    _nest.GetParent().AddChild(bee);

                    bee.GlobalPosition = BeeSpawnPoint.GlobalPosition;
                }

                _active = false;

            }


        }

        if (!_active)
        {
            _curTime += delta;

            if (_curTime >= _maxTime)
            {
                _curTime = 0;
                _active = true;
            }
        }


        return base.Process(delta);
    }

}
