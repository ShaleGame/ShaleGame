using Godot;
using CrossedDimensions.States;
using CrossedDimensions.Characters;
using CrossedDimensions.BoundingBoxes;
using CrossedDimensions.Environment.Triggers;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class CypherSequencer : State
{
    [Export]
    public float PhaseTwoThresholdRatio { get; set; } = 0.5f;

    [Export]
    public double TimeBetweenAttacks { get; set; } = 1.0;

    [Export]
    public double StaggerDuration { get; set; } = 3.0;

    [Export]
    public float HoverSpeed { get; set; } = 180f;

    [Export]
    public float HoverArrivalThreshold { get; set; } = 16f;

    private readonly string[] _phase1Sequence = { "BurstGun", "GroundSlam", "BurstGun", "HomingMissiles" };
    private readonly string[] _phase2Sequence = { "BulletHell", "SwitchPressure", "BulletHell", "SpiralBulletHell", "PhaseTwoStagger", "SwitchPressure" };

    private Character _boss;
    private StateMachine _attacks;
    private AttackIdle _attackIdle;
    private State _phaseTransition;
    private Hurtbox _bossHurtbox;
    private CollisionShape2D _bossHurtboxShape;

    private Marker2D _airPathLeft;
    private Marker2D _airPathRight;
    private Marker2D _currentAirTarget;
    private Trigger _topSwitch;
    private Trigger _bottomSwitch;

    private int _phase1Index;
    private int _phase2Index;
    private bool _isPhaseTwo;

    private bool _waitingForAttackToFinish;
    private bool _queuedAttackFinished;
    private double _attackCooldownRemaining;
    private double _staggerRemaining;

    private AnchorClone _activeClone;
    private bool _attacksInitialized;

    public override State Enter(State previousState)
    {
        _boss = Context as Character;
        _attacks = _boss.FindChild("Attacks") as StateMachine;

        if (_attacks != null && !_attacksInitialized)
        {
            _attacks.Initialize(_boss);
            _attacksInitialized = true;
        }

        _attackIdle = _attacks?.GetNode<AttackIdle>("AttackIdle");
        _phaseTransition = GetParent().GetNode<State>("PhaseTransition");

        _bossHurtbox = _boss?.GetNode<Hurtbox>("Hurtbox");
        _bossHurtboxShape = _boss?.GetNode<CollisionShape2D>("Hurtbox/CollisionShape2D");

        _airPathLeft ??= _boss?.GetNodeOrNull<Marker2D>("AirPathLeft");
        _airPathRight ??= _boss?.GetNodeOrNull<Marker2D>("AirPathRight");
        if (_currentAirTarget == null || !GodotObject.IsInstanceValid(_currentAirTarget))
        {
            _currentAirTarget = _airPathRight ?? _airPathLeft;
        }

        _attackIdle.AttackHasFinished += OnAttackFinished;

        _attackCooldownRemaining = TimeBetweenAttacks;
        _waitingForAttackToFinish = false;
        _queuedAttackFinished = false;

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        _attacks?.Process(delta);

        UpdatePhaseTwoVulnerability();

        if (!_isPhaseTwo && _boss.Health.CurrentHealth <= _boss.Health.MaxHealth * PhaseTwoThresholdRatio)
        {
            return _phaseTransition;
        }

        if (_staggerRemaining > 0)
        {
            _staggerRemaining -= delta;
            return null;
        }

        if (_queuedAttackFinished)
        {
            _queuedAttackFinished = false;
            _waitingForAttackToFinish = false;
            _attackCooldownRemaining = TimeBetweenAttacks;
        }

        if (_waitingForAttackToFinish)
        {
            return null;
        }

        _attackCooldownRemaining -= delta;
        if (_attackCooldownRemaining > 0)
        {
            return null;
        }

        var nextAttack = GetNextAttackName();
        GD.Print($"[FileCypher] Starting attack: {nextAttack}");
        _waitingForAttackToFinish = true;
        _attacks.ChangeState(nextAttack);

        return null;
    }

    public override State PhysicsProcess(double delta)
    {
        _attacks?.PhysicsProcess(delta);

        if (!_isPhaseTwo || _currentAirTarget == null)
        {
            return null;
        }

        if (IsInPhaseTwoStaggerState())
        {
            _boss.Velocity = Vector2.Zero;
            _boss.VelocityFromInput = Vector2.Zero;
            _boss.VelocityFromExternalForces = Vector2.Zero;
            return null;
        }

        var direction = _boss.GlobalPosition.DirectionTo(_currentAirTarget.GlobalPosition);

        _boss.Velocity = Vector2.Zero;
        _boss.VelocityFromInput = Vector2.Zero;
        _boss.VelocityFromExternalForces = Vector2.Zero;
        _boss.GlobalPosition += direction * HoverSpeed * (float)delta;

        if (_boss.GlobalPosition.DistanceTo(_currentAirTarget.GlobalPosition) <= HoverArrivalThreshold)
        {
            _currentAirTarget = _currentAirTarget == _airPathLeft ? _airPathRight : _airPathLeft;
        }

        return null;
    }

    public override void Exit(State nextState)
    {
        if (_attackIdle != null)
        {
            _attackIdle.AttackHasFinished -= OnAttackFinished;
        }

        if (_activeClone != null && GodotObject.IsInstanceValid(_activeClone))
        {
            _activeClone.CloneDied -= OnCloneDied;
        }

        base.Exit(nextState);
    }

    public void SetPhaseTwo()
    {
        _isPhaseTwo = true;
        _phase2Index = 0;
        _attackCooldownRemaining = TimeBetweenAttacks;
        _waitingForAttackToFinish = false;
        _queuedAttackFinished = false;
        _currentAirTarget = _airPathRight ?? _airPathLeft;
        SetBossVulnerable(false);
    }

    public void ConfigureAirPathMarkers(Marker2D left, Marker2D right)
    {
        _airPathLeft = left;
        _airPathRight = right;
        _currentAirTarget = _airPathRight ?? _airPathLeft;
    }

    public void ConfigurePhaseTwoSwitches(Trigger topSwitch, Trigger bottomSwitch)
    {
        _topSwitch = topSwitch;
        _bottomSwitch = bottomSwitch;
    }

    public void RegisterClone(AnchorClone clone)
    {
        if (_activeClone != null && GodotObject.IsInstanceValid(_activeClone))
        {
            return;
        }

        _activeClone = clone;
        _activeClone.CloneDied += OnCloneDied;

        SetBossVulnerable(false);
    }

    public bool HasActiveClone()
    {
        return _activeClone != null && GodotObject.IsInstanceValid(_activeClone);
    }

    private string GetNextAttackName()
    {
        if (!_isPhaseTwo)
        {
            var attack = _phase1Sequence[_phase1Index];
            _phase1Index = (_phase1Index + 1) % _phase1Sequence.Length;
            return attack;
        }

        var phase2Attack = _phase2Sequence[_phase2Index];
        _phase2Index = (_phase2Index + 1) % _phase2Sequence.Length;

        if (phase2Attack == "SummonClone" && HasActiveClone())
        {
            return GetNextAttackName();
        }

        return phase2Attack;
    }

    private void OnAttackFinished()
    {
        GD.Print("[FileCypher] Attack finished.");
        _queuedAttackFinished = true;
    }

    private void OnCloneDied()
    {
        if (_activeClone != null)
        {
            _activeClone.CloneDied -= OnCloneDied;
        }

        _activeClone = null;

        SetBossVulnerable(true);

        _attacks.ChangeState("AttackIdle");
        _waitingForAttackToFinish = false;
        _queuedAttackFinished = false;
        _staggerRemaining = StaggerDuration;
    }

    private void SetBossVulnerable(bool vulnerable)
    {
        if (_bossHurtbox != null)
        {
            _bossHurtbox.Monitoring = vulnerable;
            _bossHurtbox.Monitorable = vulnerable;
        }

        if (_bossHurtboxShape != null)
        {
            _bossHurtboxShape.Disabled = !vulnerable;
        }
    }

    private void UpdatePhaseTwoVulnerability()
    {
        if (!_isPhaseTwo)
        {
            return;
        }

        if (IsInPhaseTwoStaggerState())
        {
            SetBossVulnerable(true);
            return;
        }

        bool bothHeld = _topSwitch?.IsActive == true && _bottomSwitch?.IsActive == true;
        SetBossVulnerable(bothHeld);
    }

    private bool IsInPhaseTwoStaggerState()
    {
        return _attacks?.CurrentState?.Name == "PhaseTwoStagger";
    }
}
