using CrossedDimensions.Characters;
using CrossedDimensions.UI.UIDialogueBox;
using Godot;
using System.Linq;

namespace CrossedDimensions.Environment.Cutscene;

[GlobalClass]
public partial class DialogueListener : Node
{
    private DialogueBox _dialogueBox;
    private CutscenePlayer _cutscenePlayer;
    private Interactables.Interactable _interactable;
    private bool _interactableSubscribed;
    private bool _cutscenePlayerSubscribed;
    private bool _readyCompleted;

    [Export]
    public Interactables.Interactable Interactable
    {
        get => _interactable;
        set
        {
            if (ReferenceEquals(_interactable, value))
            {
                return;
            }

            DisconnectInteractable();
            _interactable = value;

            if (_readyCompleted)
            {
                ConnectInteractable();
            }
        }
    }

    [Export]
    public DialogueReel DialogueReel { get; set; }

    public DialogueBox DialogueBox
    {
        get => _dialogueBox;
        set => _dialogueBox = value;
    }

    public CutscenePlayer CutscenePlayer
    {
        get => _cutscenePlayer;
        set
        {
            if (ReferenceEquals(_cutscenePlayer, value))
            {
                return;
            }

            DisconnectCutscenePlayer();
            _cutscenePlayer = value;

            if (_readyCompleted)
            {
                ConnectCutscenePlayer();
            }
        }
    }

    public override void _Ready()
    {
        _readyCompleted = true;
        ConnectInteractable();
        ConnectCutscenePlayer();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            DisconnectInteractable();
            DisconnectCutscenePlayer();
        }
    }

    private void OnInteracted()
    {
        InvokeDialogue(DialogueReel);
    }

    private void OnCutsceneDialogueRequested()
    {
        if (_cutscenePlayer?.RequestedDialogueReel is null)
        {
            return;
        }

        DialogueReel = _cutscenePlayer.RequestedDialogueReel;
    }

    private void ConnectInteractable()
    {
        if (_interactableSubscribed || Interactable is null)
        {
            return;
        }

        Interactable.Interacted += OnInteracted;
        _interactableSubscribed = true;
    }

    private void DisconnectInteractable()
    {
        if (!_interactableSubscribed || _interactable is null)
        {
            return;
        }

        _interactable.Interacted -= OnInteracted;
        _interactableSubscribed = false;
    }

    private void ConnectCutscenePlayer()
    {
        if (_cutscenePlayerSubscribed || CutscenePlayer is null)
        {
            return;
        }

        CutscenePlayer.DialogueRequested += OnCutsceneDialogueRequested;
        _cutscenePlayerSubscribed = true;
    }

    private void DisconnectCutscenePlayer()
    {
        if (!_cutscenePlayerSubscribed || _cutscenePlayer is null)
        {
            return;
        }

        _cutscenePlayer.DialogueRequested -= OnCutsceneDialogueRequested;
        _cutscenePlayerSubscribed = false;
    }

    private void InvokeDialogue(DialogueReel reel)
    {
        var originalCharacter = GetTree().GetNodesInGroup("Player")
            .OfType<Character>()
            .Where((x) => x.Cloneable is not null)
            .Select((x) => x.Cloneable)
            .First((cloneable) => !cloneable.IsClone);

        var hud = originalCharacter.GetNode("%PlayerHud");

        var dialogueBox = hud.GetNode<DialogueBox>("DialogueBox");
        dialogueBox.OpenDialogue(reel);
    }
}
