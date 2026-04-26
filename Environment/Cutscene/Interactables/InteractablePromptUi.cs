using Godot;

namespace CrossedDimensions.Environment.Cutscene.Interactables;

[GlobalClass]
public partial class InteractablePromptUi : Node2D
{
    [Export]
    public Interactable Interactable { get; set; }

    [Export]
    public Label PromptLabel { get; set; }

    [Export]
    public ProgressBar HoldProgressBar { get; set; }

    [Export]
    public Vector2 WorldOffset { get; set; } = new(0f, -40f);

    public override void _Ready()
    {
        TopLevel = true;
        ConnectToInteractable(Interactable);
        ApplyCurrentState();
    }

    public override void _Process(double delta)
    {
        if (!IsInstanceValid(Interactable))
        {
            return;
        }

        GlobalPosition = Interactable.GlobalPosition + WorldOffset;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            ConnectToInteractable(null);
        }
    }

    public void SetInteractable(Interactable interactable)
    {
        ConnectToInteractable(interactable);
        ApplyCurrentState();
    }

    private void ConnectToInteractable(Interactable interactable)
    {
        if (IsInstanceValid(Interactable))
        {
            Interactable.InteractAvailabilityChanged -= OnInteractAvailabilityChanged;
            Interactable.InteractProgressChanged -= OnInteractProgressChanged;
        }

        Interactable = interactable;

        if (!IsInstanceValid(Interactable))
        {
            Visible = false;
            return;
        }

        Interactable.InteractAvailabilityChanged += OnInteractAvailabilityChanged;
        Interactable.InteractProgressChanged += OnInteractProgressChanged;
    }

    private void ApplyCurrentState()
    {
        if (!IsInstanceValid(Interactable))
        {
            Visible = false;
            return;
        }

        OnInteractAvailabilityChanged(Interactable.InteractAllowed, Interactable.GetInteractPromptText());
        OnInteractProgressChanged(Interactable.IsHoldingToInteract(), Interactable.GetHoldProgress());
    }

    private void OnInteractAvailabilityChanged(bool available, string promptText)
    {
        Visible = available;

        if (PromptLabel is not null)
        {
            PromptLabel.Text = promptText ?? string.Empty;
        }

        if (!available && HoldProgressBar is not null)
        {
            HoldProgressBar.Visible = false;
            HoldProgressBar.Value = 0;
        }
    }

    private void OnInteractProgressChanged(bool isHolding, float progress)
    {
        if (HoldProgressBar is null)
        {
            return;
        }

        HoldProgressBar.Visible = isHolding;
        HoldProgressBar.Value = progress * HoldProgressBar.MaxValue;
    }
}
