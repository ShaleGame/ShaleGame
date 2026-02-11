using Godot;

namespace CrossedDimensions.Characters;

/// <summary>
/// AnimationPlayer script for the player character. This script is used to
/// control the player's animations based on the current state of the
/// character.
/// </summary>
public partial class PlayerAnimator : AnimationPlayer
{
    [Export]
    public Character PlayerCharacter { get; set; }

    [Export]
    public Node2D BodyAnchor { get; set; }

    [Export]
    public Node2D ArmAnchor { get; set; }

    public override void _Ready()
    {
        PlayerCharacter.MovementStateMachine.StateChanged += OnMovementStateChanged;
    }

    private void OnMovementStateChanged(States.State newState)
    {
        if (newState is States.Characters.CharacterIdleState)
        {
            // play anim
            //Play("idle");
        }
        else if (newState is States.Characters.CharacterMoveState)
        {

        }
        else if (newState is States.Characters.CharacterAirState)
        {

        }
    }

    public override void _Process(double delta)
    {
        // flip the sprite depending on the character's target
        // and also rotate the arm to point towards the target

        Vector2 dir = PlayerCharacter.Controller.Target.Normalized();
        float angle = dir.Angle();

        if (dir.X < 0)
        {
            BodyAnchor.Scale = new Vector2(-1, 1);
            angle = Mathf.Pi - angle;
        }
        else
        {
            BodyAnchor.Scale = new Vector2(1, 1);
        }

        ArmAnchor.Rotation = angle;
    }
}
