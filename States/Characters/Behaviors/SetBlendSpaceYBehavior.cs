using Godot;

namespace CrossedDimensions.States.Characters.Behaviors;

/// <summary>
/// A behavior that sets an AnimationTree 1D blend-space parameter's
/// blend_position to the character's Y velocity every frame.
/// </summary>
[GlobalClass]
public partial class SetBlendSpaceYBehavior : CharacterState
{
    [Export]
    public AnimationTree AnimationTree { get; set; }

    // The parameter name under AnimationTree.parameters (e.g. "Air").
    [Export]
    public StringName ParameterName { get; set; } = "";

    // Optional scale to apply to the velocity before writing.
    [Export]
    public float Scale { get; set; } = 1f;

    public override State Process(double delta)
    {
        if (AnimationTree is not null && CharacterContext is not null &&
            !string.IsNullOrEmpty(ParameterName.ToString()))
        {
            float blendValue = CharacterContext.Velocity.Y * Scale;
            AnimationTree.Set($"parameters/{ParameterName}/blend_position", blendValue);
        }

        return base.Process(delta);
    }
}
