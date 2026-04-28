using System;
using Godot;

namespace CrossedDimensions.Environment.Cutscene.Interactables;

/// <summary>
/// Base example for interactable objects
/// </summary>

public partial class Interactable : Area2D
{
    [Export]
    public bool AutoInstantiateUi { get; set; } = true;

    [Export]
    public PackedScene InteractableUiScene { get; set; }

    [Export]
    public Sprite2D Sprite { get; set; }

    [Export]
    public string InteractText { get; set; } = "Interact";

    [Export]
    public float HoldSecs { get; set; } = 0.5f;

    [Export]
    public bool InteractAllowed { get; set; } = false;

    [Export]
    public StringName InteractAction { get; set; } = "interact";

    [Export]
    public int InteractPriority { get; set; } = 0;
    [Export]
    public bool ShowSparkle { get; set; } = true;
    [Export]
    public GpuParticles2D SparkleParticle { get; set; }

    public float HoldTimer { get; private set; } = 0f;
    private bool _sendSignalInteractAvailable { get; set; } = false;
    private bool _sendSignalHoldUI { get; set; } = false;

    [Signal]
    public delegate void InteractedEventHandler();

    [Signal]
    public delegate void DisplayingHoldUIEventHandler();

    [Signal]
    public delegate void InteractAvailableEventHandler();

    [Signal]
    public delegate void InteractAvailabilityChangedEventHandler(bool available, string promptText);

    [Signal]
    public delegate void InteractProgressChangedEventHandler(bool isHolding, float progress);

    private bool _lastInteractAllowed;
    private bool _lastIsHolding;
    private float _lastHoldProgress = -1f;

    public override void _Ready()
    {
        if (!AutoInstantiateUi || InteractableUiScene is null)
        {
            return;
        }

        var uiInstance = InteractableUiScene.Instantiate();
        AddChild(uiInstance);

        if (uiInstance is InteractablePromptUi promptUi)
        {
            promptUi.SetInteractable(this);
            return;
        }

        GD.PushWarning($"Interactable '{Name}' instantiated UI scene '{InteractableUiScene.ResourcePath}', but root node does not inherit {nameof(InteractablePromptUi)}.");
    }

    internal void OnArea2DBodyEntered(Node body)
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

        // The body itself is a non-clone player, so at least one valid body is overlapping.
        InteractAllowed = true;
    }

    internal void OnArea2DBodyExited(Node body)
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

        foreach (var overlapping in GetOverlappingBodies())
        {
            if (overlapping is not Characters.Character ch)
            {
                continue;
            }

            if (!ch.IsInGroup("Player") || (ch.Cloneable?.IsClone ?? false))
            {
                continue;
            }

            return; // non-clone player still present
        }

        InteractAllowed = false;
    }

    public override void _Process(double delta)
    {
        SparkleParticle.Emitting = ShowSparkle;
        EmitStateSignalsIfNeeded();

        if (!InteractAllowed)
        {
            _sendSignalInteractAvailable = false;
            _sendSignalHoldUI = false;
            HoldTimer = 0f;
            EmitStateSignalsIfNeeded();
            return;
        }
        if (_sendSignalInteractAvailable == false)
        {
            //send signal only once
            SignalInteractAvailable();
        }

        if (Input.IsActionPressed(InteractAction))
        {
            if (_sendSignalHoldUI == false && HoldTimer > 0 && InteractAllowed)
            {
                //send signal only once
                SignalHoldUI();
            }

            HoldTimer += (float)delta;
            EmitStateSignalsIfNeeded();

            if (HoldTimer >= HoldSecs)
            {
                HoldTimer = 0f;
                _sendSignalHoldUI = false;
                EmitStateSignalsIfNeeded();
                //force release to keep _holdTimer at 0
                Input.ActionRelease(InteractAction);
                Interact();
            }
        }
        else
        {
            HoldTimer = 0f;
            _sendSignalHoldUI = false;
            EmitStateSignalsIfNeeded();
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

    public string GetInteractPromptText()
    {
        var actionText = GetInteractActionDisplayText();
        return string.IsNullOrWhiteSpace(InteractText)
            ? actionText
            : $"[{actionText}] {InteractText}";
    }

    public float GetHoldProgress()
    {
        if (HoldSecs <= 0f)
        {
            return InteractAllowed ? 1f : 0f;
        }

        return Mathf.Clamp(HoldTimer / HoldSecs, 0f, 1f);
    }

    public bool IsHoldingToInteract()
    {
        return InteractAllowed && HoldTimer > 0f;
    }

    private string GetInteractActionDisplayText()
    {
        var keyBinds = InputMap.ActionGetEvents(InteractAction);
        foreach (var inputEvent in keyBinds)
        {
            if (inputEvent is InputEventKey keyEvent)
            {
                return keyEvent.AsTextPhysicalKeycode();
            }

            if (inputEvent is InputEventJoypadButton joypadButton)
            {
                return joypadButton.AsText();
            }

            if (inputEvent is InputEventMouseButton mouseButton)
            {
                return mouseButton.AsText();
            }
        }

        return InteractAction.ToString();
    }

    private void EmitStateSignalsIfNeeded()
    {
        if (_lastInteractAllowed != InteractAllowed)
        {
            _lastInteractAllowed = InteractAllowed;
            EmitSignal(SignalName.InteractAvailabilityChanged, InteractAllowed, GetInteractPromptText());
        }

        var isHolding = IsHoldingToInteract();
        var holdProgress = isHolding ? GetHoldProgress() : 0f;
        if (_lastIsHolding == isHolding && Mathf.IsEqualApprox(_lastHoldProgress, holdProgress))
        {
            return;
        }

        _lastIsHolding = isHolding;
        _lastHoldProgress = holdProgress;
        EmitSignal(SignalName.InteractProgressChanged, isHolding, holdProgress);
    }
}
