using CrossedDimensions.Characters;
using Godot;

namespace CrossedDimensions.Environment.Triggers;

[GlobalClass]
public partial class JumpCrystal : Area2D
{
    [Export]
    public AnimationPlayer ActivationAnimationPlayer { get; set; }

    [Export]
    public StringName ActivateAnimation { get; set; } = "activate";

    [Export]
    public StringName DeactivateAnimation { get; set; } = "deactivate";

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    public override void _ExitTree()
    {
        BodyEntered -= OnBodyEntered;
        BodyExited -= OnBodyExited;
    }

    private void OnBodyEntered(Node body)
    {
        if (!TryGetOriginalPlayer(body, out var character))
        {
            return;
        }

        character.AllowJumpInput = true;
        character.AllowMidAirJump = true;
        PlayAnimation(ActivateAnimation);
    }

    private void OnBodyExited(Node body)
    {
        if (!TryGetOriginalPlayer(body, out var character))
        {
            return;
        }

        PlayAnimation(DeactivateAnimation);

        if (!character.IsOnFloor() && character.AllowJumpInput && character.AllowMidAirJump)
        {
            character.AllowJumpInput = false;
            character.AllowMidAirJump = false;
        }
    }

    private bool TryGetOriginalPlayer(Node body, out Character character)
    {
        character = body as Character;

        if (character is null)
        {
            return false;
        }

        if (!character.IsInGroup("Player"))
        {
            return false;
        }

        if (character.Cloneable?.IsClone ?? true)
        {
            return false;
        }

        return true;
    }

    private void PlayAnimation(StringName animation)
    {
        if (ActivationAnimationPlayer is null)
        {
            return;
        }

        ActivationAnimationPlayer.Stop();
        ActivationAnimationPlayer.Play(animation);
    }
}
