using Godot;
using CrossedDimensions.Extensions;

namespace CrossedDimensions.Weapons.Behaviors;

/// <summary>
/// A weapon behavior that plays a sound effect when the weapon is fired.
/// Allows for optional random pitch variation for added audio variety.
/// </summary>
[GlobalClass]
public partial class PlaySoundBehavior : States.State
{
    [Export]
    public AudioStreamPlayer2D Sound { get; set; }

    [Export]
    public float PitchVariation { get; set; } = 0.0f;

    public override States.State Enter(States.State previousState)
    {
        if (Sound != null)
        {
            var player = Sound.PlayOneShot()
                .WithRandomPitch(PitchVariation);
        }

        return base.Enter(previousState);
    }
}
