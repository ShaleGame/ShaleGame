using CrossedDimensions.Environment.Cutscene;
using CrossedDimensions.Saves;
using Godot;

namespace CrossedDimensions.Environment.Triggers;

[GlobalClass]
public partial class CutsceneTrigger : Area2D
{
    private bool _isConsumed;
    private bool _requiresPlayerExitBeforeRetrigger;

    [Export]
    public CutsceneMetadata Cutscene { get; set; }

    [Export]
    public Marker2D ReturnPlayerMarker { get; set; }

    [Export]
    public string SaveKey { get; set; } = "";

    [Export]
    public bool DisableAfterPlaying { get; set; }

    [Export]
    public bool DestroyAfterPlaying { get; set; }

    [Export]
    public bool DisableImmediatelyOnTrigger { get; set; }

    public override void _Ready()
    {
        RestoreConsumedState();
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            BodyEntered -= OnBodyEntered;
            BodyExited -= OnBodyExited;
        }
    }

    private void OnBodyEntered(Node body)
    {
        if (body is not Characters.Character character)
        {
            return;
        }

        if (!character.IsInGroup("Player"))
        {
            return;
        }

        if (character.Cloneable?.IsClone ?? false)
        {
            character.Cloneable.Merge();
            return;
        }

        if (_isConsumed || _requiresPlayerExitBeforeRetrigger)
        {
            return;
        }

        if (SceneManager.Instance is null)
        {
            GD.PushWarning(
                $"CutsceneTrigger '{Name}' could not find SceneManager.");
            return;
        }

        if (Cutscene is null)
        {
            GD.PushWarning(
                $"CutsceneTrigger '{Name}' does not have cutscene " +
                "metadata assigned.");
            return;
        }

        _requiresPlayerExitBeforeRetrigger = true;

        if (DisableImmediatelyOnTrigger && DisableAfterPlaying)
        {
            ConsumeAfterPlaying(
                destroyAfterPlaying: false,
                disableAfterPlaying: true);
        }

        SceneManager.Instance.CallDeferred(
            nameof(SceneManager.PlayCutsceneSync),
            CreateRuntimeCutsceneMetadata(),
            true);
    }

    private void OnBodyExited(Node body)
    {
        if (body is not Characters.Character character)
        {
            return;
        }

        if (!character.IsInGroup("Player"))
        {
            return;
        }

        if (character.Cloneable?.IsClone ?? false)
        {
            return;
        }

        if (_isConsumed)
        {
            return;
        }

        _requiresPlayerExitBeforeRetrigger = false;
    }

    public void ConsumeAfterPlaying(
        bool destroyAfterPlaying,
        bool disableAfterPlaying)
    {
        if (_isConsumed)
        {
            return;
        }

        _isConsumed = true;
        PersistConsumedState();

        if (!GodotObject.IsInstanceValid(this) || IsQueuedForDeletion())
        {
            return;
        }

        if (destroyAfterPlaying)
        {
            Monitoring = false;
            Monitorable = false;
            GetParent()?.RemoveChild(this);
            Free();
            return;
        }

        if (disableAfterPlaying)
        {
            Monitoring = false;
            Monitorable = false;
        }
    }

    private void RestoreConsumedState()
    {
        if (string.IsNullOrEmpty(SaveKey)
            || SaveManager.Instance?.CurrentSave is null)
        {
            return;
        }

        if (!SaveManager.Instance.TryGetKey<bool>(SaveKey, out var isConsumed)
            || !isConsumed)
        {
            return;
        }

        _isConsumed = true;

        if (DestroyAfterPlaying)
        {
            CallDeferred(nameof(RemoveAndFree));
            return;
        }

        Monitoring = false;
        Monitorable = false;
    }

    private void PersistConsumedState()
    {
        if (string.IsNullOrEmpty(SaveKey)
            || SaveManager.Instance?.CurrentSave is null)
        {
            return;
        }

        SaveManager.Instance.SetKey(SaveKey, true);
    }

    private void RemoveAndFree()
    {
        if (!GodotObject.IsInstanceValid(this) || IsQueuedForDeletion())
        {
            return;
        }

        Monitoring = false;
        Monitorable = false;
        GetParent()?.RemoveChild(this);
        Free();
    }

    private CutsceneMetadata CreateRuntimeCutsceneMetadata()
    {
        if (Cutscene is null)
        {
            return null;
        }

        var runtimeMetadata = new CutsceneMetadata
        {
            CutsceneScenePath = Cutscene.CutsceneScenePath,
            RepositionPlayerOnReturn = Cutscene.RepositionPlayerOnReturn,
            ReturnPlayerPosition = Cutscene.ReturnPlayerPosition,
            TriggerNodePath = GetTree().CurrentScene?.GetPathTo(this) ?? default,
            DisableTriggerAfterPlaying = DisableAfterPlaying,
            DestroyTriggerAfterPlaying = DestroyAfterPlaying
        };

        if (runtimeMetadata.RepositionPlayerOnReturn
            && ReturnPlayerMarker is not null)
        {
            runtimeMetadata.ReturnPlayerPosition =
                ReturnPlayerMarker.GlobalPosition;
        }

        return runtimeMetadata;
    }
}
