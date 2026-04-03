using CrossedDimensions.Characters;
using Godot;

namespace CrossedDimensions.Environment.Triggers;

[GlobalClass]
public partial class DashCrystal : Area2D
{
    [Export]
    public AnimationPlayer ActivationAnimationPlayer { get; set; }

    [Export]
    public StringName ActivationAnimation { get; set; } = "activate";

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    public override void _ExitTree()
    {
        BodyEntered -= OnBodyEntered;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is not Character character)
        {
            return;
        }

        if (!character.IsInGroup("Player"))
        {
            return;
        }

        if (character.Cloneable?.IsClone ?? true)
        {
            return;
        }

        if (!character.Cloneable.ResetSplitCooldown())
        {
            GD.PushWarning($"DashCrystal '{Name}' could not reset split cooldown.");
            return;
        }

        if (ActivationAnimationPlayer is not null)
        {
            ActivationAnimationPlayer.Stop();
            ActivationAnimationPlayer.Play(ActivationAnimation);
        }
    }
}
