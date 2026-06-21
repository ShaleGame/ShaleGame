using Godot;
using CrossedDimensions.Environment.Cutscene;
using System.Linq;

namespace CrossedDimensions.UI.UIDialogueBox;

[GlobalClass]
public partial class DialogueBox : Control
{
    [Export]
    protected DialoguePlayer _dialoguePlayer;

    protected bool dialougeIsDone;

    protected double _timer = 0;

    [Export]
    public double VisibleCharacterTime { get; set; } = 0.025; //50ms

    [Export]
    public Label DialogueBodyLabel { get; set; }

    [Export]
    public TextureRect DialogueSpeakerPortrait { get; set; }

    [Export]
    public Label DialogueSpeakerName { get; set; }

    public override void _Ready()
    {
        dialougeIsDone = true;
        _dialoguePlayer.Ending += OnDialogueEnding;
        DialogueBodyLabel.VisibleCharacters = 0;
    }
    public override void _Process(double _delta)
    {
        // if visible character count < target char count &&
        if (dialougeIsDone)
        {
            return;
        }

        if (DialogueBodyLabel.VisibleRatio < 1 && (_timer -= _delta) <= 0)
        {
            //runs ~every 80ms
            _timer += VisibleCharacterTime;
            // increase visible char count by 1
            //revael one additonal chareacter unless dialougeIsDone == 1

            DialogueBodyLabel.VisibleCharacters++;
        }
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event.IsActionPressed("interact"))
        {
            if (dialougeIsDone)
            {
                CloseDialogue();
            }
            else //dialouge is not done
            {
                if (DialogueBodyLabel.VisibleRatio < 1)
                {
                    DialogueBodyLabel.VisibleRatio = 1;
                }
                else
                {
                    AdvanceDialogue();
                }
            }
            GetTree().Root.SetInputAsHandled();
        }
    }

    public void OpenDialogue(DialogueReel reel)
    {
        dialougeIsDone = false;
        if (!_dialoguePlayer.StartDialogue(reel, this))
        {
            dialougeIsDone = true;
            return;
        }
        //pause sceen tree
        GetTree().Paused = true;
        //display _dialougePlayer.targetText to the dialouge box
        UpdateDialogueBox();
        _timer = 0;

        Show();
    }
    protected void CloseDialogue()
    {
        //unpause sceene tree
        GetTree().Paused = false;
        //hide dialouge box 
        Hide();
    }

    //for manually advancing the dialouge one Dialouge frame at a time
    //the defualt behavior is that the dialouge advances char by char automatacly
    //when a full DiolugeFrame has been displasyed the dialog box waits for player input
    //to advance
    protected void AdvanceDialogue()
    {
        if (_dialoguePlayer.AdvanceText())
        {
            UpdateDialogueBox();
            _timer = 0;
        }
    }
    private void UpdateDialogueBox()
    {
        DialogueBodyLabel.VisibleRatio = 0;
        SetPortrait();
        SetDialogueText();
        SetSpeakerName();
    }
    //.called when the DialougeReel ends	
    protected virtual void OnDialogueEnding()//done
    {
        dialougeIsDone = true;
    }
    private void SetDialogueText()//done
    {
        //loads the entire current dialouge frame onto the label 
        DialogueBodyLabel.Text = _dialoguePlayer.targetText;
    }
    private void SetPortrait()
    {
        //sets the portriat of the speaker from the current Dialogue frame
        var portrait = _dialoguePlayer.CurrentFrame.Portrait;
        if (portrait?.Length > 0)
        {
            DialogueSpeakerPortrait.Texture = _dialoguePlayer.CurrentFrame.Portrait[0];
            DialogueSpeakerPortrait.Show();
        }
        else
        {
            DialogueSpeakerPortrait.Texture = null;
            DialogueSpeakerPortrait.Hide();
        }
    }
    private void SetSpeakerName()
    {
        //sets the name for the speaker to the speaker fo the current Dialogue frame
        //should be called whenever a frame ends ie en
        if (!string.IsNullOrEmpty(_dialoguePlayer.CurrentFrame.Speaker))
        {
            DialogueSpeakerName.Text = _dialoguePlayer.CurrentFrame.Speaker;
            DialogueSpeakerName.Show();
        }
        else
        {
            DialogueSpeakerName.Text = null;
            DialogueSpeakerName.Hide();
        }
    }
}
