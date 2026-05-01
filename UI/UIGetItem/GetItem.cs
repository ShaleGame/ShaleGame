using Godot;
using CrossedDimensions.Environment.Cutscene;
using CrossedDimensions.Items;
using System;

namespace CrossedDimensions.UI.UIGetItem;

[GlobalClass]
public partial class GetItem : UIDialogueBox.DialogueBox
{
    [Export]
    public ItemData Item { get; set; }

    public DialogueReel ItemGetReel { get; set; }

    [Signal]
    public delegate void CloseWeaponUIEventHandler();

    //create a single frame DialogueReel to feed to the DialoguePlayer
    private DialogueReel CreateReelFromWeaponData(ItemData data)
    {
        if (data == null)
        {
            throw new NullReferenceException("GetItem.cs: Failed to create DialogueReel, no ItemData provided");
        }

        DialogueReel reel = new DialogueReel();
        DialogueFrame frame = new DialogueFrame();
        frame.Portrait = new Texture2D[1];
        reel.Frames = new DialogueFrame[1];

        frame.Text = data.Description;
        frame.Speaker = data.Name;
        frame.Portrait[0] = data.Icon;
        reel.Frames[0] = frame;

        return reel;
    }

    public void StartItemGet()
    {
        ItemGetReel = CreateReelFromWeaponData(Item);
        OpenDialogue(ItemGetReel);
    }

    protected override void OnDialogueEnding()
    {
        EmitSignal(SignalName.CloseWeaponUI);
        GD.Print("Emitting close weapon UI signal");
        base.OnDialogueEnding();
    }
}
