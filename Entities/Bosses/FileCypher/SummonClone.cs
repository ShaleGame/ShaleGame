using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class SummonClone : State
{
    [Export]
    public PackedScene CloneScene { get; set; }

    [Export]
    public Marker2D CloneAnchorPoint { get; set; }

    [Export]
    public float CloneHealthFractionOfBossMax { get; set; } = 0.25f;

    [Export]
    public double CastTime { get; set; } = 0.6;

    private Character _boss;
    private State _attackIdle;
    private CypherSequencer _sequencer;
    private double _timer;
    private bool _spawned;

    public override State Enter(State previousState)
    {
        _boss = Context as Character;
        _attackIdle = GetParent().GetNode<State>("AttackIdle");
        _sequencer = _boss?.GetNode<StateMachine>("StateMachine")?.GetNode<CypherSequencer>("CypherSequencer");

        _timer = CastTime;
        _spawned = false;

        var anim = _boss?.GetNode<AnimationPlayer>("AnimationPlayer");
        anim?.Play("SummonClone");

        if (_sequencer != null && _sequencer.HasActiveClone())
        {
            return _attackIdle;
        }

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        _timer -= delta;

        if (!_spawned && _timer <= 0)
        {
            SpawnClone();
            _spawned = true;
            _timer = 0.2;
            return null;
        }

        if (_spawned && _timer <= 0)
        {
            return _attackIdle;
        }

        return null;
    }

    private void SpawnClone()
    {
        if (_boss == null || CloneScene == null || CloneAnchorPoint == null || _sequencer == null)
        {
            return;
        }

        var clone = CloneScene.Instantiate<AnchorClone>();
        _boss.AddChild(clone);
        clone.TopLevel = true;
        clone.GlobalPosition = CloneAnchorPoint.GlobalPosition;

        int cloneHealth = Mathf.RoundToInt(_boss.Health.MaxHealth * CloneHealthFractionOfBossMax);
        clone.Initialize(_boss, cloneHealth);

        _sequencer.RegisterClone(clone);
    }
}
