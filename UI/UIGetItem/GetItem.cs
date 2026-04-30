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

    //create a single frame DialogueReel to feed to the DialoguePlayer
    public DialogueReel CreateReelFromWeaponData(ItemData data)
    {
        DialogueReel reel = new DialogueReel();
        DialogueFrame frame = new DialogueFrame();
        frame.Portrait = new Texture2D[1];
        reel.Frames = new DialogueFrame[1];

        try
        {
            frame.Text = data.Description;
            frame.Speaker = data.Name;
            frame.Portrait[0] = data.Icon;
            reel.Frames[0] = frame;
        }
        catch
        {
            throw new NullReferenceException("GetItem.cs: Failed to create DialogueReel, no ItemData provided");
        }

        return reel;
    }

    public void StartItemGet()
    {
        ItemGetReel = CreateReelFromWeaponData(Item);
        OpenDialogue(ItemGetReel);
    }
}
