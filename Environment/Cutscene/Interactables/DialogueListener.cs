using CrossedDimensions.Characters;
using CrossedDimensions.UI.UIDialogueBox;
using Godot;
using System.Linq;

namespace CrossedDimensions.Environment.Cutscene.Interactables;

[GlobalClass]
public partial class DialogueListener : Node
{
    [Export]
    public Interactable Interactable { get; set; }

    [Export]
    public DialogueReel DialogueReel { get; set; }

    public override void _Ready()
    {
        Interactable.Interacted += OnInteracted;
    }

    private void OnInteracted()
    {
        InvokeDialogue(DialogueReel);
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
