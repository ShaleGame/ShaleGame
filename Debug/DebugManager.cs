using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrossedDimensions.BoundingBoxes;
using CrossedDimensions.Characters;
using CrossedDimensions.Saves;
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
    private const string ToggleInspectorAction = "debug_toggle_inspector";

    public static DebugManager Instance { get; private set; }

    public bool GodModeEnabled { get; private set; }

    public bool NoclipEnabled { get; private set; }

    public bool InvisiblePlayerEnabled { get; private set; }

    private bool _hudVisible = true;
    private bool _debugPanelVisible;
    private bool _debugInspectorVisible;

    [Export]
    public Label DebugLabel { get; set; }

    [Export]
    public CanvasItem DebugPanel { get; set; }

    [Export]
    public Window DebugInspector { get; set; }

    private readonly Dictionary<ulong, (uint Layer, uint Mask)> _collisionBackup = new();
    private VBoxContainer _saveInspectorRows;
    private Button _toggleDebugPanelButton;
    private Button _reloadInMemoryButton;
    private Button _reloadFromDiskButton;
    private Button _writeSaveToDiskButton;

    public override void _Ready()
    {
        Instance = this;
        Layer = 128;
        ProcessMode = ProcessModeEnum.Always;
        SetProcessUnhandledInput(true);
        DebugLabel ??= GetNodeOrNull<Label>("%DebugPanelText");
        DebugPanel ??= GetNodeOrNull<CanvasItem>("%DebugPanel");
        DebugInspector ??= GetNodeOrNull<Window>("%DebugInspectorWindow");
        _saveInspectorRows = GetNodeOrNull<VBoxContainer>("%SaveInspectorRows");
        _toggleDebugPanelButton = GetNodeOrNull<Button>("%ToggleDebugPanelButton");
        _reloadInMemoryButton = GetNodeOrNull<Button>("%ReloadInMemoryButton");
        _reloadFromDiskButton = GetNodeOrNull<Button>("%ReloadFromDiskButton");
        _writeSaveToDiskButton = GetNodeOrNull<Button>("%WriteSaveToDiskButton");

        WireInspectorButtons();

        _debugPanelVisible = false;
        SetPanelVisible(_debugPanelVisible);
        _debugInspectorVisible = false;
        SetInspectorVisible(_debugInspectorVisible);
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
        else if (@event.IsActionPressed(ToggleInspectorAction, false))
        {
            _debugInspectorVisible = !_debugInspectorVisible;
            SetInspectorVisible(_debugInspectorVisible);
            if (_debugInspectorVisible)
            {
                RefreshSaveInspector();
            }
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

    private void SetInspectorVisible(bool visible)
    {
        if (DebugInspector is not null)
        {
            DebugInspector.Visible = visible;
        }
    }

    private void WireInspectorButtons()
    {
        if (_toggleDebugPanelButton is not null)
        {
            _toggleDebugPanelButton.Pressed += ToggleDebugPanelFromInspector;
        }

        if (_reloadInMemoryButton is not null)
        {
            _reloadInMemoryButton.Pressed += ReloadFromInMemorySave;
        }

        if (_reloadFromDiskButton is not null)
        {
            _reloadFromDiskButton.Pressed += ReloadFromDisk;
        }

        if (_writeSaveToDiskButton is not null)
        {
            _writeSaveToDiskButton.Pressed += WriteSaveToDisk;
        }
    }

    private void ToggleDebugPanelFromInspector()
    {
        _debugPanelVisible = !_debugPanelVisible;
        SetPanelVisible(_debugPanelVisible);
    }

    private async void ReloadFromInMemorySave()
    {
        var saveManager = SaveManager.Instance;
        var sceneManager = SceneManager.Instance;
        if (saveManager?.CurrentSave is null || sceneManager is null)
        {
            return;
        }

        Vector2 capturedPosition = sceneManager.GetPlayerPosition();
        sceneManager.ReloadCurrentSceneFromSave(saveManager.CurrentSave);
        await RestorePlayerPositionAfterReload(capturedPosition);
        RefreshSaveInspector();
    }

    private async void ReloadFromDisk()
    {
        var saveManager = SaveManager.Instance;
        var sceneManager = SceneManager.Instance;
        if (saveManager?.CurrentSave is null || sceneManager is null)
        {
            return;
        }

        string saveName = saveManager.CurrentSave.SaveName;
        if (string.IsNullOrEmpty(saveName))
        {
            return;
        }

        Vector2 capturedPosition = sceneManager.GetPlayerPosition();
        SaveFile reloadedSave = saveManager.ReadPersistentFromName(saveName);
        sceneManager.ReloadCurrentSceneFromSave(reloadedSave);
        await RestorePlayerPositionAfterReload(capturedPosition);
        RefreshSaveInspector();
    }

    private void WriteSaveToDisk()
    {
        if (SaveManager.Instance?.CurrentSave is null)
        {
            return;
        }

        SaveManager.Instance.WritePersistent();
    }

    private async Task RestorePlayerPositionAfterReload(Vector2 capturedPosition)
    {
        for (int i = 0; i < 120; i++)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            Character player = GetActivePlayer();
            if (player is not null)
            {
                player.GlobalPosition = capturedPosition;
                return;
            }
        }
    }

    private void RefreshSaveInspector()
    {
        if (_saveInspectorRows is null)
        {
            return;
        }

        SaveFile save = SaveManager.Instance?.CurrentSave;

        foreach (Node child in _saveInspectorRows.GetChildren())
        {
            child.QueueFree();
        }

        if (save is null)
        {
            var emptyLabel = new Label { Text = "No in-memory save loaded." };
            _saveInspectorRows.AddChild(emptyLabel);
            return;
        }

        var keys = save.KeyValue.Keys
            .Select(k => k.AsString())
            .OrderBy(k => k)
            .ToList();

        foreach (string key in keys)
        {
            Variant value = save.KeyValue[key];
            AddSaveInspectorRow(key, value);
        }
    }

    private void AddSaveInspectorRow(string key, Variant value)
    {
        var row = new HBoxContainer();
        row.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

        var keyLabel = new Label();
        keyLabel.Text = key;
        keyLabel.CustomMinimumSize = new Vector2(220, 0);
        row.AddChild(keyLabel);

        switch (value.VariantType)
        {
            case Variant.Type.String:
                {
                    var editor = new LineEdit();
                    editor.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                    editor.Text = value.AsString();
                    editor.TextChanged += text => SaveManager.Instance?.SetKey(key, text);
                    row.AddChild(editor);
                    break;
                }
            case Variant.Type.Int:
                {
                    var editor = new SpinBox();
                    editor.MinValue = int.MinValue;
                    editor.MaxValue = int.MaxValue;
                    editor.Step = 1;
                    editor.Value = value.AsInt32();
                    editor.ValueChanged += number => SaveManager.Instance?.SetKey(key, (int)Mathf.Round((float)number));
                    row.AddChild(editor);
                    break;
                }
            case Variant.Type.Float:
                {
                    var editor = new SpinBox();
                    editor.MinValue = -1_000_000;
                    editor.MaxValue = 1_000_000;
                    editor.Step = 0.1;
                    editor.Value = value.AsSingle();
                    editor.ValueChanged += number => SaveManager.Instance?.SetKey(key, (float)number);
                    row.AddChild(editor);
                    break;
                }
            case Variant.Type.Bool:
                {
                    var editor = new CheckBox();
                    editor.ButtonPressed = value.AsBool();
                    editor.Toggled += toggled => SaveManager.Instance?.SetKey(key, toggled);
                    row.AddChild(editor);
                    break;
                }
            default:
                {
                    var unsupported = new Label();
                    unsupported.Text = $"Unsupported: {value.VariantType}";
                    row.AddChild(unsupported);
                    break;
                }
        }

        _saveInspectorRows.AddChild(row);
    }
}
