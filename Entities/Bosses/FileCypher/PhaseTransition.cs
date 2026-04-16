using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class PhaseTransition : State
{
    [Export]
    public double TransitionDuration { get; set; } = 1.5;

    private Character _boss;
    private AnimationPlayer _anim;
    private CypherSequencer _sequencer;
    private StateMachine _attacks;
    private double _remaining;

    public override State Enter(State previousState)
    {
        _boss = Context as Character;
        _anim = _boss?.GetNode<AnimationPlayer>("AnimationPlayer");
        _sequencer = GetParent().GetNode<CypherSequencer>("CypherSequencer");
        _attacks = _boss?.GetNode<StateMachine>("Attacks");

        _attacks?.ChangeState("AttackIdle");

        if (_boss != null)
        {
            _boss.MotionMode = CharacterBody2D.MotionModeEnum.Floating;
            _boss.Velocity = Vector2.Zero;
            _boss.VelocityFromInput = Vector2.Zero;
            _boss.VelocityFromExternalForces = Vector2.Zero;
        }

        _anim?.Play("PhaseTransition");
        _sequencer?.SetPhaseTwo();

        _remaining = TransitionDuration;

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        _remaining -= delta;
        if (_remaining <= 0)
        {
            return _sequencer;
        }

        return null;
    }
}
