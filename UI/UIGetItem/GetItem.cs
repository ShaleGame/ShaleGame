using Godot;
using CrossedDimensions.Environment.Cutscene;
using CrossedDimensions.Items;

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
        if (data == null)
        {
            GD.PushWarning("GetItem.cs: No ItemData provided, could not create DialogueFrame");
        }
        
        DialogueReel reel = new DialogueReel();
        DialogueFrame frame = new DialogueFrame();

        frame.Portrait = new Texture2D[1];
        Texture2D icon = data.Icon;
        frame.Portrait[0] = icon;    
        frame.Text = data.Description;
        frame.Speaker = data.Name;

        reel.Frames = new DialogueFrame[1];
        reel.Frames[0] = frame;

        return reel;
    }

    public void StartItemGet()
    {
        ItemGetReel = CreateReelFromWeaponData( Item );
        OpenDialogue( ItemGetReel );
    }
}
