using Castle.Components.DictionaryAdapter;
using Godot;
using NSubstitute.Core;

namespace CrossedDimensions.Environment.Cutscene.Interactables;

/// <summary>
/// Base example for interactable objects
/// </summary>

public partial class Interactable : Area2D
{
    [Export]
    public Sprite2D Sprite { get; set; }

    [Export]
    public float HoldSecs { get; set; } = 0.5f;

    [Export]
    public bool InteractAllowed { get; set; } = false;

    [Export]
    public StringName InteractAction { get; set; } = "interact";

    [Export]
    public int InteractPriority { get; set; } = 0;

    public float HoldTimer { get; private set; } = 0f;
    private bool _sendSignalInteractAvailable { get; set; } = false;
    private bool _sendSignalHoldUI { get; set; } = false;

    [Signal]
    public delegate void InteractedEventHandler();

    [Signal]
    public delegate void DisplayingHoldUIEventHandler();

    [Signal]
    public delegate void InteractAvailableEventHandler();

    internal void OnArea2DBodyEntered(Node body)
    {
        if (GetOverlappingBodies().Count > 0)
        {
            InteractAllowed = true;
        }
    }

    internal void OnArea2DBodyExited(Node body)
    {
        if (GetOverlappingBodies().Count == 0)
        {
            InteractAllowed = false;
        }
    }

    public override void _Process(double delta)
    {
        if (!InteractAllowed)
        {
            _sendSignalInteractAvailable = false;
            _sendSignalHoldUI = false;
            HoldTimer = 0f;
            return;
        }
        else
        {
            if (_sendSignalInteractAvailable == false)
            {
                //send signal only once
                SignalInteractAvailable();
            }
        }

        if (Input.IsActionPressed(InteractAction))
        {
            if (_sendSignalHoldUI == false)
            {
                //send signal only once
                SignalHoldUI();
            }

            HoldTimer += (float)delta;

            if (HoldTimer >= HoldSecs)
            {
                HoldTimer = 0f;
                _sendSignalHoldUI = false;
                //force release to keep _holdTimer at 0
                Input.ActionRelease(InteractAction);
                Interact();
            }
        }
        else
        {
            HoldTimer = 0f;
            _sendSignalHoldUI = false;
        }

        if (_sendSignalInteractAvailable == true)
        {
            var _keyName = "";
            var _keyBinds = InputMap.ActionGetEvents("interact");
            foreach (var i in _keyBinds)
            {
                if (i is InputEventKey)
                {
                    _keyName = ((InputEventKey)i).AsText();
                }
            }
            //set the keybind UI node to visible and set its text to _keyName

        }

        if (_sendSignalHoldUI == true)
        {
            //set the hold interact UI to visible and set the amount it is filled to _holdTimer / HoldSecs, if HoldSecs != 0
        }
    }

    protected virtual void Interact()
    {
        GD.Print($"Interacted with {Name}");
        EmitSignal(SignalName.Interacted);
    }

    protected virtual void SignalHoldUI()
    {
        EmitSignal(SignalName.DisplayingHoldUI);
        _sendSignalHoldUI = true;
    }

    protected virtual void SignalInteractAvailable()
    {
        EmitSignal(SignalName.InteractAvailable);
        _sendSignalInteractAvailable = true;
    }
}
