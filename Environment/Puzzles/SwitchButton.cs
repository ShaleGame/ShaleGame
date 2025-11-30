using Godot;
namespace CrossDimensions.Environment.Puzzles;

public partial class SwitchButton : Area2D
{
    [Export]
    public Sprite2D Sprite { get; set; }
    [Export]
    private Texture2D PressedTexture;
    [Export]
    private Texture2D UnpressedTexture;
    [Export]
    public SwitchDoor Door { get; set; }
    public bool SwitchPressed { get; set; } = false;
    public override void _Ready()
    {
        this.BodyEntered += OnArea2DBodyEntered;
        this.BodyExited += OnArea2DBodyExited;
        if (Door == null)
        {
            GD.PrintErr("SwitchButton could not find parent SwitchDoor!");
        }
        GD.Print("SwitchButton ready!");
    }
    private void OnArea2DBodyEntered(Node body)
    {
        if (body is CharacterBody2D)
        {
            GD.Print("Switch area entered by body!");
            SwitchPressed = true;
            Sprite.Texture = PressedTexture;
            Door?.Activate();
        }
    }
    private void OnArea2DBodyExited(Node body)
    {
        GD.Print("Switch area left by body!");
        if (body is CharacterBody2D)
        {
            SwitchPressed = false;
            Sprite.Texture = UnpressedTexture;
            Door?.Activate();
        }
    }
}