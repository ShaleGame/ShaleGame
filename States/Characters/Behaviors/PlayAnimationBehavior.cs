using Godot;

namespace CrossedDimensions.States.Characters.Behaviors;

/// <summary>
/// A state behavior that plays an animation when the parent state is entered.
/// </summary>
[GlobalClass]
public partial class PlayAnimationBehavior : State
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    [Export]
    public string[] Animations { get; set; } = [];

    public override State Enter(State previousState)
    {
        if (AnimationPlayer is null || Animations.Length == 0)
        {
            return base.Enter(previousState);
        }

        var firstAnimation = Animations[0];

        if (string.IsNullOrEmpty(firstAnimation))
        {
            return base.Enter(previousState);
        }

        AnimationPlayer.Play(firstAnimation);

        for (int i = 1; i < Animations.Length; i++)
        {
            var animation = Animations[i];
            if (!string.IsNullOrEmpty(animation))
            {
                AnimationPlayer.Queue(animation);
            }
        }

        return base.Enter(previousState);
    }
}
