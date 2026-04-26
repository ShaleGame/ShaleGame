using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class Idle : State
{
    [Export]
    public double IntroDuration { get; set; } = 1.0;

    private Character _boss;
    private AnimationPlayer _anim;
    private State _sequencer;
    private double _remaining;

    public override State Enter(State previousState)
    {
        _boss = Context as Character;
        _anim = _boss?.GetNode<AnimationPlayer>("AnimationPlayer");
        _sequencer = GetParent().GetNode<State>("CypherSequencer");

        _anim?.Play("Phase1Idle");
        _remaining = IntroDuration;

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
