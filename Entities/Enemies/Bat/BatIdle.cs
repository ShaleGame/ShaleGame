using CrossedDimensions.Characters;
using Godot;

namespace CrossedDimensions.States.Enemies;

[GlobalClass]
public partial class BatIdle : State
{
    [Export]
    public float DetectionRadius { get; set; } = 300f;

    private Godot.Timer spotPlayerTimer;

    private Character _bat;
    private Node2D _player;
    private State _attacking;

    private AnimatedSprite2D _sprite;
    private bool spottedPlayer;

    private Callable _timeoutCallable;

    public override void _Ready()
    {
        _timeoutCallable = new Callable(this, nameof(TimerSetOff));

        base._Ready();
    }

    public override State Enter(State previousState)
    {
        _bat = Context as Character;

        // Get reference to Attacking state
        _attacking = GetParent().GetNode<State>("Attacking");

        // Get player reference
        _player = GetTree().GetFirstNodeInGroup("Player") as Node2D;

        // Get animation sprite
        _sprite = _bat?.GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        // Tell movement state machine to hang
        var movementSh = _bat?.GetNode<StateMachine>("MovementStateMachine");
        movementSh?.ChangeState("Hanging");

        spotPlayerTimer = _bat.GetNode<Godot.Timer>("SpotPlayerTimer");
        // Avoid duplicate signal attachments
        if (!spotPlayerTimer.IsConnected(Godot.Timer.SignalName.Timeout, _timeoutCallable))
        {
            spotPlayerTimer.Connect(Godot.Timer.SignalName.Timeout, _timeoutCallable);
        }

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {

        // Check if player is within detection radius
        if (_bat != null && _player != null)
        {
            // Raycast directly upward to find ceiling
            var spaceState = _bat.GetWorld2D().DirectSpaceState;
            var query = PhysicsRayQueryParameters2D.Create(_bat.GlobalPosition, _player.GlobalPosition);
            query.CollideWithAreas = false;
            query.CollideWithBodies = true;

            var result = spaceState.IntersectRay(query);

            if (!result.ContainsKey("collider"))
            {
                return base.Process(delta);
            }

            var collider = result["collider"].As<Node>();

            if (collider == _player)
            {
                var position = result["position"].As<Vector2>();
                float distanceToPlayer = _bat.GlobalPosition.DistanceTo(position);
                if (distanceToPlayer <= DetectionRadius)
                {
                    if (spottedPlayer)
                    {
                        spottedPlayer = false;
                        return _attacking;
                    }
                    else if (spotPlayerTimer.IsStopped())
                    {
                        spotPlayerTimer.Start();
                        _sprite.Frame = 1;
                    }
                }
            }

            spottedPlayer = false;
        }

        return base.Process(delta);
    }

    public void TimerSetOff()
    {
        spottedPlayer = true;
    }
}
