using System.Collections.Generic;
using System.Linq;
using CrossedDimensions.BoundingBoxes;
using CrossedDimensions.Characters;
using CrossedDimensions.States;
using Godot;

namespace CrossedDimensions.Debugging;

[GlobalClass]
public partial class DebugManager : CanvasLayer
{
    private const string ToggleGodModeAction = "debug_toggle_god_mode";
    private const string ToggleNoclipAction = "debug_toggle_noclip";
    private const string ToggleInvisibleAction = "debug_toggle_invisible";
    private const string ToggleHudAction = "debug_toggle_hud";
    private const string TogglePanelAction = "debug_toggle_panel";

    public static DebugManager Instance { get; private set; }

    public bool GodModeEnabled { get; private set; }

    public bool NoclipEnabled { get; private set; }

    public bool InvisiblePlayerEnabled { get; private set; }

    private bool _hudVisible = true;
    private bool _debugPanelVisible;

    [Export]
    public Label DebugLabel { get; set; }

    [Export]
    public CanvasItem DebugPanel { get; set; }

    private readonly Dictionary<ulong, (uint Layer, uint Mask)> _collisionBackup = new();

    public override void _Ready()
    {
        Instance = this;
        Layer = 128;
        ProcessMode = ProcessModeEnum.Always;
        SetProcessUnhandledInput(true);
        DebugLabel ??= GetNodeOrNull<Label>("DebugPanel/Margin/DebugText");
        DebugPanel ??= GetNodeOrNull<CanvasItem>("DebugPanel");
        _debugPanelVisible = false;
        SetPanelVisible(_debugPanelVisible);
        UpdatePanel();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!HasDebugAccess() || @event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
        {
            return;
        }

        if (@event.IsActionPressed(TogglePanelAction, false))
        {
            _debugPanelVisible = !_debugPanelVisible;
            SetPanelVisible(_debugPanelVisible);
        }
        else if (@event.IsActionPressed(ToggleGodModeAction, false))
        {
            GodModeEnabled = !GodModeEnabled;
        }
        else if (@event.IsActionPressed(ToggleNoclipAction, false))
        {
            NoclipEnabled = !NoclipEnabled;
        }
        else if (@event.IsActionPressed(ToggleInvisibleAction, false))
        {
            InvisiblePlayerEnabled = !InvisiblePlayerEnabled;
        }
        else if (@event.IsActionPressed(ToggleHudAction, false))
        {
            _hudVisible = !_hudVisible;
        }
        else
        {
            return;
        }

        ApplyDebugState();
        UpdatePanel();
        GetViewport().SetInputAsHandled();
    }

    public override void _Process(double delta)
    {
        ApplyDebugState();
        UpdatePanel();
    }

    private bool HasDebugAccess()
    {
        return true;
    }

    private void ApplyDebugState()
    {
        var player = GetActivePlayer();
        if (player is null)
        {
            return;
        }

        ApplyCollisionState(player);
        ApplyHurtboxState(player);
        ApplyVisibilityState(player);
        ApplyMovementState(player);
        ApplyHudState(player);
    }

    private Character GetActivePlayer()
    {
        return GetTree()
            .GetNodesInGroup("Player")
            .OfType<Character>()
            .OrderBy(p => p.Cloneable?.IsClone ?? false)
            .FirstOrDefault();
    }

    private void ApplyCollisionState(Character player)
    {
        ulong id = player.GetInstanceId();
        if (!_collisionBackup.ContainsKey(id))
        {
            _collisionBackup[id] = (player.CollisionLayer, player.CollisionMask);
        }

        if (NoclipEnabled)
        {
            player.CollisionLayer = 0;
            player.CollisionMask = 0;
            return;
        }

        if (_collisionBackup.TryGetValue(id, out var backup))
        {
            player.CollisionLayer = backup.Layer;
            player.CollisionMask = backup.Mask;
        }
    }

    private void ApplyHurtboxState(Character player)
    {
        var hurtbox = player.GetNodeOrNull<Hurtbox>("Hurtbox");
        bool disableHurtbox = GodModeEnabled || NoclipEnabled;
        if (hurtbox is null)
        {
            return;
        }

        hurtbox.Monitoring = !disableHurtbox;
        hurtbox.Monitorable = !disableHurtbox;

        var shape = hurtbox.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        if (shape is not null)
        {
            shape.Disabled = disableHurtbox;
        }
    }

    private void ApplyVisibilityState(Character player)
    {
        player.Visible = !InvisiblePlayerEnabled;
    }

    private void ApplyMovementState(Character player)
    {
        if (player.MovementStateMachine is null)
        {
            return;
        }

        State noclipState = player.MovementStateMachine.GetNodeOrNull<State>("Noclip State");
        State idleState = player.MovementStateMachine.GetNodeOrNull<State>("Idle State");
        State current = player.MovementStateMachine.CurrentState;

        if (NoclipEnabled)
        {
            if (noclipState is not null && current != noclipState)
            {
                player.MovementStateMachine.ChangeState(noclipState);
            }
        }
        else if (current == noclipState && idleState is not null)
        {
            player.MovementStateMachine.ChangeState(idleState);
        }
    }

    private void ApplyHudState(Character player)
    {
        var hud = player.GetNodeOrNull<CanvasItem>("CanvasLayer/PlayerHud");
        if (hud is not null)
        {
            hud.Visible = _hudVisible;
        }
    }

    private void UpdatePanel()
    {
        if (DebugLabel is null)
        {
            return;
        }

        Character player = GetActivePlayer();
        string positionText = player is null
            ? "N/A"
            : $"({player.GlobalPosition.X:0.00}, {player.GlobalPosition.Y:0.00})";

        string sceneName = GetTree().CurrentScene?.Name ?? "N/A";

        DebugLabel.Text =
            $"God Mode: {(GodModeEnabled ? "On" : "Off")}\n" +
            $"Noclip: {(NoclipEnabled ? "On" : "Off")}\n" +
            $"Invisible Player: {(InvisiblePlayerEnabled ? "On" : "Off")}\n" +
            $"Position: {positionText}\n" +
            $"Scene: {sceneName}";
    }

    private void SetPanelVisible(bool visible)
    {
        if (DebugPanel is not null)
        {
            DebugPanel.Visible = visible;
        }
    }
}
