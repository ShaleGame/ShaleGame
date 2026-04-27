using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class PhaseTwoStagger : State
{
    [Export]
    public double Duration { get; set; } = 5.0;

    private Character _boss;
    private AnimationPlayer _anim;
    private State _attackIdle;
    private CypherSequencer _sequencer;
    private double _remaining;

    public override State Enter(State previousState)
    {
        _boss = Context as Character;
        _anim = _boss?.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        _sequencer = GetParent().GetNode<CypherSequencer>("CypherSequencer");
        _attackIdle = GetParent().GetNode<State>("AttackIdle");
        _remaining = Duration;

        _anim?.Play("Phase2Stagger");

        if (_boss != null)
        {
            _boss.Velocity = Vector2.Zero;
            _boss.VelocityFromInput = Vector2.Zero;
            _boss.VelocityFromExternalForces = Vector2.Zero;
        }

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        _remaining -= delta;
        if (_remaining <= 0d)
        {
            return _attackIdle;
        }

        return null;
    }

    public override void Exit(State nextState)
    {
        _sequencer?.RefreshBossVulnerabilityAnimation(nextState);
        base.Exit(nextState);
    }
}
