using Godot;
using CrossedDimensions.Environment.BossSystem;
using CrossedDimensions.Environment.Triggers;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class FileCypherTestController : Node2D
{
    [Export]
    public BossSystem BossSystem { get; set; }

    [Export]
    public Node2D PhaseTwoPlatformTop { get; set; }

    [Export]
    public Node2D PhaseTwoPlatformBottom { get; set; }

    [Export]
    public Trigger TopSwitch { get; set; }

    [Export]
    public Trigger BottomSwitch { get; set; }

    [ExportCategory("Boss Context Markers")]
    [Export]
    public Marker2D BossAirPathLeft { get; set; }

    [Export]
    public Marker2D BossAirPathRight { get; set; }

    [Export]
    public Marker2D BossBombRunStart { get; set; }

    [Export]
    public Marker2D BossBombRunEnd { get; set; }

    [Export]
    public Marker2D BossBombRunMarker { get; set; }

    [Export]
    public Marker2D BossCloneAnchorPoint { get; set; }

    [Export]
    public Marker2D BossTopSwitchTarget { get; set; }

    [Export]
    public Marker2D BossBottomSwitchTarget { get; set; }

    public override void _Ready()
    {
        SetPhaseTwoPlatformsActive(false);

        if (BossSystem != null)
        {
            BossSystem.BossSpawned += OnBossSpawned;
        }
    }

    private void OnBossSpawned()
    {
        var boss = BossSystem?.BossInstance;
        if (boss == null)
        {
            return;
        }

        InjectBossContext(boss);

        var spiral = boss.GetNodeOrNull<SpiralBulletHell>("Attacks/SpiralBulletHell");
        if (spiral != null)
        {
            spiral.TopSwitch = TopSwitch;
            spiral.BottomSwitch = BottomSwitch;
        }

        var switchPressure = boss.GetNodeOrNull<SwitchPressure>("Attacks/SwitchPressure");
        if (switchPressure != null)
        {
            switchPressure.TopSwitch = TopSwitch;
            switchPressure.BottomSwitch = BottomSwitch;
            switchPressure.TopTarget = BossTopSwitchTarget;
            switchPressure.BottomTarget = BossBottomSwitchTarget;
        }

        var phaseTransition = boss.GetNodeOrNull<PhaseTransition>("StateMachine/PhaseTransition");
        if (phaseTransition != null)
        {
            phaseTransition.PhaseChanged += OnBossPhaseChanged;
        }
    }

    private void OnBossPhaseChanged()
    {
        SetPhaseTwoPlatformsActive(true);
    }

    private void InjectBossContext(Character boss)
    {
        var sequencer = boss.GetNodeOrNull<CypherSequencer>("StateMachine/CypherSequencer");
        sequencer?.ConfigureAirPathMarkers(BossAirPathLeft, BossAirPathRight);
        sequencer?.ConfigurePhaseTwoSwitches(TopSwitch, BottomSwitch);

        var homingMissiles = boss.GetNodeOrNull<HomingMissiles>("Attacks/HomingMissiles");
        if (homingMissiles != null)
        {
            homingMissiles.BombRunStart = BossBombRunStart;
            homingMissiles.BombRunEnd = BossBombRunEnd;
            homingMissiles.BombRunMarker = BossBombRunMarker;
        }

        var summonClone = boss.GetNodeOrNull<SummonClone>("Attacks/SummonClone");
        if (summonClone != null)
        {
            summonClone.CloneAnchorPoint = BossCloneAnchorPoint;
        }

    }

    private void SetPhaseTwoPlatformsActive(bool active)
    {
        SetNodeActive(PhaseTwoPlatformTop, active);
        SetNodeActive(PhaseTwoPlatformBottom, active);
    }

    private static void SetNodeActive(Node node, bool active)
    {
        if (node == null)
        {
            return;
        }

        if (node is CanvasItem canvasItem)
        {
            canvasItem.Visible = active;
        }

        foreach (var child in node.GetChildren())
        {
            SetNodeActive(child as Node, active);
        }

        if (node is CollisionShape2D collisionShape)
        {
            collisionShape.Disabled = !active;
        }
    }
}
