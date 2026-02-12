using Godot;
namespace CrossedDimensions.Environment.Puzzles;

public partial class SwitchButton : Area2D
{
    [Export]
    public Sprite2D Sprite { get; set; }

    [Export]
    private Texture2D _pressedTexture;

    [Export]
    private Texture2D _unpressedTexture;

    [Export]
    public SwitchDoor Door { get; set; }

    /// <summary>
    /// If <c>true</c>, the switch will remain pressed after being activated,
    /// and will not deactivate when the player leaves. If <c>false</c>, the
    /// switch will deactivate when the player leaves.
    /// </summary>
    [Export]
    public bool StayPressed { get; set; } = false;

    public bool SwitchPressed { get; private set; } = false;

    private int _bodyCount = 0;

    /// <summary>
    /// The number of bodies currently activating the switch.
    /// </summary>
    public int BodyCount
    {
        get => _bodyCount;
        set
        {
            _bodyCount = value;

            // if there is at least one body on the switch, it should be pressed
            if (_bodyCount > 0)
            {
                SwitchPressed = true;
                Sprite.Texture = _pressedTexture;
            }
            else if (!StayPressed)
            {
                SwitchPressed = false;
                Sprite.Texture = _unpressedTexture;
            }

            // despite the name, it just updates the state of the door
            Door?.Activate();
        }
    }

    public override void _Ready()
    {
        this.BodyEntered += OnArea2DBodyEntered;
        this.BodyExited += OnArea2DBodyExited;
        if (Door == null)
        {
            throw new System.Exception("SwitchButton requires a SwitchDoor to function.");
        }
    }

    private void OnArea2DBodyEntered(Node body)
    {
        if (body is CharacterBody2D)
        {
            BodyCount++;
        }
    }

    private void OnArea2DBodyExited(Node body)
    {
        if (body is CharacterBody2D)
        {
            BodyCount--;
        }
    }
}
